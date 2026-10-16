using ErrorMessageMapping.Application;
using ErrorMessageMapping.Domain;
using ErrorMessageMapping.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 設定 Serilog 寫檔
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.File(
        "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorPages();

// 註冊 DbContext（使用 SQLite）
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=errormapping.db"));

// 註冊 Repository 與 Service
builder.Services.AddScoped<IErrorMessageMappingRepository, ErrorMappingRepository>();
builder.Services.AddScoped<ErrorMessageMappingService>();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandler = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (exceptionHandler?.Error != null)
        {
            Log.Error(exceptionHandler.Error, "Unhandled exception occurred");
        }
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Internal Server Error");
    });
});

// 自動建立資料庫（開發用）
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // 如果資料庫是空的，就塞入測試資料
    if (!db.ErrorMappings.Any())
    {
        db.ErrorMappings.AddRange(
            //new ErrorMapping
            //{
            //    MatchType = MappingConstants.MatchTypeExact,
            //    MatchValue1 = "Job Done",
            //    ReplaceMessage = "工作完成",
            //    CreatedBy = "Seed"
            //},
            new ErrorMapping
            {
                MatchType = MappingConstants.MatchTypeExact,
                MatchValue1 = "Job Done",
                ReplaceMessage = "工作完成",
                CreatedBy = "Seed"
            },
            new ErrorMapping
            {
                MatchType = MappingConstants.MatchTypeFuzzy,
                MatchValue1 = "backup",
                MatchValue2 = "error",
                ReplaceMessage = "備份作業發生錯誤",
                CreatedBy = "Seed"
            }
        );
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();
app.MapGet("/", () => Results.Redirect("/Index"));

app.Run();