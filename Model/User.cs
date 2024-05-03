using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class User: IDatabaseEntity
{
    public User()
    {
        IsEnable = true; // 將 IsEnable 屬性設置為 true
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    [Key]
    [Column(TypeName = "uuid")]
    public Guid Id { get; set; }
    
    [Required]
    public string Account { get; set; }

    [Required]
    public string Password { get; set; } // 儲存密碼的哈希值
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [RegularExpression(@"^\d{10,15}$")] // 限制電話號碼格式
    public string Phone { get; set; }

    public bool IsEnable { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdatedUser { get ; set ; }
}
