using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class LoginLog : IDatabaseEntity
{
    public LoginLog()
    {
        IsEnable = true; // 將 IsEnable 屬性設置為 true
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    [Key]
    [Column(TypeName = "uuid")]
    public Guid Id { get; set; }
    
    [Required]
    [ForeignKey("User")]
    public Guid UserId { get; set; } // 登錄使用者的 ID
    
    [Required]
    public DateTime LoginTime { get; set; } // 登錄時間
    // 如果需要記錄登出時間，可以添加以下屬性
    public DateTime? LogoutTime { get; set; } // 登出時間

    // 如果需要記錄登錄 IP，可以添加以下屬性
    public string IPAddress { get; set; } // 登錄 IP 地址

    // 如果需要記錄登錄地點，可以添加以下屬性
    public string Location { get; set; } // 登錄地點
    
    public bool IsEnable { get; set; }

    public Guid UpdatedUser { get ; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get ; set; }
    
}
