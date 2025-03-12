using System;
using System.ComponentModel.DataAnnotations;
using Kharazmi.Constants;
using Kharazmi.Extensions;

namespace Kharazmi.Options.Cookie
{
    /// <summary> </summary>
    public class ExtendedCookieOption : ConfigurePluginOption
    {
        private bool _useAuthenticationCookie;
        private bool _useDisabledAuthorization;

        public ExtendedCookieOption()
        {
            CookieOption = new CookieOption();
        }

        public bool UseDisabledAuthorization
        {
            get => Enable && _useDisabledAuthorization;
            set => _useDisabledAuthorization = value;
        }

        public bool UseAuthenticationCookie
        {
            get => Enable && _useAuthenticationCookie;
            set => _useAuthenticationCookie = value;
        }

        /// <summary> </summary>
        [StringLength(100)]
        public string? ApplicationName { get; set; }

        /// <summary> </summary>
        [StringLength(100)]
        public string? SharedCookieAppName { get; set; }

        /// <summary> </summary>
        public TimeSpan? SharedCookieExpire { get; set; }

        /// <summary> </summary>
        [StringLength(100)]
        public string? AccessDeniedPath { get; set; }

        /// <summary> </summary>
        [StringLength(100)]
        public string? LoginPath { get; set; }

        /// <summary> </summary>
        [StringLength(100)]
        public string? LogoutPath { get; set; }

        /// <summary> </summary>
        [StringLength(100)]
        public string? ErrorPath { get; set; }

        /// <summary> </summary>
        [StringLength(100)]
        public string? RedirectOnCookieExpire { get; set; }

        /// <summary> </summary>
        [StringLength(100)]
        public string? ReturnUrlParameter { get; set; }

        /// <summary>_</summary>
        public CookieOption CookieOption { get; set; }

        public bool UseCookieValidation { get; set; }

        /// <summary>_</summary>
        public CookieValidateOption? CookieValidateOption { get; set; }

        public override void Validate()
        {
            if (Enable == false) return;
            if (UseDisabledAuthorization) return;

            if (LoginPath.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(ExtendedCookieOption), nameof(LoginPath)));

            if (LogoutPath.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(ExtendedCookieOption), nameof(LogoutPath)));

            CookieOption.Validate();
            if (!UseCookieValidation) return;
            CookieValidateOption ??= new CookieValidateOption();
            CookieValidateOption.Validate();
        }
    }
}