#region

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kharazmi.Configuration;
using Kharazmi.Exceptions;
using Kharazmi.Extensions;
using Kharazmi.Options;
using Kharazmi.Options.Mongo;
using MongoDB.Driver;

#endregion

namespace Kharazmi.Localization.Extensions
{
    public static class MongoDbOptionsExtensions
    {
        public static MongoOption GetMongoOption(this ISettingProvider settingProvider, string? optionKey)
        {
            var cacheOption = settingProvider.Get<CacheOption>();
            var options = settingProvider.Get<MongoOptions>();
            var key = optionKey.IsEmpty()
                ? options.DefaultOption.IsEmpty()
                    ? options.ChildOptions
                        .FirstOrDefault(x => x.OptionKey != cacheOption.DistributedProviderOptionKey)?.OptionKey
                    : options.DefaultOption
                : optionKey;

            var option = options.FindOrNone(x =>
                x.OptionKey == key && optionKey != cacheOption.DistributedProviderOptionKey);

            if (option.IsNull())
                throw new NotFoundOptionException(nameof(MongoOptions), key);
            return option;
        }


        public static MongoClientSettings BuildClientSettings(this MongoOption option)
            => option.BuildClientSettings(new MongoClientSettings());

        public static MongoClientSettings BuildClientSettings(this MongoOption option,
            [NotNull] MongoClientSettings clientSettings)
        {
            if (option.ConnectionString.IsNotEmpty())
            {
                var mongoUrl = MongoUrl.Create(option.ConnectionString);
                clientSettings = MongoClientSettings.FromUrl(mongoUrl);
            }
            else
            {
                clientSettings.ApplicationName = option.ApplicationName;

                clientSettings.Server = new MongoServerAddress(option.Host, option.Port);

                if (option.ReplicaSetName.IsNotEmpty())
                    clientSettings.ReplicaSetName = option.ReplicaSetName;

                if (option.UserName.IsNotEmpty() && option.Password.IsNotEmpty())
                    clientSettings.Credential = new MongoCredential("SCRAM-SHA-1",
                        new MongoInternalIdentity(option.Database, option.UserName),
                        new PasswordEvidence(option.Password));
            }

            return clientSettings;
        }
    }
}