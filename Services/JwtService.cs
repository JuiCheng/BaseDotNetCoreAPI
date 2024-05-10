using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public class JwtService
{
    private readonly string _issuer;
    private readonly string _secretKey;

    // JwtService 的建構函式，接收發行者和密鑰作為參數
    public JwtService(string issuer, string secretKey)
    {
        _issuer = issuer; // 將發行者保存在私有欄位中
        _secretKey = secretKey; // 將密鑰保存在私有欄位中
    }

    // 生成 JWT 的方法，接收用戶名作為參數，並返回生成的 JWT 字串
    public string GenerateToken(string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler(); // 創建 JWT Security Token Handler

        // 將 密鑰 轉換為 UTF-8 編碼的位元組陣列，以便用於加密或解密操作。
        var key = Encoding.UTF8.GetBytes(_secretKey);

        // 設定 JWT 的描述，包括主題、過期時間和簽名驗證方式
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, username) // 添加用戶名到聲明中
            }),
            Expires = DateTime.UtcNow.AddDays(7), // 設定過期時間為當前時間後的7天
            Issuer = _issuer, // 設定發行者
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature) // 使用 HMAC SHA-256 簽名方式
        };

        // 生成 JWT
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        // 將 JWT 轉換為字串並返回
        return tokenHandler.WriteToken(token);
    }

    // 解析 JWT 的方法，接收 JWT 字串作為參數，返回 JWT 包含的聲明（ClaimsPrincipal）
    public ClaimsPrincipal GetPrincipal(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler(); // 創建 JWT Security Token Handler

        // 將密鑰從 Base64 字串轉換為 byte 陣列
        var key = Convert.FromBase64String(_secretKey);

        // 設定 JWT 驗證參數，包括驗證簽名和設置時鐘偏移量為零
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true, // 驗證簽名金鑰是否有效
            IssuerSigningKey = new SymmetricSecurityKey(key), // 設置簽名金鑰
            ValidateIssuer = true, // 驗證發行者
            ValidIssuer = _issuer, // 設定有效的發行者
            ValidateAudience = false, // 不驗證收眾者
            ValidateLifetime = true, // 驗證 JWT 是否已過期
            ClockSkew = TimeSpan.Zero // 設置時鐘偏移量為零
        };

        // 驗證 JWT，並返回解析後的聲明
        ClaimsPrincipal principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
        return principal;
    }
}
