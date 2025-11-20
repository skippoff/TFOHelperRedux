namespace TFOHelperRedux.Models
{
    // Обёртка для привязки к чекбоксам
    public class SelectableItem<T>
    {
        public T Value { get; set; }
        public bool IsSelected { get; set; }

        public SelectableItem(T value, bool selected = false)
        {
            Value = value;
            IsSelected = selected;
        }
    }
}
