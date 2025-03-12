using System.ComponentModel.DataAnnotations;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.HealthChecks;
using Kharazmi.Options.HealthCheck;

namespace Kharazmi.Options
{
    public class EmailServerOption : ConfigurePluginOption, IHaveHealthCheckOption
    {
        private bool _usePopServer;
        private bool _useSmtpServer;
        private bool _usePickupFolder;

        public EmailServerOption()
        {
        }


        public bool UseSmtpServer
        {
            get => Enable && _useSmtpServer;
            set => _useSmtpServer = value;
        }

        public bool UsePopServer
        {
            get => Enable && _usePopServer;
            set => _usePopServer = value;
        }

        [StringLength(100)] public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        [StringLength(100)] public string SmtpUsername { get; set; } = string.Empty;
        [StringLength(100)] public string SmtpPassword { get; set; } = string.Empty;
        [StringLength(100)] public string PopServer { get; set; } = string.Empty;
        public int PopPort { get; set; }
        [StringLength(100)] public string PopUsername { get; set; } = string.Empty;
        [StringLength(100)] public string PopPassword { get; set; } = string.Empty;
        [StringLength(100)] public string LocalDomain { get; set; } = string.Empty;

        public bool UsePickupFolder
        {
            get => Enable && _usePickupFolder;
            set => _usePickupFolder = value;
        }

        [StringLength(100)] public string PickupFolder { get; set; } = string.Empty;

        public bool UseHealthCheck { get; set; }
        public HealthCheckOption? HealthCheckOption { get; set; }

        public override void Validate()
        {
            if (_usePickupFolder && PickupFolder.IsEmpty())
            {
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(EmailServerOption), nameof(PickupFolder)));
                return;
            }

            if (_useSmtpServer)
            {
                if (SmtpServer.IsEmpty())
                    AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                        nameof(EmailServerOption), nameof(SmtpServer)));

                if (SmtpUsername.IsEmpty())
                    AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                        nameof(EmailServerOption), nameof(SmtpUsername)));

                if (SmtpPassword.IsEmpty())
                    AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                        nameof(EmailServerOption), nameof(SmtpPassword)));

                if (SmtpPort < 0)
                    AddValidation(MessageHelper.MustBeGreaterThan(MessageEventName.OptionsValidation,
                        nameof(EmailServerOption), nameof(SmtpPassword), SmtpPort, 0));
            }

            if (!_usePopServer) return;

            if (PopServer.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(EmailServerOption), nameof(PopServer)));

            if (PopUsername.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(EmailServerOption), nameof(PopUsername)));

            if (PopPassword.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(EmailServerOption), nameof(PopPassword)));

            if (PopPort < 0)
                AddValidation(MessageHelper.MustBeGreaterThan(MessageEventName.OptionsValidation,
                    nameof(EmailServerOption), nameof(PopPort), PopPort, 0));
            
            if(!UseHealthCheck) return;
            HealthCheckOption ??= new HealthCheckOption();
            HealthCheckOption.Validate();
        }
    }
}