using EmailService.Interfaces;
using EmailService.Middleware;
using EmailService.Services;
using Microsoft.EntityFrameworkCore;
using Shared.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddDbContext<ApplicationDbContext>
    (options => options.UseSqlServer(connStr, x => 
    {
        x.MigrationsAssembly("Shared.EntityFrameworkCore");
        x.EnableRetryOnFailure(2);
    }));

services.AddTransient<DbContext>((_) =>
{
    return new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connStr).Options);
});


services.AddCors();
services.AddControllers().AddJsonOptions(x =>
{
    x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

services.AddRouting(options => options.LowercaseUrls = true)
               .AddLogging()
               .AddOptions()
               .AddDistributedMemoryCache()
               .AddMemoryCache();

services.AddScoped<IEmailSender, EmailSender>();
services.AddScoped<IEmailLogService, EmailLogService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// migrate any database changes on startup (includes initial db creation)
using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dataContext.Database.Migrate();
}

// global cors policy
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

app.Run();
