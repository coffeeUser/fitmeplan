using System.Collections.Generic;
using Fitmeplan.Common.Search;

namespace Fitmeplan.Contracts
{
    public class PagedList<T> where T : class 
    {
        public List<T> List { get; set; }

        public PagingOptions Paging { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        public PagedList()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        /// <param name="list">The list.</param>
        public PagedList(List<T> list) : this(list, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="paging">The paging.</param>
        public PagedList(List<T> list, PagingOptions paging)
        {
            List = list;
            Paging = paging;
        }
    }
}
