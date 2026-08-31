using Microsoft.EntityFrameworkCore;
using MoviesAPI.Data;
using MoviesAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOutputCache(options =>
{
    options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(30);
});

var allowedOrigins = builder.Configuration.GetValue<string>("AllowedOrigins")!.Split(",");
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader()
        .WithExposedHeaders("total-records-count");
    });
});

builder.Services.AddDbContext<MoviesDBContext>(options =>
{
    options.UseSqlServer("name=DefaultConnection");
});

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddTransient<IFileStorage, AzureFileStorage>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseOutputCache();

app.UseAuthorization();

app.MapControllers();

app.Run();
