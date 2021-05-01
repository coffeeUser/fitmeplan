namespace Fitmeplan.Contracts.Dtos
{
    public class GroupedDictItem<T> : DictItem<T>
    {
        public string GroupLabel { get; set; }
    }
}
