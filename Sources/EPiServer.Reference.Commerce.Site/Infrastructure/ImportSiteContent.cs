using EPiServer.Commerce.Internal.Migration.Steps;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Marketing.Promotions;
using EPiServer.Configuration;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAbstraction.RuntimeModel;
using EPiServer.DataAccess;
using EPiServer.Enterprise;
using EPiServer.Logging;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Mediachase.Commerce;
using Mediachase.Commerce.BackgroundTasks;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.ImportExport;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Extensions;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Shared;
using Mediachase.Search;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using Mediachase.Commerce.Orders.ImportExport;

namespace EPiServer.Reference.Commerce.Site.Infrastructure
{
    [ServiceConfiguration(typeof(IMigrationStep))]
    public class ImportSiteContent : IMigrationStep
    {
        private IProgressMessenger _progressMessenger;

        private Injected<IContentRepository> _contentRepository = default(Injected<IContentRepository>);
        private Injected<ReferenceConverter> _referenceConverter = default(Injected<ReferenceConverter>);
        private Injected<IDataImporter> DataImporter = default(Injected<IDataImporter>);
        private Injected<TaxImportExport> TaxImportExport = default(Injected<TaxImportExport>);

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

                _progressMessenger.AddProgressMessageText("Importing taxes...", false, 0);
                ImportTaxes();

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
        /// This method deletes all enabled payment methods and creates a new credit card payment method, 
        /// a cash on delivery payment method and a authorize - Pay by credit card payment method for 
        /// every language and associates it with all available markets.
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

                AddPaymentMethod(Guid.NewGuid(),
                    "Pay By Credit Card",
                    "Authorize",
                    "Authorize - Pay By Credit Card.",
                    "Mediachase.Commerce.Orders.CreditCardPayment, Mediachase.Commerce",
                    "Mediachase.Commerce.Plugins.Payment.Authorize.AuthorizePaymentGateway, Mediachase.Commerce.Plugins.Payment",
                    false, 3, allMarkets, language, paymentMethodDto);
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

        private void ImportTaxes()
        {
            TaxImportExport.Service.Import(Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"App_Data\Taxes.csv"), AppContext.Current.ApplicationId, null, ',');
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
            var destinationRoot = ContentReference.GlobalBlockFolder;
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Clear the cache to ensure setup is running in a controlled environment, if perhaps we're developing and have just cleared the database.
            CacheManager.Clear();

            var options = new ImportOptions {KeepIdentity = true};

            var log = DataImporter.Service.Import(stream, destinationRoot, options);

            if (log.Errors.Any())
            {
                throw new Exception("Content could not be imported. " + GetStatus(log));
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

            var contentScanner = ServiceLocator.Current.GetInstance<IContentTypeModelScanner>();
            tasks.AddRange(contentScanner.RegisterModels());
            tasks.AddRange(contentScanner.Sync(Settings.Instance.EnableModelSyncCommit));

            Task.WaitAll(tasks.ToArray());
        }


        private string GetStatus(ITransferLog log)
        {
            var logMessage = new StringBuilder();
            var lineBreak = "<br>";

            if (log.Errors.Any())
            {
                foreach (string err in log.Errors)
                {
                    logMessage.Append(err).Append(lineBreak);
                }
            }

            if (log.Warnings.Any())
            {
                foreach (string err in log.Warnings)
                {
                    logMessage.Append(err).Append(lineBreak);
                }
            }
            return logMessage.ToString();
        }

        private void ConfigureMarketing()
        {
            var campaignLink = CreateCampaigns();
            CreateBuyFromMenShoesGetDiscountPromotion(campaignLink);
            CreateSpendAmountGetDiscountPromotion(campaignLink);
            CreateBuyFromWomenShoesGetShippingDiscountPromotion(campaignLink);
        }

        private ContentReference CreateCampaigns()
        {
            var campaign = _contentRepository.Service.GetDefault<SalesCampaign>(SalesCampaignFolder.CampaignRoot);
            campaign.Name = "QuickSilver";
            campaign.Created = DateTime.UtcNow;
            campaign.IsActive = true;
            campaign.ValidFrom = DateTime.Today;
            campaign.ValidUntil = DateTime.Today.AddYears(1);
            return _contentRepository.Service.Save(campaign, SaveAction.Publish, AccessLevel.NoAccess);
        }

