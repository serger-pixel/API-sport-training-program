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


        [Fact]
        public async Task Insert_100000_Entities()
        {
            int total = 100000;
            int parallelRequests = 50;

            var semaphore = new SemaphoreSlim(parallelRequests);
            var tasks = new List<Task>();

            var client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5276")
            };

            for (int i = 0; i < total; i++)
            {
                int index = i;

                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();

                    try
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
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);

        }

        [Fact]
        public async Task Delete_All()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5276")
            };
            await client.DeleteAsync("/api/programs/all");
        }

    }
}
