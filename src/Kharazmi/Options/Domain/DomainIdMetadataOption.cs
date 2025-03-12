using System;
using Kharazmi.Common.Metadata;
using Kharazmi.Constants;
using Kharazmi.Extensions;

namespace Kharazmi.Options.Domain
{
    public class DomainIdMetadataOption : NestedOption
    {
        private string _loggingScopeKey;
        private string _responseHeader;
        private string _requestHeader;

        public DomainIdMetadataOption()
        {
            _responseHeader = MetadataKeys.DomainIdHeader;
            _requestHeader = MetadataKeys.DomainIdHeader;
            EnsureExistInHeader = false;
            WriteToLoggingScope = false;
            _loggingScopeKey = MessageEventName.DomainId;
            WriteToResponse = true;
            ReplaceWithTraceId = false;
        }

        public string RequestHeader
        {
            get => _requestHeader;
            set
            {
                if (value.IsNotEmpty())
                    _requestHeader = value;
            }
        }

        public string ResponseHeader
        {
            get => _responseHeader;
            set
            {
                if (value.IsNotEmpty())
                    _responseHeader = value;
            }
        }

        public bool EnsureExistInHeader { get; set; }

        public bool WriteToLoggingScope { get; set; }

        public string LoggingScopeKey
        {
            get => _loggingScopeKey;
            set
            {
                if (value.IsNotEmpty())
                    _loggingScopeKey = value;
            }
        }

        public bool WriteToResponse { get; set; }

        public bool ReplaceWithTraceId { get; set; }

        public Func<string>? DomainIdGenerator { get; set; }

        public override void Validate()
        {
        }
    }
}