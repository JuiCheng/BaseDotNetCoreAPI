using Microsoft.EntityFrameworkCore; // 引入 Entity Framework Core 命名空間

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// 將 API Explorer 服務添加到依賴注入容器中。
builder.Services.AddEndpointsApiExplorer();
// 將 Swagger 生成器服務添加到依賴注入容器中。
builder.Services.AddSwaggerGen();

// 將控制器服務添加到依賴注入容器中。這將允許 ASP.NET Core MVC 使用控制器來處理 HTTP 請求。
builder.Services.AddControllers().AddNewtonsoftJson();

// 從應用程式設定中獲取連接字串。
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// 將 ApplicationDbContext 服務添加到依賴注入容器中，並配置使用 Npgsql 作為資料庫提供程序。
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 獲取 ApplicationDbContext 實例，並確保資料庫已創建。
builder.Services.BuildServiceProvider().GetService<ApplicationDbContext>().Database.EnsureCreated();

var app = builder.Build();

// Configure the HTTP request pipeline.
// 配置 HTTP 請求管道。
if (app.Environment.IsDevelopment())
{
    // 如果應用程式正在開發模式下運行，則使用 Swagger 和 Swagger UI。
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 將 HTTP 請求重定向到 HTTPS。
app.UseHttpsRedirection();

// 使用授權中間件。
app.UseAuthorization();

// 將路由映射到控制器動作。
app.MapControllers();

// 啟動應用程式。
app.Run();