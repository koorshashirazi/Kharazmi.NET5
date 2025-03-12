namespace Kharazmi.Options.Mongo
{
    public class MongoOptions : ConfigurePluginOptionWithChild<MongoOption>
    {
        private bool _useDatabaseInstaller;
        private bool _useSecondLevelCache;

        public MongoOptions()
        {
            _useDatabaseInstaller = false;
        }

        public bool UseDatabaseInstaller
        {
            get => Enable && _useDatabaseInstaller;
            set => _useDatabaseInstaller = value;
        }


        public bool UseSecondLevelCache
        {
            get => Enable && _useSecondLevelCache;
            set => _useSecondLevelCache = value;
        }

        public SecondLevelCacheOption? SecondLevelCacheOption { get; set; }

        public string? DefaultOption { get; set; }

        public override void Validate()
        {
            foreach (var option in ChildOptions)
                option.Validate();

            if(!UseSecondLevelCache) return;
            SecondLevelCacheOption ??= new SecondLevelCacheOption();
            SecondLevelCacheOption.Validate();
        }
    }
}