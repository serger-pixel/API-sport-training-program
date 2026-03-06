using API_sprot_training_program.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace API_sprot_training_program.Services
{
    public class TrainingProgramService
    {
        private readonly IMongoCollection<TrainingProgram> _programs;

        public TrainingProgramService(IOptions<DataBaseSettings> settings)
        {

            var mongoClient = new MongoClient(
            settings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                settings.Value.DatabaseName);

            _programs = mongoDatabase.GetCollection<TrainingProgram>(
                settings.Value.CollectionName);
        }

        public async Task<List<TrainingProgram>> GetAllAsync()
        {
            return await _programs.Find(_ => true).ToListAsync();
        }

        public async Task<TrainingProgram> GetByIdAsync(long id)
        {
            return await _programs.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(TrainingProgram user)
        {
            await _programs.InsertOneAsync(user);
        }

        public async Task UpdateAsync(long id, TrainingProgram user)
        {
            await _programs.ReplaceOneAsync(u => u.Id == id, user);
        }

        public async Task DeleteAsync(long id)
        {
            await _programs.DeleteOneAsync(u => u.Id == id);
        }


    }
}
