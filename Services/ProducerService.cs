using Confluent.Kafka;
using System.Net;
using System.Text.Json;
using training_service_db.Models;

namespace API_sprot_training_program.Services
{
    public class ProducerService
    {
        private readonly string broker = Environment.GetEnvironmentVariable("BROKER_ADDR") ?? "kafka:9092";

        private readonly string producer_topic = Environment.GetEnvironmentVariable("TOPIC_TRAINING") ?? "training-topic";


        public async Task<bool> SendMessage(ProducerMessage query)
        {
            string message = JsonSerializer.Serialize(query);

            ProducerConfig config = new ProducerConfig
            {
                BootstrapServers = broker,
                ClientId = Dns.GetHostName()
            };

            var producer = new ProducerBuilder<Null, string>(config).Build();
            producer.ProduceAsync(producer_topic, new Message<Null, string> { Value = message });

            return await Task.FromResult(true);
        }
    }
}
