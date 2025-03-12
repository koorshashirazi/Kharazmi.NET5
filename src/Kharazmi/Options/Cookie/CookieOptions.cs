using System.ComponentModel.DataAnnotations;
using Kharazmi.Constants;
using Kharazmi.Extensions;

namespace Kharazmi.Options.Cookie
{
    public class CookieOption : NestedOption
    {
        public CookieOption()
            => ExpirationOptions = new ExpirationOption();

        /// <summary> </summary>
        [StringLength(100)]
        public string? CookieName { get; set; }

        /// <summary> </summary>
        public bool IsPersistent { get; set; }

        /// <summary>_</summary>
        public bool IsEssential { get; set; }

        /// <summary>_</summary>
        public bool UseTicketStore { get; set; }

        /// <summary>_</summary>
        public ExpirationOption ExpirationOptions { get; set; }

        public override void Validate()
        {
            if (CookieName.IsEmpty())
            {
                AddValidation(new ValidationResult(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(CookieOption), nameof(CookieName)), new[] {nameof(CookieName)}));
            }

            ExpirationOptions.Validate();
        }
    }
}