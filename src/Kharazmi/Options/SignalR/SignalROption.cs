using System;
using System.Collections.Generic;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.HealthChecks;
using Kharazmi.Options.HealthCheck;

namespace Kharazmi.Options.SignalR
{
    public class SignalROption : ConfigurePluginOption, IHaveHealthCheckOption
    {
        private bool _useHubConnection;

        public SignalROption()
        {
        }
        
        public string? Url { get; set; }
        
        public bool UseAutomaticReconnect { get; set; }

        public HashSet<TimeSpan>? ReconnectDelays { get; set; }

        public bool UseHubConnection
        {
            get => Enable && _useHubConnection;
            set => _useHubConnection = value;
        }

        public HubConnectionOption? HubConnectionOption { get; set; }
        public bool UseHealthCheck { get; set; }
        public HealthCheckOption? HealthCheckOption { get; set; }

        public override void Validate()
        {
            if (Enable == false) return;

            if (Url.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(SignalROption), nameof(Url)));

            if (UseAutomaticReconnect)
                if (ReconnectDelays is null || ReconnectDelays.Count <= 0)
                    ReconnectDelays = SignalRConstants.DefaultRetryDelaysInMilliseconds;
            
            if (UseHubConnection)
            {
                HubConnectionOption ??= new HubConnectionOption();
                HubConnectionOption.Validate();
            }

            if (UseHealthCheck)
            {
                HealthCheckOption ??= new HealthCheckOption();
                HealthCheckOption.Validate();
            }
        }
    }
}