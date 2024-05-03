using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Role: IDatabaseEntity
{
    public Role()
    {
        IsEnable = true; // 將 IsEnable 屬性設置為 true
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    [Key]
    [Column(TypeName = "uuid")]
    public Guid Id { get; set; }
    
    [Required]
    public string Name { get; set; }

    public bool IsEnable { get; set; }

    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    public Guid UpdatedUser { get ; set; }
   
}
