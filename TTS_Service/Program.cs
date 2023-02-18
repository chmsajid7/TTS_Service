using Microsoft.CognitiveServices.Speech;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TTS_Service.Context;
using TTS_Service.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "HeyAlli", Version = "v1" });
});

//

builder.Services.AddScoped<IOpenAIService, OpenAIService>();
builder.Services.AddScoped<IConverterService, ConverterService>();
builder.Services.AddMemoryCache();

var connectionString = builder.Configuration.GetConnectionString("MySql");

builder.Services.AddDbContext<ConverterDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var subscriptionKey = builder.Configuration.GetValue<string>("CognitiveService:SubscriptionKey");
var region = builder.Configuration.GetValue<string>("CognitiveService:Region");
var language = builder.Configuration.GetValue<string>("CognitiveService:Language");

var speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
speechConfig.SpeechRecognitionLanguage = language;

builder.Services.AddSingleton(speechConfig);

//

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

//
var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<ConverterDbContext>();
dbContext.Database.Migrate();
//

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
