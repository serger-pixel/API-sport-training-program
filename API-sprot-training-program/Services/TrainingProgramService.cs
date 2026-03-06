using API_sprot_training_program.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Runtime.ConstrainedExecution;

namespace API_sprot_training_program.Services
{
    public class TrainingProgramService
    {
        private readonly IMongoCollection<TrainingProgram> _programs;

        private const int LIMIT_OF_PROGRAMS = 1000;

        public TrainingProgramService(IOptions<DataBaseSettings> settings)
        {

            var mongoClient = new MongoClient(
            settings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                settings.Value.DatabaseName);

            _programs = mongoDatabase.GetCollection<TrainingProgram>(
                settings.Value.CollectionName);
        }

        public async Task<List<TrainingProgram>> GetRandomAsync(int count)
        {
    
            var pipeline = new EmptyPipelineDefinition<TrainingProgram>()        
                .Sample(count);            
            return await _programs.Aggregate(pipeline).ToListAsync();
        }

        public async Task<List<TrainingProgram>> GetOrderAsync()
        {
            return await _programs.Find(_ => true).ToListAsync();
        }

        public async Task<List<DtoRead>> GetAllAsync()
        {
            long count = await _programs.CountDocumentsAsync(_ => true);
            List<TrainingProgram> programsList = null;
            if (count == LIMIT_OF_PROGRAMS)
            {
                programsList = GetRandomAsync(LIMIT_OF_PROGRAMS).Result;
            }
            else
            {
                programsList = GetOrderAsync().Result;
            }
           
            return programsList.Select(
                element => MapToDto(element)
                )
                .ToList();
        }

        public async Task<DtoRead> GetByIdAsync(long id)
        {
            return MapToDto(await _programs.Find(element => element.Id == id).FirstOrDefaultAsync());
        }

        public async Task CreateAsync(DtoCreateUpdate program)
        {
            await _programs.InsertOneAsync(MapToEntity(program));
        }

        public async Task UpdateAsync(long id, DtoCreateUpdate program)
        {
            await _programs.ReplaceOneAsync(element => element.Id == id, MapToEntity(program));
        }

        public async Task DeleteAsync(long id)
        {
            await _programs.DeleteOneAsync(element => element.Id == id);
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