        private void CreateBuyFromMenShoesGetDiscountPromotion(ContentReference campaignLink)
        {
            var categoryLink = _referenceConverter.Service.GetContentLink("shoes", CatalogContentType.CatalogNode);
            var promotion = _contentRepository.Service.GetDefault<BuyQuantityGetItemDiscount>(campaignLink);
            promotion.IsActive = true;
            promotion.Name = "20 % off Mens Shoes";
            promotion.Condition.Items = new List<ContentReference> {categoryLink};
            promotion.Condition.RequiredQuantity = 1;
            promotion.DiscountTarget.Items = new List<ContentReference> {categoryLink};
            promotion.Discount.UseAmounts = false;
            promotion.Discount.Percentage = 20m;
            promotion.Banner = GetAssetUrl("/Catalog/Promotions/20% off this season's shoes");
            _contentRepository.Service.Save(promotion, SaveAction.Publish, AccessLevel.NoAccess);
        }

        private void CreateSpendAmountGetDiscountPromotion(ContentReference campaignLink)
        {
            var promotion = _contentRepository.Service.GetDefault<SpendAmountGetOrderDiscount>(campaignLink);
            promotion.IsActive = true;
            promotion.Name = "$50 off Order over $500";
            promotion.Condition.Amounts = new List<Money> {new Money(500m, Currency.USD)};
            promotion.Discount.UseAmounts = true;
            promotion.Discount.Amounts = new List<Money> {new Money(50m, Currency.USD)};
            promotion.Banner = GetAssetUrl("/Catalog/Promotions/$50 off orders over $500");
            _contentRepository.Service.Save(promotion, SaveAction.Publish, AccessLevel.NoAccess);
        }

        private void CreateBuyFromWomenShoesGetShippingDiscountPromotion(ContentReference campaignLink)
        {
            var categoryLink = _referenceConverter.Service.GetContentLink("shoes-w", CatalogContentType.CatalogNode);
            var promotion = _contentRepository.Service.GetDefault<BuyQuantityGetShippingDiscount>(campaignLink);
            promotion.IsActive = true;
            promotion.Name = "$10 off shipping from Women's Shoes";
            promotion.Condition.Items = new List<ContentReference> {categoryLink};
            promotion.ShippingMethods = GetShippingMethodIds();
            promotion.Condition.RequiredQuantity = 1;
            promotion.Discount.UseAmounts = true;
            promotion.Discount.Amounts = new List<Money> {new Money(10m, Currency.USD)};
            promotion.Banner = GetAssetUrl("/Catalog/Promotions/$10 off shipping on women's shoes");
            _contentRepository.Service.Save(promotion, SaveAction.Publish, AccessLevel.NoAccess);
        }

        private IList<Guid> GetShippingMethodIds()
        {
            var shippingMethods = new List<Guid>();
            var marketService = ServiceLocator.Current.GetInstance<IMarketService>();
            var enabledMarkets = marketService.GetAllMarkets().Where(x => x.IsEnabled).ToList();
            foreach (var language in enabledMarkets.SelectMany(x => x.Languages).Distinct())
            {
                var languageId = language.TwoLetterISOLanguageName;
                var dto = ShippingManager.GetShippingMethods(languageId);
                foreach (var shippingMethodRow in dto.ShippingMethod)
                {
                    shippingMethods.Add(shippingMethodRow.ShippingMethodId);
                }
            }

            return shippingMethods;
        }

        private ContentReference GetAssetUrl(string assetPath)
        {
            if (assetPath == null)
            {
                return null;
            }

            var slugs = assetPath.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            var pathDepth = slugs.Length;
            if (pathDepth < 1)
            {
                return null;
            }

            var currentFolder = SiteDefinition.Current.SiteAssetsRoot;
            foreach (var folderName in slugs.Take(pathDepth -1))
            {
                currentFolder = GetChildContentByName<ContentFolder>(currentFolder, folderName);
                if (currentFolder == null)
                {
                    return null;
                }
            }

            return GetChildContentByName<MediaData>(currentFolder, slugs.Last());
        }

        private ContentReference GetChildContentByName<T>(ContentReference contentLink, string name) where T : IContent
        {
            var match =
                _contentRepository.Service.GetChildren<T>(contentLink)
                    .FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return match != null ? match.ContentLink : null;
        }
    }
}