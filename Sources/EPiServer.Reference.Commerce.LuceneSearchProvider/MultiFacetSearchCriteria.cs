using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mediachase.Search;
using Mediachase.Search.Extensions;

namespace EPiServer.Reference.Commerce.LuceneSearchProvider
{
    /// <summary>
    /// 
    /// </summary>
    public class MultiFacetSearchCriteria : CatalogEntrySearchCriteria
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiFacetSearchCriteria"/> class.
        /// </summary>
        public MultiFacetSearchCriteria()
        {
            MultiSelectActiveFields = new Dictionary<string, IEnumerable<ISearchFilterValue>>();
        }

        /// <summary>
        /// Gets the multi select active fields.
        /// </summary>
        /// <value>
        /// The multi select active fields.
        /// </value>
        public Dictionary<string, IEnumerable<ISearchFilterValue>>  MultiSelectActiveFields { get; private set; }

        /// <summary>
        /// Adds the mulit select active field.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="values">The values.</param>
        public void AddMultiSelectActiveField(string fieldName, IEnumerable<ISearchFilterValue> values)
        {
            MultiSelectActiveFields.Add(fieldName, values);
        }
    }
}
