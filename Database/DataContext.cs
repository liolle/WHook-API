using System.Data;
using System.Data.SQLite;

public interface IDataContext
{
  SQLiteConnection CreateConnection();
}

public partial class DataContext : IDataContext 
{
  private readonly string _connectionString = "";


  public DataContext(IConfiguration configuration)
  {
    _connectionString = configuration["DB_CONNECTION_STRING"] ?? throw new Exception("Missing configuration: DB_CONNECTION_STRING");
    InitDb();
  }

  public SQLiteConnection CreateConnection()
  {
    return new SQLiteConnection(_connectionString);  
  }

  public int ExecuteNonQuery(string query, SQLiteParameter[] parameters)
  {
    using SQLiteConnection conn = CreateConnection();
    using SQLiteCommand cmd = new(query, conn);

    if (parameters != null){cmd.Parameters.AddRange(parameters);}

    conn.Open();
    return cmd.ExecuteNonQuery(); 
  }

  public DataTable ExecuteQuery(string query, SQLiteParameter[] parameters)
  {
    using SQLiteConnection conn = CreateConnection();
    using SQLiteCommand cmd = new(query, conn);

    if (parameters != null){cmd.Parameters.AddRange(parameters);}

    conn.Open();

    using SQLiteDataAdapter adapter = new (cmd);
    DataTable resultTable = new();
    adapter.Fill(resultTable);
    return resultTable;
  }
}

// Helper methods
public partial class DataContext
{
  private void InitDb()
  {
    using var connection = new SQLiteConnection(_connectionString);
    connection.Open();

    var command = connection.CreateCommand();

    command.CommandText = @"
    CREATE TABLE IF NOT EXISTS AdminKeys (
        Value TEXT UNIQUE NOT NULL,
        PRIMARY KEY (Value)
    );

    CREATE TABLE IF NOT EXISTS ProjectKeys (
        ProjectId TEXT UNIQUE NOT NULL,
        ApiKey TEXT UNIQUE NOT NULL,
        PRIMARY KEY (ApiKey, ProjectId)
    );";

    command.ExecuteNonQuery();
  }
}

