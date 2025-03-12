using System;
using Kharazmi.Constants;
using Kharazmi.Exceptions;

namespace Kharazmi.Options.Cookie
{
    public class ExpirationOption : NestedOption
    {
        private TimeSpan? _absoluteExpiration;
        private TimeSpan? _slidingExpiration;

        public ExpirationOption()
        {
            AbsoluteExpiration = TimeSpan.FromSeconds(10);
            SlidingExpiration = TimeSpan.FromSeconds(5);
        }

        public TimeSpan? AbsoluteExpiration
        {
            get => _absoluteExpiration;
            set
            {
                if (value is null)
                    return;

                if (value.Value < TimeSpan.Zero || value.Value > TimeSpan.FromDays(30))
                    throw new OptionValidationException(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                        nameof(ExpirationOption), nameof(AbsoluteExpiration), value.Value, TimeSpan.FromDays(30)));
                _absoluteExpiration = value;
            }
        }

        public TimeSpan? SlidingExpiration
        {
            get => _slidingExpiration;
            set
            {
                if (value is null)
                    return;

                if (value.Value < TimeSpan.Zero || value.Value > TimeSpan.FromDays(30))
                    throw new OptionValidationException(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                        nameof(ExpirationOption), nameof(SlidingExpiration), value.Value, TimeSpan.FromDays(30)));
                _slidingExpiration = value;
            }
        }


        public override void Validate()
        {
            if (_absoluteExpiration.HasValue &&
                _slidingExpiration.HasValue &&
                _slidingExpiration.Value > _absoluteExpiration.Value)
            {
                AddValidation(MessageHelper.MustBeGreaterThan(MessageEventName.OptionsValidation,
                    nameof(ExpirationOption), nameof(AbsoluteExpiration), _absoluteExpiration.Value,
                    _slidingExpiration.Value));
            }


            if (_absoluteExpiration.HasValue && (_absoluteExpiration.Value < TimeSpan.Zero ||
                                                 _absoluteExpiration.Value > TimeSpan.FromDays(30)))
            {
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(ExpirationOption), nameof(AbsoluteExpiration), _absoluteExpiration.Value,
                    TimeSpan.FromDays(30)));
            }

            if (_slidingExpiration.HasValue && (_slidingExpiration.Value < TimeSpan.Zero ||
                                                _slidingExpiration.Value > TimeSpan.FromDays(30)))
            {
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(ExpirationOption), nameof(SlidingExpiration), _slidingExpiration.Value,
                    TimeSpan.FromDays(30)));
            }
        }
    }
}