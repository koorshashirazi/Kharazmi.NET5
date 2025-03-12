using System.Collections.Generic;
using System.Linq;

namespace Kharazmi.Options.HealthCheck
{
    public class HealthChecksOption : ConfigurePluginOption
    {
        private bool _useHealthCheckUi;
        private readonly HashSet<HealthEndpointOption> _healthEndpointOptions;
        private readonly HashSet<HealthUiWebHookOption> _healthUiWebHookOptions;

        public HealthChecksOption()
        {
            _healthEndpointOptions = new HashSet<HealthEndpointOption>();
            _healthUiWebHookOptions = new HashSet<HealthUiWebHookOption>();
            HealthUiOption = new HealthUiOption();
        }

        public bool UseUi
        {
            get => Enable && _useHealthCheckUi;
            set => _useHealthCheckUi = value;
        }

        public HealthUiOption HealthUiOption { get; set; }

        public IReadOnlyCollection<HealthEndpointOption> HealthEndpointOptions
        {
            get => _healthEndpointOptions;
            set
            {
                foreach (var endpointOption in value)
                    _healthEndpointOptions.Add(endpointOption);
            }
        }

        public IReadOnlyCollection<HealthUiWebHookOption> HealthUiWebHookOptions
        {
            get => _healthUiWebHookOptions;
            set
            {
                foreach (var webHookOption in value)
                    _healthUiWebHookOptions.Add(webHookOption);
            }
        }


        public override void Validate()
        {
            if (Enable == false) return;
            
            if (!HealthEndpointOptions.Any())
                _healthEndpointOptions.Add(new HealthEndpointOption());

            foreach (var endpointOption in HealthEndpointOptions)
                endpointOption.Validate();

            foreach (var webHookOption in HealthUiWebHookOptions)
                webHookOption.Validate();

            HealthUiOption.Validate();
        }
    }
}