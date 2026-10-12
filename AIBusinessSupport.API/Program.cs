using AIBusinessSupport.Api.Data;
using AIBusinessSupport.Api.Services;
using AIBusinessSupport.Api.Services.Conversations;
using AIBusinessSupport.Api.Services.Embeddings;
using AIBusinessSupport.Api.Services.VectorStore;
using AIBusinessSupport.API.AI;
using AIBusinessSupport.API.Data;
using AIBusinessSupport.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("LocalAngular", policy =>
            policy.SetIsOriginAllowed(origin =>
                    Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
                    uri.IsLoopback &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                .AllowAnyHeader()
                .AllowAnyMethod());
    });
}

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Register the CustomerService with the DI container
builder.Services.AddScoped<ICustomerService, CustomerService>();
// Register the OrderService with the DI container
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<BusinessTools>();
builder.Services.AddSingleton<IConversationStore, ConversationStore>();
//Register dbconetext here
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("BusinessDb"));
});


// Add Swagger UI for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Groq Configuration
var groqApiKey = builder.Configuration["Groq:ApiKey"];

if (string.IsNullOrWhiteSpace(groqApiKey))
{
    throw new InvalidOperationException(
        "Groq API key is not configured.");
}

var chatClient = new ChatClient(
    model: "openai/gpt-oss-20b",
    credential: new ApiKeyCredential(groqApiKey),
    options: new OpenAIClientOptions
    {
        Endpoint = new Uri("https://api.groq.com/openai/v1")
    });

builder.Services.AddSingleton<IChatClient>(
    chatClient.AsIChatClient());

builder.Services.AddSingleton<VectorDbConnectionFactory>();
builder.Services.AddScoped<DocumentChunkRepository>();
builder.Services.AddSingleton<TextChunkingService>();
builder.Services.AddScoped<DocumentIngestionService>();
builder.Services.AddScoped<DocumentSearchRepository>();
builder.Services.AddSingleton<PdfTextExtractionService>();


builder.Services.AddHttpClient<EmbeddingService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:8081");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// seed the data in the database
using(var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
}

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseCors("LocalAngular");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
