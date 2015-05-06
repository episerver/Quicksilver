using Mediachase.Commerce.Website.Search;
using Mediachase.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Models
{
    public class FilterOptionFormModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var model = (FilterOptionFormModel)base.BindModel(controllerContext, bindingContext);
            if (model == null)
            {
                return model;
            }
            
            var query = controllerContext.HttpContext.Request.QueryString["q"];
            var sort = controllerContext.HttpContext.Request.QueryString["sort"];
            var facets = controllerContext.HttpContext.Request.QueryString["facets"];
            SetupModel(model, query, sort, facets);
            return model;
        }

        protected virtual void SetupModel(FilterOptionFormModel model, string q, string sort, string facets)
        {
            EnsurePage(model);
            EnsureQ(model, q);
            EnsureSort(model, sort);
            EnsureFacets(model, facets);
        }

        protected virtual void EnsurePage(FilterOptionFormModel model)
        {
            if (model.Page < 1)
            {
                model.Page = 1;
            }
        }

        protected virtual void EnsureQ(FilterOptionFormModel model, string q)
        {
            if (string.IsNullOrEmpty(model.Q))
            {
                model.Q = q;
            }
        }

        protected virtual void EnsureSort(FilterOptionFormModel model, string sort)
        {
            if (string.IsNullOrEmpty(model.Sort))
            {
                model.Sort = sort;
            }
        }

        protected virtual void EnsureFacets(FilterOptionFormModel model, string facets)
        {
            if (model.FacetGroups == null)
            {
                model.FacetGroups = CreateFacetGroups(facets);
            }
        }

        private static List<FacetGroupOption> CreateFacetGroups(string facets)
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
            return facetGroups;
        }

        private static SearchFilter GetSearchFilter(string facet)
        {
            return SearchFilterHelper.Current.SearchConfig.SearchFilters.FirstOrDefault(filter =>
                filter.Values.SimpleValue.Any(value =>
                    string.Equals(value.value, facet, System.StringComparison.InvariantCultureIgnoreCase)));
        }

        private static FacetGroupOption CreateFacetGroup(SearchFilter searchFilter)
        {
            return new FacetGroupOption
            {
                GroupFieldName = searchFilter.field,
                Facets = new List<FacetOption>()
            };
        }

        private static FacetOption CreateFacetOption(string facet)
        {
            return new FacetOption { Name = facet, Selected = true };
        }
    }
}