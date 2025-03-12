namespace Kharazmi.Options.RabbitMq
{
    public class GeneralQueueConfiguration
    {
        public GeneralQueueConfiguration()
        {
            Exclusive = false;
            AutoDelete = false;
            Durable = true;
        }

        public bool AutoDelete { get; set; }

        public bool Durable { get; set; }

        public bool Exclusive { get; set; }
    }
}