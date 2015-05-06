using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Search;
using Mediachase.Search.Extensions;
using Mediachase.Search.Providers.Lucene;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Reference.Commerce.LuceneSearchProvider
{
    /// <summary>
    /// 
    /// </summary>
    public class MultiFacetQueryBuilder : LuceneSearchQueryBuilder
    {
        /// <summary>
        /// Builds the query.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        public override object BuildQuery(ISearchCriteria criteria)
        {
            
            var query = new BooleanQuery();

            if (!criteria.IgnoreFilterOnLanguage)
            {
                var languageQuery = new TermQuery(new Term(BaseCatalogIndexBuilder.FieldConstants.Language, criteria.Locale.ToLower()));
                query.Add(languageQuery, Occur.MUST);
            }
            var ec = criteria as CatalogEntrySearchCriteria;

            AddQuery(BaseCatalogIndexBuilder.FieldConstants.Catalog, query, ec.CatalogNames);
            AddQuery(BaseCatalogIndexBuilder.FieldConstants.Node, query, ec.CatalogNodes);

            if (ec.CatalogNodes.Count > 0)
            {
                AddQuery(BaseCatalogIndexBuilder.FieldConstants.Node, query, ec.CatalogNodes);
            }

            if (ec.Outlines.Count > 0)
            {
                AddQuery(BaseCatalogIndexBuilder.FieldConstants.Outline, query, ec.Outlines);
            }

            // Add search
            if (!String.IsNullOrEmpty(ec.SearchPhrase))
            {
                if (ec.IsFuzzySearch)
                {
                    #region Fuzzy Search the keyword

                    ITermAttribute termAtt = null;
                    using (var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
                    {
                        using (TokenStream source = analyzer.TokenStream(BaseCatalogIndexBuilder.FieldConstants.Content, new System.IO.StringReader(ec.SearchPhrase)))
                        {
                            var buffer = new CachingTokenFilter(source);
                            var success = false;
                            try
                            {
                                buffer.Reset();
                                success = true;
                            }
                            catch (System.IO.IOException)
                            {
                            }

                            if (success)
                            {
                                if (buffer.HasAttribute<ITermAttribute>())
                                {
                                    termAtt = buffer.GetAttribute<ITermAttribute>();
                                }
                            }

                            if (termAtt != null)
                            {
                                try
                                {
                                    var q = new BooleanQuery(true);
                                    while (source.IncrementToken())
                                    {
                                        var currentQuery = new FuzzyQuery(new Term(BaseCatalogIndexBuilder.FieldConstants.Content, termAtt.Term));
                                        q.Add(currentQuery, Occur.MUST);
                                    }
                                    query.Add(q, Occur.MUST);
                                }
                                catch (System.IO.IOException)
                                {
                                    // ignore
                                }
                            }
                            try
                            {
                                // rewind the buffer stream
                                buffer.Reset();
                            }
                            catch (System.IO.IOException)
                            {

                            }
                        }
                    }

                    #endregion
                }
                else
                {
                    var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, BaseCatalogIndexBuilder.FieldConstants.Content, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
                    {
                        DefaultOperator = QueryParser.Operator.AND
                    };
                    var searchQuery = parser.Parse(ec.SearchPhrase);
                    query.Add(searchQuery, Occur.MUST);
                }
            }
            if (!criteria.IncludeInactive)
            {
                var inactiveQuery = new TermQuery(
                new Term(BaseCatalogIndexBuilder.FieldConstants.IsActive, "true"));
                query.Add(inactiveQuery, Occur.MUST);
            }

            AddQuery(BaseCatalogIndexBuilder.FieldConstants.MetaClass, query, ec.SearchIndex);

            var loweredClassTypes = new StringCollection();
            foreach (string classType in ec.ClassTypes) loweredClassTypes.Add(classType.ToLowerInvariant());
            AddQuery(BaseCatalogIndexBuilder.FieldConstants.ClassType, query, loweredClassTypes);

            // Add date filter                
            var datesFilterStart = new TermRangeQuery(BaseCatalogIndexBuilder.FieldConstants.StartDate, null, ec.StartDate.ToString("s"), false, true);
            query.Add(datesFilterStart, Occur.MUST);
            var datesFilterEnd = new TermRangeQuery(BaseCatalogIndexBuilder.FieldConstants.EndDate, ec.EndDate.ToString("s"), null, true, false);
            query.Add(datesFilterEnd, Occur.MUST);

            // Add market filter _ExcludedCatalogEntryMarkets
            var marketQuery = new TermQuery(
                new Term(ExcludedCatalogEntryMarketsField.FieldName.ToLower(), criteria.MarketId.Value.ToLowerInvariant()));
            query.Add(marketQuery, Occur.MUST_NOT);

            return query;
        }


        /// <summary>
        /// Adds the selected filters to the query.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <param name="query">The query.</param>
        public BooleanQuery AddSelectedFilters(ISearchCriteria criteria, BooleanQuery query, String skipFieldName)
        {
            var newQuery = new BooleanQuery { { query, Occur.MUST } };
            var multicriteria = criteria as MultiFacetSearchCriteria;
            if (multicriteria == null)
            {
                return null;
            }
            // add the filters if any
            if (multicriteria.MultiSelectActiveFields.Count != 0)
            {
                foreach (var field in multicriteria.MultiSelectActiveFields.Keys)
                {
                    if (!String.IsNullOrEmpty(skipFieldName) && field.Equals(skipFieldName))
                    {
                        continue;
                    }
                    var values = multicriteria.MultiSelectActiveFields[field].ToList();
                    if (values.Count > 1)
                    {
                        var valuesQuery = new BooleanQuery();
                        foreach (var value in values)
                        {
                            if (CheckCondition(criteria, value))
                            {
                                continue;
                            }
                            var filterQuery = LuceneQueryHelper.CreateQuery(field, value);
                            if (filterQuery != null)
                            {
                                valuesQuery.Add(filterQuery, Occur.SHOULD);
                            }

                        }
                        newQuery.Add(valuesQuery, Occur.MUST);
                    }
                    else
                    {
                        var value = values.Single();
                        if (CheckCondition(criteria, value))
                        {
                            continue;
                        }

                        var filterQuery = LuceneQueryHelper.CreateQuery(field, value);
                        if (filterQuery != null)
                        {
                            newQuery.Add(filterQuery, Occur.MUST);
                        }
                    }

                }
            }
            return newQuery;
        }

        private static bool CheckCondition(ISearchCriteria criteria, ISearchFilterValue value)
        {
            var priceValue = value as PriceRangeValue;
            if (priceValue != null)
            {
                if (!string.Equals(criteria.Currency.CurrencyCode, priceValue.currency, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (!string.IsNullOrEmpty(priceValue.market) && criteria.MarketId.Equals(new MarketId(priceValue.market)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
