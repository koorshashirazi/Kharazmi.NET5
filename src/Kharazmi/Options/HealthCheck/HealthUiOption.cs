using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kharazmi.Constants;
using Kharazmi.Extensions;

namespace Kharazmi.Options.HealthCheck
{
    public class HealthUiOption : NestedOption
    {
        private readonly HashSet<string> _customStylesheets;
        private string _resourcesPath;
        private string _defaultStyle;
        private string _defaultLogo;
        public HealthUiOption()
        {
            AsideMenuOpened = true;
            HeaderText = "Health Checks Status";
            DisableMigrations = true;
            MaximumExecutionHistoriesPerEndpoint = 100;
            EvaluationTimeInSeconds = 10;
            ApiMaxActiveRequests = 3;
            MinimumSecondsBetweenFailureNotifications = 600;
            MustBeSecure = false;
            _customStylesheets = new HashSet<string>();
            UseRelativeApiPath = true;
            UseRelativeWebhookPath = true;
            UseRelativeResourcesPath = true;
            UseDefaultStyle = true;
            UiPath = "/health-ui";
            ApiPath = "/health-api";
            WebhookPath = "/health-webhooks";
            _resourcesPath = "wwwroot/health-ui";
            _defaultStyle = "health-ui.css";
            _defaultLogo = "health-logo-64x64.png";
        }
        public bool AsideMenuOpened { get; set; }
        public string HeaderText { get; set; }
        public bool DisableMigrations { get; set; }
        public int MaximumExecutionHistoriesPerEndpoint { get; set; }
        public int EvaluationTimeInSeconds { get; set; }
        public int ApiMaxActiveRequests { get; set; }
        public int MinimumSecondsBetweenFailureNotifications { get; set; }
        public bool MustBeSecure { get; set; }
        public string? SpecialsPolicy { get; set; }
        public bool UseRelativeApiPath { get; set; }
        public bool UseRelativeWebhookPath { get; set; }
        public bool UseRelativeResourcesPath { get; set; }
        public string UiPath { get; set; }
        public string ApiPath { get; set; }
        public string WebhookPath { get; set; }

        public string ResourcesPath
        {
            get => _resourcesPath;
            set => _resourcesPath = value;
        }

        public bool UseDefaultStyle { get; set; }

        public string DefaultStyle
        {
            get => _defaultStyle.IsEmpty() ? "health-ui.css" : _defaultStyle;
            set => _defaultStyle = value;
        }

        public string DefaultLogo
        {
            get => _defaultLogo.IsEmpty() ? "health-logo-64x64.png" : _defaultLogo;
            set => _defaultLogo = value;
        }

        public List<string> CustomStylesheets
        {
            get
            {
                if (UseDefaultStyle)
                    _customStylesheets.Add(DefaultStyle);
                else
                    _customStylesheets.Remove(DefaultStyle);
                
                return _customStylesheets.ToList();
            }
            set
            {
                foreach (var css in value.Where(x => x.IsNotEmpty()))
                    _customStylesheets.Add(css);
            }
        }

        public IEnumerable<string> GetStyleFiles()
        {
            var root = GetResourcePath();
            return CustomStylesheets.Select(x => Path.Combine(root, x));
        }

        public string GetResourceUriPath() => _resourcesPath.Insert(0, "/");
        public string GetResourcePath() => _resourcesPath.Replace("/", "\\");

        public override void Validate()
        {
            if (MaximumExecutionHistoriesPerEndpoint < 0)
                AddValidation(MessageHelper.MustBeGreaterThan(MessageEventName.OptionsValidation,
                    nameof(HealthUiOption), nameof(MaximumExecutionHistoriesPerEndpoint),
                    MaximumExecutionHistoriesPerEndpoint, 0));

            if (EvaluationTimeInSeconds < 0)
                AddValidation(MessageHelper.MustBeGreaterThan(MessageEventName.OptionsValidation,
                    nameof(HealthUiOption), nameof(EvaluationTimeInSeconds),
                    EvaluationTimeInSeconds, 0));

            if (MinimumSecondsBetweenFailureNotifications < 0)
                AddValidation(MessageHelper.MustBeGreaterThan(MessageEventName.OptionsValidation,
                    nameof(HealthUiOption), nameof(MinimumSecondsBetweenFailureNotifications),
                    MinimumSecondsBetweenFailureNotifications, 0));
            
            if (UseDefaultStyle && DefaultStyle.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(HealthUiOption), nameof(DefaultStyle)));

            if (UseDefaultStyle && DefaultLogo.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(HealthUiOption), nameof(DefaultLogo)));

            if (ResourcesPath.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(HealthUiOption), nameof(ResourcesPath)));

            if (UiPath.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(HealthUiOption), nameof(UiPath)));


            if (ApiPath.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(HealthUiOption), nameof(ApiPath)));

            if (MustBeSecure && SpecialsPolicy.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(HealthUiOption), nameof(SpecialsPolicy)));
        }
    }
}