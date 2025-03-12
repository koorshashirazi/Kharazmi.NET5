namespace Kharazmi.ElasticSearch
{
    public readonly struct EmitEventFailureHandlings
    {
        public const string WriteToSelfLog = "WriteToSelfLog";
        public const string WriteToFailureSink = "WriteToFailureSink";
        public const string ThrowException = "ThrowException";
        public const string RaiseCallback = "RaiseCallback";
    }
}