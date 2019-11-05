using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Marketing;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Features.Start.ViewModels;
using EPiServer.Web.Mvc;
using Mediachase.Commerce;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Start.Controllers
{
    public class StartController : PageController<StartPage>
    {
        private readonly IContentLoader _contentLoader;
        private readonly ICurrentMarket _currentMarket;
        private readonly MarketContentLoader _marketContentFilter;

        public StartController(
            IContentLoader contentLoader,
            ICurrentMarket currentMarket,
            MarketContentLoader marketContentFilter)
        {
            _contentLoader = contentLoader;
            _currentMarket = currentMarket;
            _marketContentFilter = marketContentFilter;
        }

        public ViewResult Index(StartPage currentPage)
        {
            var viewModel = new StartPageViewModel()
            {
                StartPage = currentPage,
                Promotions = GetActivePromotions()
            };            

            return View(viewModel);
        }

        private IEnumerable<PromotionViewModel> GetActivePromotions()
        {
            var promotions = new List<PromotionViewModel>();

            var promotionItemGroups = _marketContentFilter.GetPromotionItemsForMarket(_currentMarket.GetCurrentMarket()).GroupBy(x => x.Promotion);

            foreach (var promotionGroup in promotionItemGroups)
            {
                var promotionItems = promotionGroup.First();
                promotions.Add(new PromotionViewModel()
                {
                    Name = promotionGroup.Key.Name,
                    BannerImage = promotionGroup.Key.Banner,
                    SelectionType = promotionItems.Condition.Type,
                    Items = GetProductsForPromotion(promotionItems).Take(3)
                });
            }

            return promotions;
        }

        private IEnumerable<CatalogContentBase> GetProductsForPromotion(PromotionItems itemsOnPromotion)
        {
            var conditionProducts = new List<CatalogContentBase>();

            foreach (var conditionItemReference in itemsOnPromotion.Condition.Items)
            {
                CatalogContentBase conditionItem;
                if (_contentLoader.TryGet(conditionItemReference, out conditionItem))
                {
                    AddIfProduct(conditionItem, conditionProducts);
                    var nodeContent = conditionItem as NodeContentBase;
                    if (nodeContent != null)
                    {
                        AddItemsRecursive(nodeContent, itemsOnPromotion, conditionProducts);
                    }
                }
            }

            return conditionProducts;
        }

        private void AddItemsRecursive(NodeContentBase nodeContent, PromotionItems itemsOnPromotion, List<CatalogContentBase> conditionProducts)
        {
            foreach (var child in _contentLoader.GetChildren<CatalogContentBase>(nodeContent.ContentLink))
            {
                AddIfProduct(child, conditionProducts);

                var childNode = child as NodeContentBase;
                if (childNode != null && itemsOnPromotion.Condition.IncludesSubcategories)
                {
                    AddItemsRecursive(childNode, itemsOnPromotion, conditionProducts);
                }
            }
        }

        private static void AddIfProduct(CatalogContentBase content, List<CatalogContentBase> productsInPromotion)
        {
            if (content is ProductContent)
            {
                productsInPromotion.Add(content);
            }
        }
    }
}