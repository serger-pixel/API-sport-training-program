using API_sprot_training_program.Metrics;
using API_sprot_training_program.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Xml.Linq;

namespace API_sprot_training_program.Services
{
    public class TrainingProgramService
    {
        private readonly IMongoCollection<TrainingProgram> _programs;

        private readonly DataBaseRequestTime _data_base_metric;

        private const int LIMIT_OF_PROGRAMS = 1000;

        private readonly System.Reflection.FieldInfo[] _properties;

        public TrainingProgramService(IOptions<DataBaseSettings> settings, IMeterFactory meterFactory)
        {

            var mongoClient = new MongoClient(
            settings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                settings.Value.DatabaseName);

            _programs = mongoDatabase.GetCollection<TrainingProgram>(
                settings.Value.CollectionName);

            Type type = typeof(TrainingProgram);

            _properties = type.GetFields();

            _data_base_metric = new DataBaseRequestTime(meterFactory);
        }


        public async Task<List<DtoRead>> GetByFilter(String nameProperty, String value)
        {
            var property = typeof(DtoRead).GetProperty(nameProperty);

            if (property == null) return new List<DtoRead>();

            var targetType = property.PropertyType;
            var convertedValue = Convert.ChangeType(value, targetType);
            var filter = Builders<TrainingProgram>.Filter.Eq(nameProperty, convertedValue);
            var programsList = _programs.Find(filter).ToListAsync();
            return programsList.Result.Select(
                element => MapToDto(element)
                )
                .ToList();
        }

        public async Task<List<DtoRead>> GetRandomAsync(int count)
        {
            var pipeline = new EmptyPipelineDefinition<TrainingProgram>()        
                .Sample(count);
            Stopwatch sw = Stopwatch.StartNew();
            var programsList = _programs.Aggregate(pipeline).ToListAsync();
            await programsList;
            sw.Stop();
            _data_base_metric.add_to_counter(sw.Elapsed.TotalMilliseconds);
            return programsList.Result.Select(
                element => MapToDto(element)
                )
                .ToList();
        }

        public async Task<List<DtoRead>> GetOrderAsync()
        {
            Stopwatch sw = Stopwatch.StartNew();
            var programsList = _programs.Find(_ => true).ToListAsync();
            await programsList;
            sw.Stop();
            _data_base_metric.add_to_counter(sw.Elapsed.TotalMilliseconds);
            return programsList.Result.Select(
                element => MapToDto(element)
                )
                .ToList();
        }

        public async Task<List<DtoRead>> GetAllAsync()
        {

            long count = await _programs.CountDocumentsAsync(_ => true);
            if (count == LIMIT_OF_PROGRAMS)
            {
                return GetRandomAsync(LIMIT_OF_PROGRAMS).Result;
            }
            else
            {
                return GetOrderAsync().Result;
            }
        }

        public async Task<DtoRead?> GetByIdAsync(String id)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var element = _programs.Find(element => element.Id.Equals(id)).FirstOrDefaultAsync();
            await element;
            _data_base_metric.add_to_counter(sw.Elapsed.TotalMilliseconds);
            if (element.Result == null)
            {
                return null;
            }
            return MapToDto(element.Result);
        }

        public async Task CreateAsync(DtoCreateUpdate program)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var result = _programs.InsertOneAsync(MapToEntity(program));
            await result;
            sw.Stop();
            _data_base_metric.add_to_counter(sw.Elapsed.TotalMilliseconds);
        }

        public async Task<ReplaceOneResult> UpdateAsync(String id, DtoCreateUpdate program)
        {
            var currentProgram = MapToEntity(program);
            currentProgram.Id = id;
            Stopwatch sw = Stopwatch.StartNew();
            var result = _programs.ReplaceOneAsync(element => element.Id.Equals(id), currentProgram);
            await result;
            sw.Stop();
            _data_base_metric.add_to_counter(sw.Elapsed.TotalMilliseconds);
            return result.Result;
        }

        public async Task<DeleteResult> DeleteAsync(String id)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var task = _programs.DeleteOneAsync(element => element.Id.Equals(id));
            await task;
            _data_base_metric.add_to_counter(sw.Elapsed.TotalMilliseconds);
            return task.Result;
        }

        public async Task<DeleteResult> DeleteAllAsync()
        {
            Stopwatch sw = Stopwatch.StartNew();
            var filter = Builders<TrainingProgram>.Filter.Empty;
            var task = _programs.DeleteManyAsync(filter);
            await task;
            _data_base_metric.add_to_counter(sw.Elapsed.TotalMilliseconds);
            return task.Result;
        }

        private static DtoRead MapToDto(TrainingProgram program)
        {
            return new DtoRead
            {
                Id = program.Id,
                Title = program.Title,
                Description = program.Description,
                DurationInWeek = program.DurationInWeek,
                CntInWeek = program.CntInWeek
            };
        }

        private static TrainingProgram MapToEntity(DtoCreateUpdate dto)
        {
            return new TrainingProgram
            {
                Title = dto.Title,
                Description = dto.Description,
                DurationInWeek = dto.DurationInWeek,
                CntInWeek = dto.CntInWeek
            };
        }
    }
}
