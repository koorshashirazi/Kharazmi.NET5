using System.Collections.Generic;
using System.Linq;

namespace Kharazmi.Options.Cookie
{
    /// <summary>_ </summary>
    public class CookieValidateOption : NestedOption
    {
        private readonly HashSet<string> _claimsNotAllowEmpty;

        public CookieValidateOption()
        {
            _claimsNotAllowEmpty = new HashSet<string>();
            ClaimsMustBe = new Dictionary<string, string>();
        }

        /// <summary>_</summary>
        public string[]? ClaimsNotAllowEmpty
        {
            get => _claimsNotAllowEmpty.ToArray();
            set
            {
                if (value is null || value.Length <= 0) return;
                foreach (var val in value)
                    _claimsNotAllowEmpty.Add(val);
            }
        }

        /// <summary>_</summary>
        public Dictionary<string, string> ClaimsMustBe { get; set; }

        public override void Validate()
        {
        }
    }
}