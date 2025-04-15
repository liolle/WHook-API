namespace whook.database;

public class ApiKeyEntity
{
  public int Id { get; set; }
  public string Value { get; set; } = "";
  public bool IsActive {get;set;}
  public DateTime CreatedAt { get; set; }
}
