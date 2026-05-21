using API_sprot_training_program.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Xml.Linq;
using training_service_db.Metrics;
using training_service_db.Models;

namespace training_service_db.Services
{
    public class CoachService
    {
        private readonly IMongoCollection<Coach> _coaches;

        private readonly IMongoCollection<Coach> _uncomf;

        private readonly DataBaseRequestTime _data_base_metric;

        private readonly ProducerService _messSender;

        private const int LIMIT_OF_COACHES = 1000;

        public CoachService(IMongoClient mongoClient,
            IDataBaseSettings settings,
            IMeterFactory meterFactory,
            IDistributedCache cache)
        {


            var mongoDatabase = mongoClient.GetDatabase(
                settings.DatabaseName);

            _coaches = mongoDatabase.GetCollection<Coach>(
                settings.CollectionNameCoach);

            _uncomf = mongoDatabase.GetCollection<Coach>(
                settings.CollectionNameUncomf);

            Type type = typeof(Coach);

            _data_base_metric = new DataBaseRequestTime(meterFactory);

            _messSender = new ProducerService();
        }


        public async Task<List<CoachOutput>> GetByFilter(String nameProperty, String value)
        {
            var property = typeof(CoachOutput).GetProperty(nameProperty);

            if (property == null) return new List<CoachOutput>();

            var targetType = property.PropertyType;
            var convertedValue = Convert.ChangeType(value, targetType);
            var filter = Builders<Coach>.Filter.Eq(nameProperty, convertedValue);
            var coachList = _coaches.Find(filter).ToListAsync();
            return coachList.Result.Select(
                element => MapToOutput(element)
                )
                .ToList();
        }

        public async Task<List<CoachOutput>> GetRandomAsync(int count)
        {
            var pipeline = new EmptyPipelineDefinition<Coach>()
                .Sample(count);
            Stopwatch sw = Stopwatch.StartNew();
            var coachList = _coaches.Aggregate(pipeline).ToListAsync();
            await coachList;
            sw.Stop();
            _data_base_metric.add_value(sw.Elapsed.TotalMilliseconds);
            return coachList.Result.Select(
                element => MapToOutput(element)
                )
                .ToList();
        }

        public async Task<List<CoachOutput>> GetOrderAsync()
        {
            Stopwatch sw = Stopwatch.StartNew();
            var coachList = _coaches.Find(_ => true).ToListAsync();
            await coachList;
            sw.Stop();
            _data_base_metric.add_value(sw.Elapsed.TotalMilliseconds);
            return coachList.Result.Select(
                element => MapToOutput(element)
                )
                .ToList();
        }

        public async Task<List<CoachOutput>> GetAllAsync()
        {

            long count = await _coaches.CountDocumentsAsync(_ => true);
            if (count == LIMIT_OF_COACHES)
            {
                return GetRandomAsync(LIMIT_OF_COACHES).Result;
            }
            else
            {
                return GetOrderAsync().Result;
            }
        }

        public async Task<CoachOutput?> GetByIdAsync(String id)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var element = _coaches.Find(element => element.Id.Equals(id)).FirstOrDefaultAsync();
            await element;
            _data_base_metric.add_value(sw.Elapsed.TotalMilliseconds);
            if (element.Result == null)
            {
                return null;
            }
            return MapToOutput(element.Result);
        }

        public async Task<Coach?> CreateAsync(CoachInput coachInput)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Coach coachModel = MapToModel(coachInput);
            _uncomf.InsertOne(coachModel);
            ProducerMessage message = new ProducerMessage()
            {
                IdConfirmObject = coachModel.Id,
                IdDecisionMaker = coachModel.UserId,
            };
            _messSender.SendMessage(message);
            sw.Stop();
            _data_base_metric.add_value(sw.Elapsed.TotalMilliseconds);
            return coachModel;
        }

        public async Task<ReplaceOneResult> UpdateAsync(String id, CoachInput coach)
        {
            var currentCoach = MapToModel(coach);
            currentCoach.Id = id;
            Stopwatch sw = Stopwatch.StartNew();
            var result = _coaches.ReplaceOneAsync(element => element.Id.Equals(id), currentCoach);
            await result;
            sw.Stop();
            _data_base_metric.add_value(sw.Elapsed.TotalMilliseconds);
            return result.Result;
        }

        public async Task<ReplaceOneResult> UpdateAsync(String id, CoachMessageInput coach)
        {
            var currentCoach = await _coaches.Find(currentCoach => currentCoach.Id.Equals(id)).FirstOrDefaultAsync();
            currentCoach.UserId = coach.UserId;
            currentCoach.TimeConfirm = coach.TimeConfirm;
            Stopwatch sw = Stopwatch.StartNew();
            var result = _coaches.ReplaceOneAsync(element => element.Id.Equals(id), currentCoach);
            await result;
            sw.Stop();
            _data_base_metric.add_value(sw.Elapsed.TotalMilliseconds);
            return result.Result;
        }

        public async Task<DeleteResult> DeleteAsync(String id)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var task = _coaches.DeleteOneAsync(element => element.Id.Equals(id));
            await task;
            _data_base_metric.add_value(sw.Elapsed.TotalMilliseconds);
            return task.Result;
        }

        public async Task<DeleteResult> DeleteAllAsync()
        {
            Stopwatch sw = Stopwatch.StartNew();
            var filter = Builders<Coach>.Filter.Empty;
            var task = _coaches.DeleteManyAsync(filter);
            await task;
            _data_base_metric.add_value(sw.Elapsed.TotalMilliseconds);
            return task.Result;
        }

        private static CoachOutput MapToOutput(Coach coach)
        {
            return new CoachOutput
            {
                Id = coach.Id,
                Name = coach.Name,
                MiddleName = coach.MiddleName,
                SecondName = coach.SecondName,
                MainEducation = coach.MainEducation,
                SubEducation = coach.SubEducation,
                Specializations = new List<TrainingType>(coach.Specializations),
                UserId =coach.UserId, 
                TimeConfirm = coach.TimeConfirm
    };
        }

        private static Coach MapToModel(CoachInput coach)
        {
            return new Coach()
            {
                Name = coach.Name,
                MiddleName = coach.MiddleName,
                SecondName = coach.SecondName,
                MainEducation = coach.MainEducation,
                SubEducation = coach.SubEducation,
                Specializations = new List<TrainingType>(coach.Specializations),
                UserId = coach.UserId
            }
            ;
        }
    }
}
