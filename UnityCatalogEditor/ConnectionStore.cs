using Databricks.Sdk;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UnityCatalogEditor;

internal sealed class ConnectionStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly string connectionsDirectory;

    public ConnectionStore()
    {
        connectionsDirectory = AppContext.BaseDirectory.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);

        Directory.CreateDirectory(connectionsDirectory);
    }

    public IReadOnlyList<SavedConnection> LoadAll()
    {
        if (!Directory.Exists(connectionsDirectory))
        {
            return [];
        }

        var connections = new List<SavedConnection>();
        foreach (var filePath in Directory.EnumerateFiles(connectionsDirectory, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var connection = JsonSerializer.Deserialize<SavedConnection>(json, JsonOptions);
                if (connection is not null && !string.IsNullOrWhiteSpace(connection.Name))
                {
                    connections.Add(connection);
                }
            }
            catch
            {
                // Ignore malformed or unreadable connection files and continue loading the rest.
            }
        }

        return connections
            .OrderByDescending(connection => connection.LastUsedUtc)
            .ThenBy(connection => connection.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public void Upsert(SavedConnection savedConnection)
    {
        var filePath = GetFilePath(savedConnection.Name);
        var json = JsonSerializer.Serialize(savedConnection, JsonOptions);
        File.WriteAllText(filePath, json);
    }

    public string GetFilePath(string connectionName)
    {
        var safeName = SanitizeFileName(connectionName);
        return Path.Combine(connectionsDirectory, $"{safeName}.json");
    }

    private static string SanitizeFileName(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);

        foreach (var character in value.Trim())
        {
            builder.Append(invalidChars.Contains(character) ? '_' : character);
        }

        var safeName = builder.ToString().Trim();
        return string.IsNullOrWhiteSpace(safeName) ? "connection" : safeName;
    }
}

internal sealed record SavedConnection
{
    public string Name { get; init; } = string.Empty;

    public string Host { get; init; } = string.Empty;

    public string AzureTenantId { get; init; } = string.Empty;

    public string ClientId { get; init; } = string.Empty;

    public string ProtectedClientSecret { get; init; } = string.Empty;

    public DateTimeOffset LastUsedUtc { get; init; }

    [JsonIgnore]
    public string ClientSecret => ConnectionProtection.Unprotect(ProtectedClientSecret);

    public override string ToString()
    {
        return Name;
    }

    public DatabricksConfig ToConfig()
    {
        return new DatabricksConfig
        {
            Host = Host,
            AzureTenantId = AzureTenantId,
            ClientId = ClientId,
            ClientSecret = ClientSecret
        };
    }

    public static SavedConnection Create(string name, DatabricksConfig config)
    {
        return new SavedConnection
        {
            Name = name,
            Host = config.Host,
            AzureTenantId = config.AzureTenantId,
            ClientId = config.ClientId,
            ProtectedClientSecret = ConnectionProtection.Protect(config.ClientSecret),
            LastUsedUtc = DateTimeOffset.UtcNow
        };
    }
}

internal static class ConnectionProtection
{
    public static string Protect(string value)
    {
        var inputBytes = Encoding.UTF8.GetBytes(value);
        var protectedBytes = ProtectedData.Protect(inputBytes, optionalEntropy: null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(protectedBytes);
    }

    public static string Unprotect(string protectedValue)
    {
        var inputBytes = Convert.FromBase64String(protectedValue);
        var unprotectedBytes = ProtectedData.Unprotect(inputBytes, optionalEntropy: null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(unprotectedBytes);
    }
}
