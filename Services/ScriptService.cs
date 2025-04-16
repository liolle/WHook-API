using System.Data.SQLite;
using whook.models;

namespace whook.services;

public interface IScriptService
{
  bool IsApiKeyValidForScript(ApiKeyPayload payload);
  string? GenerateApiKey(string project_id);
  string? GenerateAdminApiKey();
  bool IsAdmin (string key);

}

public class ScriptService(IDataContext context, KeyService keyService,IConfiguration configuration) : IScriptService
{
  public string? GenerateAdminApiKey()
  {
    string key = keyService.GenerateApiKey();
    using SQLiteConnection con = context.CreateConnection();

    var command = con.CreateCommand();
    command.CommandText = @"
      INSERT INTO AdminKeys(Value)
      VALUES (@AdminKey)";

    command.Parameters.AddWithValue("@AdminKey", key);
    try
    {
      con.Open();
      int result = command.ExecuteNonQuery();
      if (result < 1){return null; }
      return key;
    }
    catch (Exception e)
    {
      Console.WriteLine($"AdminKey Generation: {e.Message}");
      return null; 
    }
  }

  public string? GenerateApiKey(string project_id)
  {
    string key = keyService.GenerateApiKey();
    using SQLiteConnection con = context.CreateConnection();

    var command = con.CreateCommand();
    command.CommandText = @"
      INSERT INTO ProjectKeys(ApiKey,ProjectId)
      VALUES (@ApiKey,@ProjectId)";

    command.Parameters.AddWithValue("@ApiKey", key);
    command.Parameters.AddWithValue("@ProjectId", project_id);
    try
    {
      con.Open();
      int result = command.ExecuteNonQuery();
      if (result < 1){return null; }
      return key;
    }
    catch (Exception e)
    {
      Console.WriteLine($"Key Generation: {e.Message}");
      return null; 
    }
  }

  public bool IsAdmin(string key)
  {
    if (String.IsNullOrEmpty(key)){return false;}
    string ADMIN_KEY = configuration["ADMIN_KEY"] ?? "";
    if (!String.IsNullOrEmpty(ADMIN_KEY) && key == ADMIN_KEY){return true;}
    using SQLiteConnection con = context.CreateConnection();

    var command = con.CreateCommand();
    command.CommandText = @"
      SELECT * 
      FROM AdminKeys 
      WHERE Value = @Key";

    command.Parameters.AddWithValue("@Key", key);
    try
    {
      con.Open();
       
      SQLiteDataReader reader = command.ExecuteReader();
      if (reader.Read()){return true; }
      return false;
    }
    catch (Exception e)
    {
      Console.WriteLine($"Admin check: {e.Message}");
      return false; 
    }
  }

  public bool IsApiKeyValidForScript(ApiKeyPayload payload)
  {
    using SQLiteConnection con = context.CreateConnection();

    var command = con.CreateCommand();
    command.CommandText = @"
      SELECT ProjectId 
      FROM ProjectKeys 
      WHERE ProjectId = @ProjectId 
      AND ApiKey = @ApiKey";

    command.Parameters.AddWithValue("@ApiKey", payload.API_KEY);
    command.Parameters.AddWithValue("@ProjectId", payload.PROJECT_ID);

    try
    {
      con.Open();
  
      SQLiteDataReader reader = command.ExecuteReader();
      if (reader.Read()){return true; }
      return false;
    }
    catch (Exception e)
    {
      Console.WriteLine($"Key Validation: {e.Message}");
      return false; 
    }
  }
}
