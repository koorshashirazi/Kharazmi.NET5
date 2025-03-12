namespace Kharazmi.RabbitMq
{
    internal struct SourceBindings
    {
        public string Source { get; set; }
        public string Vhost { get; set; }
        public string Destination { get; set; }
        public string DestinationType { get; set; }
        public string RoutingKey { get; set; }
        public object Arguments { get; set; }
        public string PropertiesKey { get; set; }
    }
}