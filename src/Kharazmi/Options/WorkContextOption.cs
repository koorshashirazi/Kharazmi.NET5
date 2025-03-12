namespace Kharazmi.Options
{
    public class WorkContextOption : ConfigurePluginOption
    {
        public bool UseUserContext { get; set; }
        public bool UseHttpRequestAccessor { get; set; }
        

        public override void Validate()
        {
            
        }
    }
}