public interface IDatabaseEntity
{
    Guid Id { get; set; }
  
    public Guid UpdatedUser { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    bool IsEnable { get; set; }
    
}