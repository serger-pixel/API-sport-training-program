using API_sprot_training_program.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Xml.Linq;

namespace API_sprot_training_program.Services
{
    public class TrainingProgramService
    {
        private readonly IMongoCollection<TrainingProgram> _programs;

        private const int LIMIT_OF_PROGRAMS = 1000;

        private readonly System.Reflection.FieldInfo[] _properties;

        public TrainingProgramService(IOptions<DataBaseSettings> settings)
        {

            var mongoClient = new MongoClient(
            settings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                settings.Value.DatabaseName);

            _programs = mongoDatabase.GetCollection<TrainingProgram>(
                settings.Value.CollectionName);

            Type type = typeof(TrainingProgram);

            _properties = type.GetFields();
        }


        public async Task<List<DtoRead>> GetByFilter(String nameProperty, String value)
        {
            var property = typeof(DtoRead).GetProperty(nameProperty);

            if (property == null) return new List<DtoRead>();

            var targetType = property.PropertyType;
            var convertedValue = Convert.ChangeType(value, targetType);
            var filter = Builders<TrainingProgram>.Filter.Eq(nameProperty, convertedValue);
            var programsList = await _programs.Find(filter).ToListAsync();
            return programsList.Select(
                element => MapToDto(element)
                )
                .ToList();
        }

        public async Task<List<DtoRead>> GetRandomAsync(int count)
        {
    
            var pipeline = new EmptyPipelineDefinition<TrainingProgram>()        
                .Sample(count);     
            var programsList = await _programs.Aggregate(pipeline).ToListAsync();
            return programsList.Select(
                element => MapToDto(element)
                )
                .ToList();
        }

        public async Task<List<DtoRead>> GetOrderAsync()
        {
            var programsList = await _programs.Find(_ => true).ToListAsync();
            return programsList.Select(
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

        public async Task<DtoRead> GetByIdAsync(String id)
        {
            return MapToDto(await _programs.Find(element => element.Id.Equals(id)).FirstOrDefaultAsync());
        }

        public async Task CreateAsync(DtoCreateUpdate program)
        {
            await _programs.InsertOneAsync(MapToEntity(program));
        }

        public async Task UpdateAsync(String id, DtoCreateUpdate program)
        {
            var currentProgram = MapToEntity(program);
            currentProgram.Id = id;
            await _programs.ReplaceOneAsync(element => element.Id.Equals(id), currentProgram);
        }

        public async Task DeleteAsync(String id)
        {
            await _programs.DeleteOneAsync(element => element.Id.Equals(id));
        }

        public async Task DeleteAllAsync()
        {
            var filter = Builders<TrainingProgram>.Filter.Empty;
            await _programs.DeleteManyAsync(filter);
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
