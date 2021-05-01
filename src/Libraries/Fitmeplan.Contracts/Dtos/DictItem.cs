namespace Fitmeplan.Contracts.Dtos
{
    public class DictItem<T>
    {
        public T Value { get; set; }

        public string Label { get; set; }

        public object ExtendedData { get; set; }
    }
}
