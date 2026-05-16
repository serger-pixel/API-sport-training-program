using Confluent.Kafka;
using MongoDB.Driver;
using System.Net;
using System.Text.Json;
using training_service_db.Models;
using training_service_db.Services;
using static Confluent.Kafka.ConfigPropertyNames;

namespace training_service_db
{
    public class BrokerService
    {
        private readonly string _broker = Environment.GetEnvironmentVariable("BROKER_ADDR") ?? "kafka:29092";

        private readonly string _topic = Environment.GetEnvironmentVariable("TOPIC_TRAINING") ?? "training-topic";

        private readonly IMongoCollection<Coach> _uncomf;

        private readonly IMongoCollection<Coach> _coach;
        public BrokerService(IMongoCollection<Coach> coach, IMongoCollection<Coach> uncom) {
            _coach = coach;
            _uncomf = uncom;
        }

        public async Task<bool> SendMessage(ProducerMessage coachRequest)
        {
            string message = JsonSerializer.Serialize(coachRequest);

            ProducerConfig config = new ProducerConfig
            {
                BootstrapServers = _broker,
                ClientId = Dns.GetHostName()
            };

            var producer = new ProducerBuilder<Null, string>(config).Build();
            producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });

            return await Task.FromResult(true);
        }

        public Task РrocessMessage()
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _broker,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            var consumerBuilder = new ConsumerBuilder<Null, string>(config).Build();
            consumerBuilder.Subscribe(_topic);
            CancellationToken token = new CancellationToken();
            try
            {
                while (true)
                {
                    var consumer = consumerBuilder.Consume(token);
                    var message = JsonSerializer.Deserialize<ConsumerMessage>(consumer.Message.Value);
                    if (message != null)
                    {
                        var element = _uncomf.Find(element => element.Id.Equals(message.CoachId)).FirstOrDefault();
                        element.TimeConfirm = message.TimeConfirm;
                        element.UserId = message.UserId;
                        _coach.InsertOne(element);
                    }

                }
            }
            catch (OperationCanceledException)
            {
                consumerBuilder.Close();
            }
            return Task.CompletedTask;
        }

    }
}
