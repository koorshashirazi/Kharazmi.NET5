using System.ComponentModel.DataAnnotations;

namespace Kharazmi.Options
{
    public class ExceptionOption : ConfigurePluginOption
    {
        public ExceptionOption()
        {
            RedirectOptions = new RedirectOption();
            ErrorViewName = "Error";
        }

        public RedirectOption RedirectOptions { get; set; }
        [StringLength(20)] public string ErrorViewName { get; set; }
        public bool UseTempDataNotification { get; set; }


        public override void Validate()
        {
            RedirectOptions.Validate();
        }
    }
}