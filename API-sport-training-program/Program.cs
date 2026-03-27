using API_sprot_training_program.Models;
using API_sprot_training_program.Services;
using MongoDB.Driver; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var connectionString =
    builder.Configuration["TrainingProgramsDatabase:ConnectionString"];

var databaseName =
    builder.Configuration["TrainingProgramsDatabase:DatabaseName"];

builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(connectionString));

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(databaseName);
});

builder.Services.AddSingleton<TrainingProgramService>();


    builder.Services.AddSwaggerGen();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection(); 
app.UseAuthorization();    

app.MapControllers();

app.Run();