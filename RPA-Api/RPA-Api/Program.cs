using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;
using RPA_Api.IService;
using RPA_Api.Repository;
using RPA_Api.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();




builder.Services.AddSingleton(provider =>
{
    var projectId = builder.Configuration["Firebase:ProjectId"]
        ?? throw new InvalidOperationException("Firebase:ProjectId is missing.");

    // Firestore client uses GOOGLE_APPLICATION_CREDENTIALS implicitly (recommended).
    return FirestoreDb.Create(projectId);
});

builder.Services.AddSingleton(_ => StorageClient.Create());

builder.Services.AddSingleton(provider =>
{
    // Initialise FirebaseAdmin only once.
    // Uses GOOGLE_APPLICATION_CREDENTIALS if set, otherwise will throw.
    if (FirebaseApp.DefaultInstance != null) return FirebaseApp.DefaultInstance;

    return FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.GetApplicationDefault()
    });
});

builder.Services.AddScoped<IResumeStorageService, FirebaseResumeStorageService>();
builder.Services.AddScoped<IResumeRepository, FirestoreResumeRepository>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

try
{
    app.MapControllers();
}
catch (ReflectionTypeLoadException ex)
{
    foreach (var le in ex.LoaderExceptions)
    {
        Console.WriteLine("LOADER EXCEPTION: " + le?.Message);
        Console.WriteLine(le);
    }
    throw;
}

app.Run();
