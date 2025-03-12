namespace Kharazmi.Models
{
    public class SelectListItem
    {
        public SelectListItem()
        {
        }

        public SelectListItem(string text, string value)
            : this()
        {
            Text = text;
            Value = value;
        }

        public SelectListItem(string text, string value, bool selected)
            : this(text, value)
        {
            Selected = selected;
        }

        public SelectListItem(string text, string value, bool selected, bool disabled)
            : this(text, value, selected)
        {
            Disabled = disabled;
        }

        public bool Disabled { get; set; }

        public bool Selected { get; set; }

        public string? Text { get; set; }

        public string? Value { get; set; }
    }
}