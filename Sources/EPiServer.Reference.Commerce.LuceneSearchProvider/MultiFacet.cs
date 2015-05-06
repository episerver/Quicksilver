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
    public class MultiFacet : Facet
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiFacet"/> class.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="key">The key.</param>
        /// <param name="name">The name.</param>
        /// <param name="count">The count.</param>
        /// <param name="isSelected">if set to <c>true</c> [is selected].</param>
        public MultiFacet(ISearchFacetGroup @group, string key, string name, int count, bool isSelected) :
            base(@group, key, name, count)
        {
            IsSelected = isSelected;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected { get; private set; }
    }
}
