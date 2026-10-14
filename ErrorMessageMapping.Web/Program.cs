using ErrorMessageMapping.Application;
using ErrorMessageMapping.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 註冊 DbContext（使用 SQLite）
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=errormapping.db"));

// 註冊 Repository 與 Service
builder.Services.AddScoped<IErrorMessageMappingRepository, ErrorMappingRepository>();
builder.Services.AddScoped<ErrorMessageMappingService>();

var app = builder.Build();

// 自動建立資料庫（開發用）
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
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

app.Run();