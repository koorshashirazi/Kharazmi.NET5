namespace Kharazmi.Models
{
    public class MessageResult<T>
    {
        public MessageResult()
        {
            
        }
        public string? Code { get; set; }
        public string? Description { set; get; }
        public T? Model { get; set; }
        public string? RedirectToUrl { get; set; }
        public string? JsHandler { get; set; }
    }

    public class MessageResult : MessageResult<object>
    {
    }
}