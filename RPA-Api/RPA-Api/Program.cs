using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;
using RPA_Api.IService;
using RPA_Api.Repository;
using RPA_Api.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---- Google Credentials (from relative path) ----
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var relPath = config["Google:ServiceAccountKeyPath"]
        ?? throw new InvalidOperationException("Missing config: Google:ServiceAccountKeyPath");

    var fullPath = Path.Combine(builder.Environment.ContentRootPath, relPath);

    if (!File.Exists(fullPath))
        throw new FileNotFoundException($"Service account key not found at: {fullPath}");

    return GoogleCredential
        .FromFile(fullPath)
        .CreateScoped("https://www.googleapis.com/auth/cloud-platform");
});

// ---- Firestore ----
builder.Services.AddSingleton<FirestoreDb>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var projectId = config["Firebase:ProjectId"]
        ?? throw new InvalidOperationException("Missing config: firebase:ProjectId");

    var credential = sp.GetRequiredService<GoogleCredential>();

    return new FirestoreDbBuilder
    {
        ProjectId = projectId,
        Credential = credential
    }.Build();
});

// ---- Cloud Storage ----
builder.Services.AddSingleton<StorageClient>(sp =>
{
    var credential = sp.GetRequiredService<GoogleCredential>();
    return new StorageClientBuilder { Credential = credential }.Build();
});

// ---- Your services ----
builder.Services.AddScoped<IResumeStorageService, FirebaseResumeStorageService>();
builder.Services.AddScoped<IResumeRepository, FirestoreResumeRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
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
