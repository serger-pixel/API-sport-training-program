using API_sprot_training_program.Models;
using MongoDB.Driver;
using System.Diagnostics;
using System.Text;
using System.Text.Json;


namespace API_sprot_training_program_test
{
    public class ApiEntityTests
    {
        [Fact]
        public async Task Insert_100_Entities()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5276")
            };

            for (int i = 0; i < 100; i++)
            {
                var entity = new DtoCreateUpdate
                {
                    Title = "Навзвание",
                    Description = "Описание",
                    CntInWeek = i % 7 + 1,
                    DurationInWeek = i % 52 + 1
                };
                var json = JsonSerializer.Serialize(entity);

                var response = await client.PostAsync(
                    "/api/programs",
                    new StringContent(json, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
            }

        }

    }
}
