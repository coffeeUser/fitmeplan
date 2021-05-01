using System;
using System.Collections.Generic;
using System.Linq;

namespace Fitmeplan.Common.Search
{
    public class Filter
    {
        /// <summary>
        /// Gets or sets the filter inputs.
        /// </summary>
        public List<FilterBase> Inputs { get; set; }

        public string SearchString { get; set; }
    }

    public abstract class FilterBase
    {
        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        public string AttrName { get; set; }

        /// <summary>
        /// Gets or sets the display name of the attribute.
        /// </summary>
        //public string AttrDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Valids this instance.
        /// </summary>
        /// <returns></returns>
        public abstract bool IsValid();
    }

    public class DateFilter : FilterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateFilter"/> class.
        /// </summary>
        public DateFilter()
        {
            Type = FilterType.Date;
        }

        /// <summary>
        /// Gets or sets date from.
        /// </summary>
        public DateTime? From { get; set; }

        /// <summary>
        /// Gets or sets the days from today from.
        /// </summary>
        public int? DaysFromTodayFrom { get; set; }

        /// <summary>
        /// Gets or sets to.
        /// </summary>
        public DateTime? To { get; set; }

        /// <summary>
        /// Gets or sets the days from today to.
        /// </summary>
        public int? DaysFromTodayTo { get; set; }

        /// <summary>
        /// Valids this instance.
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return From.HasValue || To.HasValue;
        }
    }

    public class RangeFilter : FilterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RangeFilter"/> class.
        /// </summary>
        public RangeFilter()
        {
            Type = FilterType.RangeFilter;
        }

        /// <summary>
        /// Gets or sets the low.
        /// </summary>

        public double? From { get; set; }

        /// <summary>
        /// Gets or sets the high.
        /// </summary>

        public double? To { get; set; }

        /// <summary>
        /// Valids this instance.
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return From.HasValue || To.HasValue;
        }
    }

    public class ContainsTextFilter : FilterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainsTextFilter"/> class.
        /// </summary>
        public ContainsTextFilter()
        {
            Type = FilterType.ContainsText;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Valids this instance.
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return !string.IsNullOrEmpty(Value);
        }
    }

    public class InFilter : FilterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InFilter"/> class.
        /// </summary>
        public InFilter()
        {
            Type = FilterType.In;
        }

        /// <summary>
        /// Gets or sets the values.
        /// </summary>

        public int[] Values { get; set; }

        /// <summary>
        /// Valids this instance.
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return Values != null && Values.Any();
        }
    }

    public class HierarchyFilter : FilterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InFilter"/> class.
        /// </summary>
        public HierarchyFilter()
        {
            Type = FilterType.Hierarchy;
        }

        /// <summary>
        /// Gets or sets the values.
        /// </summary>

        public string Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [exact match].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [exact match]; otherwise, <c>false</c>.
        /// </value>
        public bool? ExactMatch { get; set; }

        /// <summary>
        /// Valids this instance.
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return !string.IsNullOrEmpty(Value);
        }
    }

    public class EqualFilter : FilterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EqualFilter"/> class.
        /// </summary>
        public EqualFilter()
        {
            Type = FilterType.Equal;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>

        public object Value { get; set; }

        /// <summary>
        /// Valids this instance.
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            return true;
        }
    }
}