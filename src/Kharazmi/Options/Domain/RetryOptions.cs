using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kharazmi.Constants;

namespace Kharazmi.Options.Domain
{
    public class RetryOption : NestedOption, IRetryOptions
    {
        private int _attempt;
        private int _minDelay;
        private int _maxDelay;

        public RetryOption()
        {
            _attempt = 0;
            _minDelay = 0;
            _maxDelay = 0;
            RetryOnExceptionTypes = new HashSet<string>();
        }

        [Range(0, int.MaxValue)]
        public int Attempt
        {
            get => _attempt;
            set
            {
                if (value > 0 && value < int.MaxValue)
                    _attempt = value;
            }
        }

        [Range(0, int.MaxValue)]
        public int MinDelay
        {
            get => _minDelay;
            set
            {
                if (value > 0 && value < int.MaxValue)
                    _minDelay = value;
            }
        }

        [Range(0, int.MaxValue)]
        public int MaxDelay
        {
            get => _maxDelay;
            set
            {
                if (value > _minDelay && value > 0 && value < int.MaxValue)
                    _maxDelay = value;
            }
        }

        public HashSet<string> RetryOnExceptionTypes { get; set; }

        public override void Validate()
        {
            if (_maxDelay < _minDelay)
                AddValidation(MessageHelper.MustBeGreaterThan(MessageEventName.OptionsValidation,
                    nameof(RetryOption), nameof(MaxDelay), _maxDelay, _minDelay));
        }
    }
}