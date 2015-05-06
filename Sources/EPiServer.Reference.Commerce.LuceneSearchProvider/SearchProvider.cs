using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Search;
using Mediachase.Search;
using Mediachase.Search.Providers.Lucene;
using Lucene.Net.Index;

namespace EPiServer.Reference.Commerce.LuceneSearchProvider
{
    public class SearchProvider : Mediachase.Search.Providers.Lucene.LuceneSearchProvider
    {
        /// <summary>
        /// Searches the datasource using the specified criteria. Criteria is parsed by the query builder specified by <see cref="P:LuceneSearchProvider.QueryBuilderType"/>.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        public override ISearchResults Search(string applicationName, ISearchCriteria criteria)
        {
            var s = new Stopwatch();
            s.Start();

            var query = (BooleanQuery)QueryBuilder.BuildQuery(criteria);
            var multiQueryBuilder = QueryBuilder as MultiFacetQueryBuilder;
            if (multiQueryBuilder == null)
            {
                throw new Exception("Could not cast MultiFacetQueryBuilder...");
            }
            var filteredQuery = multiQueryBuilder.AddSelectedFilters(criteria, query, null);
            Sort querySort;
            if (criteria.Sort == null)
            {
                querySort = null;
            }
            else
            {
                querySort = new Sort(criteria.Sort.GetSort()
                    .Select(sf => new SortField(sf.FieldName, SortField.STRING, sf.IsDescending))
                    .ToArray());
            }

            MultiFacetSearchResults results = null;
            CachedIndexSearcher searcher = null;
            try
            {
                searcher = GetSearcher(applicationName, criteria.Scope);
                var docs = querySort == null
                           ? searcher.Search(filteredQuery, SearchConfiguration.Instance.MaxHitsForSearchResults)
                           : searcher.Search(filteredQuery, null, SearchConfiguration.Instance.MaxHitsForSearchResults, querySort);

                results = new MultiFacetSearchResults(searcher.IndexReader, docs, criteria, query, multiQueryBuilder, SimulateFaceting);
                IndexSearcherPool.Put(searcher);
            }
            catch (Exception)
            {
                if (searcher != null)
                {
                    searcher.Dispose();
                }
                throw;
            }

            s.Stop();
            Debug.WriteLine("Time spent in Search = " + s.Elapsed.ToString() + " Query = " + query.ToString());
            return results;
        }

        
    }
}
