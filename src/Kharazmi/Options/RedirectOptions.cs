using System.ComponentModel.DataAnnotations;

namespace Kharazmi.Options
{
    public class RedirectOption: ConfigurePluginOption
    {
        public RedirectOption()
        {
            ControllerName = "Errors";
            ActionName = "Index";
        }

        [StringLength(20)]public string ControllerName { get; set; }
        [StringLength(20)]  public string ActionName { get; set; }
        public object? RouteData { get; set; }


        public override void Validate()
        {
            
        }
    }
}