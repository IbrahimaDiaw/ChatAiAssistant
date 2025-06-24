using ChatAI_Assistant.Server.Extensions;
using ChatAI_Assistant.Server.Hubs;
using Microsoft.AspNetCore.Http.Connections;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDatabaseServices(builder.Configuration);
builder.Services.AddChatServices(builder.Configuration);
builder.Services.AddSignalRServices(builder.Environment);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy => policy
        .WithOrigins(builder.Configuration["ClientUrl"]!.Split(";"))
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/chathub", options =>
{
    options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
});

app.Run();
