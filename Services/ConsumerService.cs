using Confluent.Kafka;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net;
using System.Text.Json;
using training_service_db.Models;
using static Confluent.Kafka.ConfigPropertyNames;

namespace training_service_db.Services
{
    public class ConsumerService : BackgroundService
    {
        private readonly string broker = Environment.GetEnvironmentVariable("BROKER_ADDR") ?? "kafka:9092";

        private readonly string consumer_topic = Environment.GetEnvironmentVariable("TOPIC_USERS") ?? "users-topic";

        private readonly string group_id = Environment.GetEnvironmentVariable("GROUP_TRAINING") ?? "group-training";


        private readonly ILogger<ConsumerService> _logger;

        private readonly IMongoCollection<Coach> _coaches;

        private readonly IMongoCollection<Coach> _uncomf;

        public ConsumerService(IMongoClient mongoClient,
            IDataBaseSettings settings, ILogger<ConsumerService> logger)
        {
            var mongoDatabase = mongoClient.GetDatabase(
                settings.DatabaseName);

            _coaches = mongoDatabase.GetCollection<Coach>(
                settings.CollectionNameCoach);

            _uncomf = mongoDatabase.GetCollection<Coach>(
                settings.CollectionNameUncomf);

            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                РrocessMessage();
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Задача была отменена.");
            }
            return Task.CompletedTask;

        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public void РrocessMessage()
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = broker,
                GroupId = group_id,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            var consumerBuilder = new ConsumerBuilder<Null, string>(config).Build();
            consumerBuilder.Subscribe(consumer_topic);
            while (true)
            {
                var consumer = consumerBuilder.Consume();
                var message = JsonSerializer.Deserialize<ConsumerMessage>(consumer.Message.Value);
                if (message == null)
                {
                    continue;
                }
                _logger.LogInformation($"Получено сообщение - IdDecisionMaker : {message.IdConfirmObject}, " +
                    $"IdConfirmObject: {message.IdConfirmObject}, TimeConfirm: {message.TimeConfirm}");

                var element = _uncomf.Find(element => element.Id.Equals(message.IdConfirmObject)).FirstOrDefault();
                element.TimeConfirm = message.TimeConfirm;
                _coaches.InsertOne(element);

            }
        }

    }
}
