using Castle.Core.Internal;
using EPiServer.Commerce.Internal.Migration.Steps;
using EPiServer.Configuration;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAbstraction.RuntimeModel;
using EPiServer.Enterprise;
using EPiServer.Enterprise.Transfer;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.BackgroundTasks;
using Mediachase.Commerce.Catalog.ImportExport;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Extensions;
using Mediachase.Commerce.Marketing;
using Mediachase.Commerce.Marketing.Dto;
using Mediachase.Commerce.Marketing.Managers;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Shared;
using Mediachase.Search;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace EPiServer.Reference.Commerce.Site.Infrastructure
{
    [ServiceConfiguration(typeof(IMigrationStep))]
    public class ImportSiteContent : IMigrationStep
    {
        private IProgressMessenger _progressMessenger;

        public int Order
        {
            get { return 1000; }
        }

        public string Name { get { return "Quicksilver content"; } }
        public string Description { get { return "Import catalog, assets, payment methods, shipping methods, and promotions for Quicksilver"; } }

        public bool Execute(IProgressMessenger progressMessenger)
        {
            _progressMessenger = progressMessenger;
            try
            {
                _progressMessenger.AddProgressMessageText("Importing product assets...", false, 0);
                ImportAssets(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"App_Data\ProductAssets.episerverdata"));

                _progressMessenger.AddProgressMessageText("Enabling currencies...", false, 0);
                EnableCurrencies();

                _progressMessenger.AddProgressMessageText("Importing catalog...", false, 0);
                ImportCatalog(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"App_Data\Catalog_Fashion.zip"));

                _progressMessenger.AddProgressMessageText("Disabling default market...", false, 0);
                DisableDefaultMarket();

                _progressMessenger.AddProgressMessageText("Configuring payment methods", false, 0);
                ConfigurePaymentMethods();

                _progressMessenger.AddProgressMessageText("Configuring shipment methods", false, 0);
                ConfigureShippingMethods();

                _progressMessenger.AddProgressMessageText("Creating marketing campaigns and Promotions", false, 0);
                ConfigureMarketing();

                _progressMessenger.AddProgressMessageText("Rebuilding index...", false, 0);
                BuildIndex(_progressMessenger, AppContext.Current.ApplicationId, AppContext.Current.ApplicationName, true);
                _progressMessenger.AddProgressMessageText("Done rebuilding index", false, 0);

                return true;
            }
            catch (Exception ex)
            {
                _progressMessenger.AddProgressMessageText("ImportSiteContent failed: " + ex.Message + "Stack trace:" + ex.StackTrace, true, 0);
                LogManager.GetLogger(GetType()).Error(ex.Message, ex.StackTrace);
                return false;
            }
        }

        private void EnableCurrencies()
        {
            var c = new CurrencySetup();
            c.CreateConversions();
        }

        /// <summary>
        /// This method deletes all enabled payment methods and creates a new credit card payment method and a cash on delivery payment method for every language
        /// and associates it with all available markets.
        /// </summary>
        /// <remarks>
        /// This will ensure we have at least two different working payment methods available.
        /// Probably not a real world scenario but for our testing purposes it is fine =)
        /// </remarks>
        private void ConfigurePaymentMethods()
        {
            var marketService = ServiceLocator.Current.GetInstance<IMarketService>();
            var allMarkets = marketService.GetAllMarkets().Where(x => x.IsEnabled).ToList();
            foreach (var language in allMarkets.SelectMany(x => x.Languages).Distinct())
            {
                var paymentMethodDto = PaymentManager.GetPaymentMethods(language.TwoLetterISOLanguageName);
                foreach (var method in paymentMethodDto.PaymentMethod)
                {
                    method.Delete();
                }
                PaymentManager.SavePayment(paymentMethodDto);

                AddPaymentMethod(Guid.NewGuid(),
                    "Credit card",
                    "GenericCreditCard",
                    "Credit card payment",
                    "Mediachase.Commerce.Orders.CreditCardPayment, Mediachase.Commerce",
                    "EPiServer.Reference.Commerce.Shared.GenericCreditCardPaymentGateway, EPiServer.Reference.Commerce.Shared",
                    true, 1, allMarkets, language, paymentMethodDto);

                AddPaymentMethod(Guid.NewGuid(),
                    "Cash on delivery",
                    "CashOnDelivery",
                    "The payment is settled as part of the order delivery.",
                    "Mediachase.Commerce.Orders.OtherPayment, Mediachase.Commerce",
                    "Mediachase.Commerce.Plugins.Payment.GenericPaymentGateway, Mediachase.Commerce.Plugins.Payment",
                    false, 2, allMarkets, language, paymentMethodDto);
            }
        }

        /// <summary>
        /// Adds a payment method for a specific language.
        /// </summary>
        /// <param name="id">The ID of the payment method.</param>
        /// <param name="name">The name of the payment method.</param>
        /// <param name="systemKeyword">The system name of the payment method.</param>
        /// <param name="description">A description of the payment method.</param>
        /// <param name="implementationClass"></param>
        /// <param name="gatewayClass"></param>
        /// <param name="isDefault">Indicate whether it should be the default method or not.</param>
        /// <param name="orderIndex">The ordering index when the method is listed.</param>
        /// <param name="markets">All markets the method should be associated with.</param>
        /// <param name="language">The language for the payment method.</param>
        /// <param name="paymentMethodDto">The dataset used for creating new payment method rows.</param>
        private static void AddPaymentMethod(Guid id, string name, string systemKeyword, string description, string implementationClass, string gatewayClass, 
            bool isDefault, int orderIndex, IEnumerable<IMarket> markets, CultureInfo language, PaymentMethodDto paymentMethodDto)
        {
            var row = paymentMethodDto.PaymentMethod.AddPaymentMethodRow(id, name, description, language.TwoLetterISOLanguageName,
                            systemKeyword, true, isDefault, gatewayClass,
                            implementationClass, false, orderIndex, DateTime.Now, DateTime.Now, AppContext.Current.ApplicationId);

            var paymentMethod = new PaymentMethod(row);
            paymentMethod.MarketId.AddRange(markets.Where(x => x.IsEnabled && x.Languages.Contains(language)).Select(x => x.MarketId));
            paymentMethod.SaveChanges();
        }

        /// <summary>
        /// This method deletes the default shipping methods (while keeping the shipping options) and creates three shipping methods
        /// for each combination of language and currency and associates it with all available markets.
        /// </summary>
        /// <remarks>
        /// Note that this can easily result in a combinatorial explosion (languages * currencies * 3) but it is a side-effect of
        /// how the site is set up with selection of market, language and currency. In most cases the set of markets & languages
        /// will be highly restricted so the combinatorial effect will probably not be an issue.
        /// </remarks>
        private void ConfigureShippingMethods()
        {
            var marketService = ServiceLocator.Current.GetInstance<IMarketService>();
            var enabledMarkets = marketService.GetAllMarkets().Where(x => x.IsEnabled).ToList();
            foreach (var language in enabledMarkets.SelectMany(x => x.Languages).Distinct())
            {
                var languageId = language.TwoLetterISOLanguageName;
                var dto = ShippingManager.GetShippingMethods(languageId);
                DeleteShippingMethods(dto);
                ShippingManager.SaveShipping(dto);

                var marketsForCurrentLanguage = enabledMarkets.Where(x => x.Languages.Contains(language));
                var shippingSet = CreateShippingMethodsForLanguageAndCurrencies(dto, marketsForCurrentLanguage, languageId);
                ShippingManager.SaveShipping(dto);

                AssociateShippingMethodWithMarkets(dto, marketsForCurrentLanguage, shippingSet);
                ShippingManager.SaveShipping(dto);
            }
        }

        private void DeleteShippingMethods(ShippingMethodDto dto)
        {
            foreach (var method in dto.ShippingMethod)
            {
                method.Delete();
            }
        }

        private IEnumerable<ShippingMethodDto.ShippingMethodRow> CreateShippingMethodsForLanguageAndCurrencies(ShippingMethodDto dto, IEnumerable<IMarket> markets, string languageId)
        {
            var shippingOption = dto.ShippingOption.First(x => x.Name == "Generic Gateway");
            var shippingMethods = new List<ShippingMethodDto.ShippingMethodRow>();
            var sortOrder = 1;

            var usdCostExpress = new Money(20, Currency.USD);
            var usdCostFast = new Money(15, Currency.USD);
            var usdCostRegular = new Money(5, Currency.USD);

            foreach (var currency in markets.SelectMany(m => m.Currencies).Distinct())
            {
                shippingMethods.Add(CreateShippingMethod(dto, shippingOption, languageId, sortOrder++, "Express-" + currency, string.Format("Express {0} (1 day)({1})", currency, languageId), usdCostExpress, currency));
                shippingMethods.Add(CreateShippingMethod(dto, shippingOption, languageId, sortOrder++, "Fast-" + currency, string.Format("Fast {0} (2-3 days)({1})", currency, languageId), usdCostFast, currency));
                shippingMethods.Add(CreateShippingMethod(dto, shippingOption, languageId, sortOrder++, "Regular-" + currency, string.Format("Regular {0} (4-7 days)({1})", currency, languageId), usdCostRegular, currency));
            }

            return shippingMethods;
        }

        private ShippingMethodDto.ShippingMethodRow CreateShippingMethod(ShippingMethodDto dto, ShippingMethodDto.ShippingOptionRow shippingOption, string languageId, int sortOrder, string name, string description, Money costInUsd, Currency currency)
        {
            Money shippingCost = CurrencyFormatter.ConvertCurrency(costInUsd, currency);
            if (shippingCost.Currency != currency)
            {
                throw new InvalidOperationException("Cannot convert to currency " + currency + " Missing conversion data.");
            }
            return dto.ShippingMethod.AddShippingMethodRow(
                Guid.NewGuid(),
                shippingOption,
                AppContext.Current.ApplicationId,
                languageId,
                true,
                name,
                "",
                shippingCost.Amount,
                shippingCost.Currency,
                description,
                false,
                sortOrder,
                DateTime.Now,
                DateTime.Now);
        }

        private void AssociateShippingMethodWithMarkets(ShippingMethodDto dto, IEnumerable<IMarket> markets, IEnumerable<ShippingMethodDto.ShippingMethodRow> shippingSet)
        {
            foreach (var shippingMethod in shippingSet)
            {
                foreach (var market in markets.Where(m => m.Currencies.Contains(shippingMethod.Currency)))
                {
                    dto.MarketShippingMethods.AddMarketShippingMethodsRow(market.MarketId.Value, shippingMethod);
                }
            }
        }

        private void DisableDefaultMarket()
        {
            var marketService = ServiceLocator.Current.GetInstance<IMarketService>();

            // Disable default market
            _progressMessenger.AddProgressMessageText("Disabling default market...", false, 0);
            var defaultMarket = new MarketImpl(marketService.GetMarket(MarketId.Default)) { IsEnabled = false };
            marketService.UpdateMarket(defaultMarket);
        }
        
        private void ImportAssets(string path)
        {
            var importer = new DataImporter { DestinationRoot = ContentReference.GlobalBlockFolder };
            importer.Stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Clear the cache to ensure setup is running in a controlled environment, if perhaps we're developing and have just cleared the database.
            CacheManager.Clear();
            importer.KeepIdentity = true;
            importer.Import();

            if (importer.Log.Errors.Count > 0)
            {
                throw new Exception("Content could not be imported. " + GetStatus(importer));
            }
        }

        private void ImportCatalog(string path)
        {
            var importJob = new ImportJob(AppContext.Current.ApplicationId, path, "Catalog.xml", true);

            Action importCatalog = () =>
            {
                _progressMessenger.AddProgressMessageText("Importing Catalog content...", false, 20);
                Action<IBackgroundTaskMessage> addMessage = msg =>
                {
                    var isError = msg.MessageType == BackgroundTaskMessageType.Error;
                    var percent = (int)Math.Round(msg.GetOverallProgress() * 100);
                    var message = msg.Exception == null
                        ? msg.Message
                        : string.Format("{0} {1}", msg.Message, msg.ExceptionMessage);
                    _progressMessenger.AddProgressMessageText(message, isError, percent);
                };
                importJob.Execute(addMessage, CancellationToken.None);
                _progressMessenger.AddProgressMessageText("Done importing Catalog content", false, 60);
            };

            importCatalog();

            //We are running in front-end site context, the metafield update events are ignored, we need to sync manually
            _progressMessenger.AddProgressMessageText("Syncing metaclasses with content types", false, 60);
            SyncMetaClassesToContentTypeModels();
            _progressMessenger.AddProgressMessageText("Done syncing metaclasses with content types", false, 70);
        }

        private void BuildIndex(IProgressMessenger progressMessenger, Guid applicationId, string applicationName, bool rebuild)
        {
            var searchManager = new SearchManager(applicationName);
            searchManager.SearchIndexMessage += SearchManager_SearchIndexMessage;
            searchManager.BuildIndex(rebuild);
        }

        private void SearchManager_SearchIndexMessage(object source, SearchIndexEventArgs args)
        {
            // The whole index building process would take 20% (from 70 to 90) of the import process. Then the percent value here should be calculated based on 20%
            var percent = 70 + Convert.ToInt32(args.CompletedPercentage) * 2 / 10;
            _progressMessenger.AddProgressMessageText(args.Message, false, percent);
        }

        /// <summary>
        /// Synchronizes the meta classes to content type models.
        /// The synchronization will be done when site starts up. 
        /// To avoid restarting a site, we do the models synchronization manually.
        /// </summary>
        private static void SyncMetaClassesToContentTypeModels()
        {
            var cachedRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>() as ICachedRepository;
            if (cachedRepository != null)
            {
                cachedRepository.ClearCache();
            }

            cachedRepository = ServiceLocator.Current.GetInstance<IPropertyDefinitionRepository>() as ICachedRepository;
            if (cachedRepository != null)
            {
                cachedRepository.ClearCache();
            }

            var tasks = new List<Task>();

            var contentScanner = ServiceLocator.Current.GetInstance<ContentTypeModelScanner>();
            tasks.AddRange(contentScanner.RegisterModels());
            tasks.AddRange(contentScanner.Sync(Settings.Instance.EnableModelSyncCommit));

            Task.WaitAll(tasks.ToArray());
        }


        private string GetStatus(ITransferContext importer)
        {
            var logMessage = new StringBuilder();
            var lineBreak = "<br>";

            if (importer.Log.Errors.Count > 0)
            {
                foreach (string err in importer.Log.Errors)
                {
                    logMessage.Append(err).Append(lineBreak);
                }
            }

            if (importer.Log.Warnings.Count > 0)
            {
                foreach (string err in importer.Log.Warnings)
                {
                    logMessage.Append(err).Append(lineBreak);
                }
            }
            return logMessage.ToString();
        }

        private void ConfigureMarketing()
        {
            CreatePromotions(CreateCampaigns());
        }

        private int CreateCampaigns()
        {
            var dto = new CampaignDto();
            var campaignRow = dto.Campaign.NewCampaignRow();
            campaignRow.Name = "QuickSilver";
            campaignRow.Created = DateTime.UtcNow;
            campaignRow.IsArchived = false;
            campaignRow.ApplicationId = AppContext.Current.ApplicationId;
            campaignRow.Modified = DateTime.UtcNow;
            campaignRow.IsActive = true;
            campaignRow.StartDate = DateTime.Today;
            campaignRow.EndDate = DateTime.Today.AddYears(1);
            dto.Campaign.AddCampaignRow(campaignRow);
            CampaignManager.SaveCampaign(dto);
            return dto.Campaign.First().CampaignId;
        }

        private void CreatePromotions(int campaignId)
        {
            var dto = new PromotionDto();

            CreatePromotion(dto, "25 % off Mens Shoes", 0.00m, PromotionType.Percentage, "EntryCustomDiscount", "entry", campaignId);
            CreatePromotion(dto, "$50 off Order over $500", 50.00m, PromotionType.ValueBased, "OrderVolumeDiscount", "order", campaignId);
            CreatePromotion(dto, "$10 off shipping from Women's Shoes", 10.00m, PromotionType.ValueBased, "BuySKUFromCategoryXGetDiscountedShipping", "shipping", campaignId);
            PromotionManager.SavePromotion(dto);
            dto = PromotionManager.GetPromotionDto();
            foreach (var promotion in dto.Promotion)
            {
                foreach (var languageCode in new[] { "en", "sv" })
                {
                    var name = String.Empty;
                    switch (promotion.PromotionId)
                    {
                        case 1:
                            name = languageCode.Equals("en") ? "25 % off Mens Shoes" : "25% rabatt på herrskor";
                            break;
                        case 2:
                            name = languageCode.Equals("en") ? "$50 off Order over $500" : "$50 rabatt på order över $500";
                            break;
                        case 3:
                            name = languageCode.Equals("en") ? "$10 off shipping from Women's Shoes" : "$10 rabatt på frakt av Damskor";
                            break;
                    }
                    var promoLanguageRow = dto.PromotionLanguage.NewPromotionLanguageRow();
                    promoLanguageRow.PromotionId = promotion.PromotionId;
                    promoLanguageRow.LanguageCode = languageCode;
                    promoLanguageRow.DisplayName = name;
                    dto.PromotionLanguage.Rows.Add(promoLanguageRow);
                }
            }
            PromotionManager.SavePromotion(dto);
            UpdatePromotionParams();
            CreateExpression(1, "25 % off Mens Shoes");
            CreateExpression(2, "$50 off Order over $500");
            CreateExpression(3, "$10 off shipping from Women's Shoes");

        }

        private void CreatePromotion(PromotionDto dto, string name, decimal reward, PromotionType rewardType, string promotionType, string promotionGroup, int campaignId)
        {
            var promotionRow = dto.Promotion.NewPromotionRow();
            promotionRow.ApplicationId = AppContext.Current.ApplicationId;
            promotionRow.Name = name;
            promotionRow.StartDate = DateTime.Today;
            promotionRow.EndDate = DateTime.Today.AddYears(1);
            promotionRow.Created = DateTime.UtcNow;
            promotionRow.ModifiedBy = "admin";
            promotionRow.Status = "active";
            promotionRow.OfferAmount = reward;
            promotionRow.OfferType = (int)rewardType;
            promotionRow.PromotionGroup = promotionGroup;
            promotionRow.CampaignId = campaignId;
            promotionRow.ExclusivityType = "none";
            promotionRow.PromotionType = promotionType;
            promotionRow.PerOrderLimit = 0;
            promotionRow.ApplicationLimit = 0;
            promotionRow.CustomerLimit = 0;
            promotionRow.Priority = 1;
            promotionRow.CouponCode = "";
            if (name.Equals("25 % off Mens Shoes"))
            {
                promotionRow.OfferAmount = 25m;
                promotionRow.OfferType = 0;
            }
            //In commerce manager, this promotion type is displayed quite differently from others. 
            //The percentage based offer type has value "0" and value based offer type has value "1". 
            //But in C# code, PromotionType.Percentage = 1 and PromotionType.ValueBased = 2
            else if (name.Equals("$10 off shipping from Women's Shoes"))
            {
                promotionRow.OfferType = 1;
            }
            dto.Promotion.Rows.Add(promotionRow);
            return;
        }

        private void UpdatePromotionParams()
        {
            var sql = "EXEC(N'UPDATE [dbo].[Promotion] SET Params = 0x0001000000ffffffff01000000000000000c02000000534d6564696163686173652e436f6e736f6c654d616e616765722c2056657273696f6e3d382e31312e342e3635362c2043756c747572653d6e65757472616c2c205075626c69634b6579546f6b656e3d6e756c6c0c03000000634d6564696163686173652e427573696e657373466f756e646174696f6e2c2056657273696f6e3d382e31312e342e3635362c2043756c747572653d6e65757472616c2c205075626c69634b6579546f6b656e3d34316432653761363135626132383663050100000036417070735f4d61726b6574696e675f50726f6d6f74696f6e735f456e747279437573746f6d446973636f756e742b53657474696e67730200000013436f6e646974696f6e45787072657373696f6e1054617267657445787072657373696f6e04043c4d6564696163686173652e427573696e657373466f756e646174696f6e2e46696c74657245787072657373696f6e4e6f6465436f6c6c656374696f6e030000003c4d6564696163686173652e427573696e657373466f756e646174696f6e2e46696c74657245787072657373696f6e4e6f6465436f6c6c656374696f6e03000000020000000904000000090500000005040000003c4d6564696163686173652e427573696e657373466f756e646174696f6e2e46696c74657245787072657373696f6e4e6f6465436f6c6c656374696f6e020000000b5f706172656e744e6f646512436f6c6c656374696f6e60312b6974656d730403324d6564696163686173652e427573696e657373466f756e646174696f6e2e46696c74657245787072657373696f6e4e6f646503000000bc0153797374656d2e436f6c6c656374696f6e732e47656e657269632e4c69737460315b5b4d6564696163686173652e427573696e657373466f756e646174696f6e2e46696c74657245787072657373696f6e4e6f64652c204d6564696163686173652e427573696e657373466f756e646174696f6e2c2056657273696f6e3d382e31312e342e3635362c2043756c747572653d6e65757472616c2c205075626c69634b6579546f6b656e3d343164326537613631356261323836635d5d0300000009060000000907000000010500000004000000090800000009090000000c0a0000004953797374656d2c2056657273696f6e3d342e302e302e302c2043756c747572653d6e65757472616c2c205075626c69634b6579546f6b656e3d623737613563353631393334653038390506000000324d6564696163686173652e427573696e657373466f756e646174696f6e2e46696c74657245787072657373696f6e4e6f64650b0000000b5f617474726962757465730b5f6368696c644e6f6465730c5f6465736372697074696f6e045f6b65790b5f706172656e744e6f6465095f726561646f6e6c79055f6e616d650a5f636f6e646974696f6e065f76616c7565095f6e6f646554797065075f6d6574686f6404040101040001040204043253797374656d2e436f6c6c656374696f6e732e5370656369616c697a65642e4e616d6556616c7565436f6c6c656374696f6e0a0000003c4d6564696163686173652e427573696e657373466f756e646174696f6e2e46696c74657245787072657373696f6e4e6f6465436f6c6c656374696f6e03000000324d6564696163686173652e427573696e657373466f756e646174696f6e2e46696c74657245787072657373696f6e4e6f646503000000012e4d6564696163686173652e427573696e657373466f756e646174696f6e2e436f6e646974696f6e456c656d656e7403000000364d6564696163686173652e427573696e657373466f756e646174696f6e2e46696c74657245787072657373696f6e4e6f646554797065030000002b4d6564696163686173652e427573696e657373466f756e646174696f6e2e4d6574686f64456c656d656e7403000000030000000a0904000000060c00000000060d0000002430303030303030302d303030302d303030302d303030302d3030303030303030303030300a01090c0000000a0a05f1ffffff364d6564696163686173652e427573696e657373466f756e646174696f6e2e46696c74657245787072657373696f6e4e6f646554797065010000000776616c75655f5f000803000000020000000a0407000000bc0153797374656d2e436f6c6c656374696f6e732e47656e657269632e4c69737460315b5b4d6564696163686173652e427573696e657373466f756e646174696f6e2e46696c74657245787072657373696f6e4e6f64652c204d6564696163686173652e427573696e657373466f756e646174696f6e2c2056657273696f6e3d382e31312e342e3635362c2043756c747572653d6e65757472616c2c205075626c69634b6579546f6b656e3d343164326537613631356261323836635d5d03000000065f6974656d73055f73697a65085f76657273696f6e040000344d6564696163686173652e427573696e657373466f756e646174696f6e2e46696c74657245787072657373696f6e4e6f64655b5d030000000808091000000001000000010000000108000000060000000a0905000000090c00000006130000002430303030303030302d303030302d303030302d303030302d3030303030303030303030300a01090c0000000a0a01ebfffffff1ffffff020000000a01090000000700000009160000000100000001000000071000000000010000000400000004324d6564696163686173652e427573696e657373466f756e646174696f6e2e46696c74657245787072657373696f6e4e6f64650300000009170000000d03071600000000010000000400000004324d6564696163686173652e427573696e657373466f756e646174696f6e2e46696c74657245787072657373696f6e4e6f64650300000009180000000d030117000000060000000a0a0a0619000000267b37373731443737372d453332372d343230302d383738442d4145383446443039453642437d090600000000061b0000001a5461726765744c696e654974656d2e436174616c6f674e6f6465091c000000061d0000000573686f657301e2fffffff1ffffff000000000a011800000006000000091f0000000a0a0620000000267b37314346374533322d354631422d346237312d424330392d3733453734423333323837397d09080000000006220000000e6765742025206f6666206974656d09230000000806000000000000394001dcfffffff1ffffff000000000a051c0000002e4d6564696163686173652e427573696e657373466f756e646174696f6e2e436f6e646974696f6e456c656d656e7407000000045f6b6579055f6e616d650c5f6465736372697074696f6e055f74797065125f637573746f6d436f6e74726f6c50617468125f637573746f6d436f6e74726f6c54797065065f6974656d7301010104010104324d6564696163686173652e427573696e657373466f756e646174696f6e2e436f6e646974696f6e456c656d656e7454797065030000003b4d6564696163686173652e427573696e657373466f756e646174696f6e2e436f6e646974696f6e53656c6563744974656d436f6c6c656374696f6e03000000030000000625000000267b41383331414132302d353244372d346162372d384635382d3644333735353241334237317d06260000000d457175616c7320285465787429092600000005d9ffffff324d6564696163686173652e427573696e657373466f756e646174696f6e2e436f6e646974696f6e456c656d656e7454797065010000000776616c75655f5f000803000000ff0000000628000000347e2f417070732f4d61726b6574696e672f45787072657373696f6e46756e6374696f6e732f5465787446696c7465722e617363780a0a051f0000003253797374656d2e436f6c6c656374696f6e732e5370656369616c697a65642e4e616d6556616c7565436f6c6c656374696f6e0700000008526561644f6e6c790c4861736850726f766964657208436f6d706172657205436f756e74044b6579730656616c7565730756657273696f6e00030300060500013253797374656d2e436f6c6c656374696f6e732e43617365496e73656e73697469766548617368436f646550726f76696465722a53797374656d2e436f6c6c656374696f6e732e43617365496e73656e736974697665436f6d706172657208080a000000000929000000092a00000001000000092b000000092c0000000200000001230000001c000000062d000000267b41394244334637362d444642452d346633612d393031332d3038343230423545334439387d062e00000006726577617264092e00000001d1ffffffd9ffffffff00000006300000003e7e2f417070732f4d61726b6574696e672f45787072657373696f6e46756e6374696f6e732f446563696d616c50657263656e7446696c7465722e617363780a0a04290000003253797374656d2e436f6c6c656374696f6e732e43617365496e73656e73697469766548617368436f646550726f766964657201000000066d5f74657874031d53797374656d2e476c6f62616c697a6174696f6e2e54657874496e666f0931000000042a0000002a53797374656d2e436f6c6c656374696f6e732e43617365496e73656e736974697665436f6d7061726572010000000d6d5f636f6d70617265496e666f032053797374656d2e476c6f62616c697a6174696f6e2e436f6d70617265496e666f0932000000112b0000000100000006330000001e66696c74657245787072657373696f6e4368696c64456e61626c654b6579102c00000001000000093400000004310000001d53797374656d2e476c6f62616c697a6174696f6e2e54657874496e666f070000000f6d5f6c697374536570617261746f720c6d5f6973526561644f6e6c790d6d5f63756c747572654e616d6511637573746f6d43756c747572654e616d650b6d5f6e446174614974656d116d5f757365557365724f766572726964650d6d5f77696e33324c616e67494401000101000000010801080635000000012c01090c000000090c00000000000000007f00000004320000002053797374656d2e476c6f62616c697a6174696f6e2e436f6d70617265496e666f04000000066d5f6e616d650977696e33324c4349440763756c747572650d6d5f536f727456657273696f6e0100000308082053797374656d2e476c6f62616c697a6174696f6e2e536f727456657273696f6e090c000000000000007f0000000a04340000001c53797374656d2e436f6c6c656374696f6e732e41727261794c69737403000000065f6974656d73055f73697a65085f76657273696f6e05000008080937000000010000000100000010370000000100000006380000000546616c73650b000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 where PromotionId = 1')";
            ExecuteSql(sql);
            sql = "UPDATE [dbo].[Promotion] SET Params = 0x3c3f786d6c2076657273696f6e3d22312e30223f3e0d0a3c53657474696e677320786d6c6e733a7873693d22687474703a2f2f7777772e77332e6f72672f323030312f584d4c536368656d612d696e7374616e63652220786d6c6e733a7873643d22687474703a2f2f7777772e77332e6f72672f323030312f584d4c536368656d61223e0d0a20203c526577617264547970653e57686f6c654f726465723c2f526577617264547970653e0d0a20203c416d6f756e744f66663e35303c2f416d6f756e744f66663e0d0a20203c416d6f756e74547970653e56616c75653c2f416d6f756e74547970653e0d0a20203c4d696e4f72646572416d6f756e743e3530303c2f4d696e4f72646572416d6f756e743e0d0a3c2f53657474696e67733e where PromotionId = 2";
            ExecuteSql(sql);
            sql = "UPDATE [dbo].[Promotion] SET Params = 0x3c3f786d6c2076657273696f6e3d22312e30223f3e0d0a3c53657474696e677320786d6c6e733a7873693d22687474703a2f2f7777772e77332e6f72672f323030312f584d4c536368656d612d696e7374616e63652220786d6c6e733a7873643d22687474703a2f2f7777772e77332e6f72672f323030312f584d4c536368656d61223e0d0a20203c43617465676f7279436f64653e73686f65732d773c2f43617465676f7279436f64653e0d0a20203c4d696e696d756d5175616e746974793e313c2f4d696e696d756d5175616e746974793e0d0a20203c5368697070696e674d6574686f6449643e66633763326435332d376331632d343239382d383138392d6638623166386538353433393c2f5368697070696e674d6574686f6449643e0d0a20203c526577617264547970653e57686f6c654f726465723c2f526577617264547970653e0d0a20203c416d6f756e744f66663e31303c2f416d6f756e744f66663e0d0a20203c416d6f756e74547970653e56616c75653c2f416d6f756e74547970653e0d0a3c2f53657474696e67733e where PromotionId = 3";
            ExecuteSql(sql);
        }

        private void CreateExpression(int promotionId, string name)
        {
            var xml = String.Empty;
            if (name.Equals("25 % off Mens Shoes"))
            {
                using (var sr = new StreamReader(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"App_Data\Promotions\25_Percent_off_Mens_Shoes.xml")))
                {
                    xml = sr.ReadToEnd();
                }

            }
            else if (name.Equals("$50 off Order over $500"))
            {
                using (var sr = new StreamReader(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"App_Data\Promotions\50_off_Order_over_500.xml")))
                {
                    xml = sr.ReadToEnd();
                }
            }
            else if (name.Equals("$10 off shipping from Women's Shoes"))
            {
                using (var sr = new StreamReader(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"App_Data\Promotions\10_off_shipping_from_Womens_Shoes.xml")))
                {
                    xml = sr.ReadToEnd();
                }
                xml = xml.Replace("fc7c2d53-7c1c-4298-8189-f8b1f8e85439", ShippingManager.GetShippingMethods("en").ShippingMethod.FirstOrDefault(x => x.LanguageId.Equals("en") && x.Name.Contains("Express") && x.Currency.Equals("USD")).ShippingMethodId.ToString().ToLower());
            }
            xml = xml.Replace("'", "''");
            var sql = String.Format("INSERT INTO [dbo].[Expression] ([ApplicationId], [Name], [Description], [Category], [ExpressionXml], [Created], [Modified], [ModifiedBy]) VALUES (N'{0}', N'{1}', N'{1}', N'Promotion', '{2}', N'20150430 09:55:05.570', NULL, N'admin')", AppContext.Current.ApplicationId, name.Replace("'", "''"), xml);
            ExecuteSql(sql);
            sql = String.Format("INSERT INTO [dbo].[PromotionCondition] ([PromotionId], [ExpressionId], [CatalogName], [CatalogNodeId], [CatalogEntryId]) VALUES ({0}, {0}, NULL, NULL, NULL)", promotionId);
            ExecuteSql(sql);
        }

        private void ExecuteSql(String sql)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["EcfSqlConnection"].ConnectionString))
            {
                var command = new SqlCommand(sql, connection)
                {
                    CommandType = CommandType.Text
                };
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }

}
