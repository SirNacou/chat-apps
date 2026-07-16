using Npgsql;

namespace Server.Infrastructure.Options;

public class DatabaseOptions
{
    public const string SectionName = "Database";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 5432;
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // Computed property to generate the valid Npgsql connection string dynamically
    public string ConnectionString => new NpgsqlConnectionStringBuilder()
    {
        Host = Host,
        Port = Port,
        Database = Database,
        Username = Username,
        Password = Password,
        Pooling = true
    }.ConnectionString;
}