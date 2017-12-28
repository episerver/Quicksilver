using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.Search.Models;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Website.Search;
using Mediachase.Search;
using Mediachase.Search.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Search.ViewModels
{
    public class FilterOptionViewModelBinder : DefaultModelBinder
    {
        private readonly IContentLoader _contentLoader;
        private readonly LocalizationService _localizationService;
        private readonly LanguageResolver _languageResolver;

        public FilterOptionViewModelBinder(IContentLoader contentLoader, 
            LocalizationService localizationService,
            LanguageResolver languageResolver)
        {
            _contentLoader = contentLoader;
            _localizationService = localizationService;
            _languageResolver = languageResolver;
        }

        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            bindingContext.ModelName = "FilterOption";
            var model = (FilterOptionViewModel)base.BindModel(controllerContext, bindingContext);
            if (model == null)
            {
                return null;
            }

            var contentLink = controllerContext.RequestContext.GetContentLink();
            IContent content = null;
            if (!ContentReference.IsNullOrEmpty(contentLink))
            {
                content = _contentLoader.Get<IContent>(contentLink);
            }

            var query = controllerContext.HttpContext.Request.QueryString["q"];
            var sort = controllerContext.HttpContext.Request.QueryString["sort"];
            var facets = controllerContext.HttpContext.Request.QueryString["facets"];
            SetupModel(model, query, sort, facets, content);
            return model;
        }

        protected virtual void SetupModel(FilterOptionViewModel model, string q, string sort, string facets, IContent content)
        {
            EnsurePage(model);
            EnsureQ(model, q);
            EnsureSort(model, sort);
            EnsureFacets(model, facets, content);
        }

        protected virtual void EnsurePage(FilterOptionViewModel model)
        {
            if (model.Page < 1)
            {
                model.Page = 1;
            }
        }

        protected virtual void EnsureQ(FilterOptionViewModel model, string q)
        {
            if (string.IsNullOrEmpty(model.Q))
            {
                model.Q = q;
            }
        }

        protected virtual void EnsureSort(FilterOptionViewModel model, string sort)
        {
            if (string.IsNullOrEmpty(model.Sort))
            {
                model.Sort = sort;
            }
        }

        protected virtual void EnsureFacets(FilterOptionViewModel model, string facets, IContent content)
        {
            if (model.FacetGroups == null)
            {
                model.FacetGroups = CreateFacetGroups(facets, content);
            }
        }

        private List<FacetGroupOption> CreateFacetGroups(string facets, IContent content)
        {
            var facetGroups = new List<FacetGroupOption>();
            if (string.IsNullOrEmpty(facets))
            {
                return facetGroups;
            }
            foreach (var facet in facets.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                var searchFilter = GetSearchFilter(facet);
                if (searchFilter != null)
                {
                    var facetGroup = facetGroups.FirstOrDefault(fg => fg.GroupFieldName == searchFilter.field);
                    if (facetGroup == null)
                    {
                        facetGroup = CreateFacetGroup(searchFilter);
                        facetGroups.Add(facetGroup);
                    }
                    var facetOption = facetGroup.Facets.FirstOrDefault(fo => fo.Name == facet);
                    if (facetOption == null)
                    {
                        facetOption = CreateFacetOption(facet);
                        facetGroup.Facets.Add(facetOption);
                    }
                }
            }
            var nodeContent = content as NodeContent;
            if (nodeContent == null)
            {
                return facetGroups;
            }
            var filter = GetSearchFilterForNode(nodeContent);
            var selectedFilters = facets.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            var nodeFacetValues = filter.Values.SimpleValue.Where(x => selectedFilters.Any(y => y.ToLower().Equals(x.key.ToLower())));
            if (!nodeFacetValues.Any())
            {
                return facetGroups;
            }
            var nodeFacet = CreateFacetGroup(filter);

            foreach (var nodeFacetValue in nodeFacetValues)
            {
                nodeFacet.Facets.Add(CreateFacetOption(nodeFacetValue.value));
            }

            facetGroups.Add(nodeFacet);
            return facetGroups;
        }

        private SearchFilter GetSearchFilter(string facet)
        {
            return SearchFilterHelper.Current.SearchConfig.SearchFilters.FirstOrDefault(filter =>
                filter.Values.SimpleValue.Any(value =>
                    string.Equals(value.value, facet, System.StringComparison.InvariantCultureIgnoreCase)));
        }

        private FacetGroupOption CreateFacetGroup(SearchFilter searchFilter)
        {
            return new FacetGroupOption
            {
                GroupFieldName = searchFilter.field,
                Facets = new List<FacetOption>()
            };
        }

        private FacetOption CreateFacetOption(string facet)
        {
            return new FacetOption { Name = facet, Key = facet, Selected = true };
        }

        public SearchFilter GetSearchFilterForNode(NodeContent nodeContent)
        {
            var configFilter = new SearchFilter
            {
                field = BaseCatalogIndexBuilder.FieldConstants.Node,
                Descriptions = new Descriptions
                {
                    defaultLocale = _languageResolver.GetPreferredCulture().Name
                },
                Values = new SearchFilterValues()
            };

            var desc = new Description
            {
                locale = "en",
                Value = _localizationService.GetString("/Facet/Category")
            };
            configFilter.Descriptions.Description = new[] { desc };

            var nodes = _contentLoader.GetChildren<NodeContent>(nodeContent.ContentLink).ToList();
            var nodeValues = new SimpleValue[nodes.Count];
            var index = 0;
            var preferredCultureName = _languageResolver.GetPreferredCulture().Name;
            foreach (var node in nodes)
            {
                var val = new SimpleValue
                {
                    key = node.Code,
                    value = node.Code,
                    Descriptions = new Descriptions
                    {
                        defaultLocale = preferredCultureName
                    }
                };
                var desc2 = new Description
                {
                    locale = preferredCultureName,
                    Value = node.DisplayName
                };
                val.Descriptions.Description = new[] { desc2 };

                nodeValues[index] = val;
                index++;
            }
            configFilter.Values.SimpleValue = nodeValues;
            return configFilter;
        }
    }
}