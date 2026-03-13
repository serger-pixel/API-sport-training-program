
using API_sprot_training_program.Models;
using API_sprot_training_program.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<DataBaseSettings>(
    builder.Configuration.GetSection("TrainingProgramsDatabase"));
builder.Services.AddSingleton<TrainingProgramService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();

app.Run();
