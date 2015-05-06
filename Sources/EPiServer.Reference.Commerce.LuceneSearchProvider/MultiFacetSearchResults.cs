using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Mediachase.Commerce.Website.Search;
using Mediachase.Search;
using Mediachase.Search.Extensions;
using Lucene.Net.Util;
using Mediachase.Search.Providers.Lucene;


namespace EPiServer.Reference.Commerce.LuceneSearchProvider
{
    public class MultiFacetSearchResults : SearchResults
    {
        private readonly MultiFacetQueryBuilder _queryBuilder;

        private FacetGroup[] _facetGroups;

        /// <summary>
        /// Gets the facet groups.
        /// </summary>
        /// <value>The facet groups.</value>
        public override ISearchFacetGroup[] FacetGroups
        {
            get { return _facetGroups; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiFacetSearchResults"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="topDocs">The top docs.</param>
        /// <param name="criteria">The criteria.</param>
        /// <param name="query">The query.</param>
        /// <param name="queryBuilder">The query builder.</param>
        /// <param name="simulateFaceting">if set to <c>true</c> [simulate faceting].</param>
        public MultiFacetSearchResults(IndexReader reader, TopDocs topDocs, ISearchCriteria criteria, Query query, MultiFacetQueryBuilder queryBuilder, bool simulateFaceting)
            : base(null, criteria)
        {
            _queryBuilder = queryBuilder;
            PopulateDocuments(reader, topDocs, criteria);
            if (simulateFaceting)
            {
                PopulateFacets(reader, query);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiFacetSearchResults"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="topDocs">The top docs.</param>
        /// <param name="criteria">The criteria.</param>
        /// <param name="query">The query.</param>
        /// <param name="nonFilteredQuery">The non filtered query.</param>
        /// <param name="queryBuilder">The query builder.</param>
        public MultiFacetSearchResults(IndexReader reader, TopDocs topDocs, ISearchCriteria criteria, Query query,  MultiFacetQueryBuilder queryBuilder)
            : this(reader, topDocs, criteria, query, queryBuilder, true) 
        {
        }

        /// <summary>
        /// Populates the documents.
        /// </summary>
        /// <param name="reader">The <see cref="IndexReader"/> object</param>
        /// <param name="topDocs">The topDocs.</param>
        private void PopulateDocuments(IndexReader reader, TopDocs topDocs, ISearchCriteria criteria)
        {
            if (topDocs == null)
            {
                return;
            }
           
            var totalCount = topDocs.TotalHits;
            var recordsToRetrieve = SearchCriteria.RecordsToRetrieve;
            var startIndex = SearchCriteria.StartingRecord;
            if (recordsToRetrieve > totalCount)
            {
                recordsToRetrieve = totalCount;
            }

            var searchDocuments = new SearchDocuments();
            for (var index = startIndex; index < startIndex + recordsToRetrieve; index++)
            {
                if (index >= totalCount || index >= topDocs.ScoreDocs.Length)
                {
                    break;
                }

                var searchDoc = new SearchDocument();
                foreach (var field in reader.Document(topDocs.ScoreDocs[index].Doc).GetFields())
                {
                    searchDoc.Add(new SearchField(field.Name, field.StringValue));
                }

                searchDocuments.Add(searchDoc);
            }

            searchDocuments.TotalCount = topDocs.TotalHits;
            base.Documents = searchDocuments;
        }

        /// <summary>
        /// Populates the facets.
        /// </summary>
        private void PopulateFacets(IndexReader reader, Query query)
        {
            var groups = new List<FacetGroup>();
            var criteria = SearchCriteria as MultiFacetSearchCriteria;
            if (criteria == null)
            {
                return;
            }
            
            foreach (var searchFilter in SearchCriteria.Filters)
            {
                var filteredQuery = _queryBuilder.AddSelectedFilters(criteria, query as BooleanQuery, searchFilter.field);
                var filter = new CachingWrapperFilter(new QueryWrapperFilter(filteredQuery));
                var baseDocIdSet = filter.GetDocIdSet(reader);
                var selctedFilters = criteria.MultiSelectActiveFields.ContainsKey(searchFilter.field) ? criteria.MultiSelectActiveFields[searchFilter.field] : new List<ISearchFilterValue>();
                var group = new FacetGroup(searchFilter.field, SearchCommon.GetDescription(SearchCriteria.Locale, searchFilter.Descriptions));
                var groupCount = 0;

                groupCount += GetFilterCount(reader, baseDocIdSet, group, searchFilter.field, searchFilter.Values.SimpleValue,
                    t => false, selctedFilters.ToList())
                    ;
                groupCount += GetFilterCount(reader, baseDocIdSet, group, searchFilter.field, searchFilter.Values.RangeValue,
                    t => false, selctedFilters.ToList());
                groupCount += GetFilterCount(reader, baseDocIdSet, group, searchFilter.field, searchFilter.Values.PriceRangeValue,
                    t => t == null || !SearchCriteria.Currency.CurrencyCode.Equals(t.currency, StringComparison.OrdinalIgnoreCase), selctedFilters.ToList());

                // Add only if items exist under
                if (groupCount > 0)
                {
                    groups.Add(group);
                }
            }

            _facetGroups = groups.ToArray();
        }

        private int GetSingleValueCount<T>(IndexReader reader, DocIdSet baseDocIdSet, string filterField, T value) where T : ISearchFilterValue
        {
            var queryFilter = new QueryWrapperFilter(LuceneQueryHelper.CreateQuery(filterField, value));
            var bs = new OpenBitSetDISI(baseDocIdSet.Iterator(), reader.MaxDoc + 100);
            bs.InPlaceAnd(queryFilter.GetDocIdSet(reader).Iterator());
            return (int)bs.Cardinality();
        }

        private void AddNewFacet<T>(FacetGroup facetGroup, T value, int newCount, bool isSelected) where T : ISearchFilterValue
        {
            var newFacet = new MultiFacet(facetGroup, value.key, SearchCommon.GetDescription(SearchCriteria.Locale, value.Descriptions), newCount, isSelected);
            facetGroup.Facets.Add(newFacet);
        }

        private int GetFilterCount<T>(IndexReader reader, DocIdSet baseDocIdSet, FacetGroup facetGroup, string filterField, T[] values,
            Func<T, bool> invalidValue, List<ISearchFilterValue> selectedFilters) where T : ISearchFilterValue
        {
            if (values == null)
            {
                return 0;
            }

            var count = 0;
            foreach (T value in values)
            {
                if (invalidValue(value))
                {
                    continue;
                }
                var newCount = GetSingleValueCount<T>(reader, baseDocIdSet, filterField, value);
                if (newCount > 0)
                {
                    AddNewFacet<T>(facetGroup, value, newCount, selectedFilters.FirstOrDefault(x => x.key == value.key) != null);
                    count += newCount;
                }
            }
            return count;
        }

        
    }
}
