namespace GHPCommerce.Infra.MessageBrokers.Mqtt
{
    public class MqttOptions
    {
        public int Port { get; set; }
        public int QualityOfService { get; set; }
        public bool Retain { get; set; }
        public string ServerUrl { get; set; }
        public string TopicToPublish { get; set; }
        public  string ClientId { get; set; }
    }
}
