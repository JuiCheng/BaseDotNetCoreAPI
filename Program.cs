using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // 引入 Entity Framework Core 命名空間

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

// 注入 JwtService 服務
builder.Services.AddSingleton<JwtService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    // 根據應用程式的環境來獲取對應的 Issuer
    var issuer = configuration["Jwt:Issuer"];
    // 這裡的 secretKey 應該是從安全的位置（比如 appsettings.json）加載的
    var secretKey = configuration["Jwt:SecretKey"];
    return new JwtService(issuer, secretKey);
});

// 設定 JWT 驗證參數
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // 將 密鑰 轉換為 UTF-8 編碼的位元組陣列，以便用於加密或解密操作。
        var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]);
        // 創建並設置 JWT 的驗證參數
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // 設置是否驗證發行者
            ValidateIssuer = false,
            // 設置是否驗證收眾者
            ValidateAudience = false,
            // 從應用程式的配置中獲取 JWT 的有效發行者
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            // 從應用程式的配置中獲取 JWT 的有效收眾者
            ValidAudience = builder.Configuration["Jwt:Audience"],

            // 設置是否驗證 JWT 是否已過期
            ValidateLifetime = true,
            // 設置是否驗證簽名金鑰
            ValidateIssuerSigningKey = true,
            // 使用 UTF-8 編碼將 JWT 的密鑰（Secret Key）從配置中取出並轉換為 SymmetricSecurityKey
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

// 定義 Swagger 文件的生成設置，包括 API 版本信息等
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    // 定義 JWT Bearer 的 SecurityScheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization", // 在 HTTP Header 中的授權標頭名稱
        Type = SecuritySchemeType.ApiKey, // 指定授權類型為 API 金鑰
        Scheme = "Bearer", // 設置授權方案為 Bearer
        BearerFormat = "JWT", // Bearer 的格式為 JWT
        In = ParameterLocation.Header, // 指定 JWT 在 HTTP Header 中的位置
        Description = "JWT Authorization" // 描述授權方式
    });

    // 在 Swagger 的 Operation 中使用 JWT 驗證
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { } // 設置該操作需要的授權，這裡是空的，表示不需要特定權限
        }
    });
});


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