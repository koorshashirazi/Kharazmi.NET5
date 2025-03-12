using System;
using System.Collections.Generic;
using System.Linq;
using Kharazmi.Constants;
using Kharazmi.Extensions;

namespace Kharazmi.Options
{
    public class InboxOutboxOption : ConfigurePluginOption
    {
        public InboxOutboxOption()
        {
            ProcessType = OutboxProcessTypes.Sequential;
            OutboxMessageExpiration = TimeSpan.FromHours(8);
            InboxMessageExpiration = TimeSpan.FromHours(8);
            PersistenceProvider = OutboxPersistenceProviders.HostMemory;
            PublishingInterval = TimeSpan.FromMilliseconds(2000);
        }

        public TimeSpan OutboxMessageExpiration { get; set; }
        public TimeSpan InboxMessageExpiration { get; set; }
        public TimeSpan PublishingInterval { get; set; }
        public string ProcessType { get; set; }
        public IReadOnlyCollection<string> ProcessTypes => OutboxProcessTypes.GetProcessTypes();
        public string PersistenceProvider { get; set; }
        public IReadOnlyCollection<string> PersistenceProviders => OutboxPersistenceProviders.GetPersistenceProviders;
        public string? PersistenceProviderOptionKey { get; set; }

        public override void Validate()
        {
            if (!Enable) return;

            if (ProcessType.IsEmpty())
                ProcessType = OutboxProcessTypes.Sequential;

            if (PersistenceProvider.IsEmpty())
                PersistenceProvider = OutboxPersistenceProviders.HostMemory;

            if (ProcessTypes.FirstOrDefault(x => x == ProcessType) is null)
                AddValidation(MessageHelper.OutOfRange(MessageEventName.OptionsValidation, nameof(InboxOutboxOption),
                    ProcessTypes, nameof(ProcessType)));

            if (PersistenceProviders.FirstOrDefault(x => x == PersistenceProvider) is null)
                AddValidation(MessageHelper.OutOfRange(MessageEventName.OptionsValidation, nameof(InboxOutboxOption),
                    PersistenceProviders, nameof(PersistenceProvider)));

            if (PersistenceProviderOptionKey != OutboxPersistenceProviders.HostMemory)
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation, nameof(InboxOutboxOption),
                    nameof(PersistenceProviderOptionKey)));

            if (PublishingInterval.Ticks <= -1)
                AddValidation(MessageHelper.MustBePositive(MessageEventName.OptionsValidation,
                    nameof(InboxOutboxOption), nameof(PublishingInterval), PublishingInterval));
        }
    }
}