namespace Fitmeplan.Common.Search
{
    /// <summary>
    /// 	Represents class for paging options.
    /// </summary>
    public class PagingOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagingOptions"/> class.
        /// </summary>
        public PagingOptions()
        {
            Page = 1;
        }
        /// <summary>
        /// 	Gets or sets current page
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 	Gets or sets items count
        /// </summary>
        public int ItemsCount { get; set; }

        /// <summary>
        /// 	Gets or sets number of results per page
        /// </summary>
        public int ItemsPerPage { get; set; } = 50;

        /// <summary>
        /// 	Gets or sets pages count
        /// </summary>
        public int PageCount { get { return ItemsCount / ItemsPerPage + (ItemsCount % ItemsPerPage == 0 ? 0 : 1); } }

        /// <summary>
        /// 	Gets/sets order by property.
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// 	Gets/sets ascending type.
        /// </summary>
        public bool? Asc { get; set; } = true;

        public int PageIndex
        {
            get { return Page - 1; }
        }
    }
}
