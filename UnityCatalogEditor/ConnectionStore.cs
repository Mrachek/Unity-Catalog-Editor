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

    private readonly string filePath;

    public ConnectionStore()
    {
        var folderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "UnityCatalogEditor");

        Directory.CreateDirectory(folderPath);
        filePath = Path.Combine(folderPath, "saved-connections.json");
    }

    public IReadOnlyList<SavedConnection> LoadAll()
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        var json = File.ReadAllText(filePath);
        var connections = JsonSerializer.Deserialize<List<SavedConnection>>(json, JsonOptions) ?? [];
        return connections
            .OrderByDescending(connection => connection.LastUsedUtc)
            .ThenBy(connection => connection.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public void Upsert(SavedConnection savedConnection)
    {
        var connections = LoadAll()
            .Where(connection => !string.Equals(connection.Name, savedConnection.Name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        connections.Add(savedConnection);
        SaveAll(connections);
    }

    private void SaveAll(IEnumerable<SavedConnection> connections)
    {
        var orderedConnections = connections
            .OrderByDescending(connection => connection.LastUsedUtc)
            .ThenBy(connection => connection.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var json = JsonSerializer.Serialize(orderedConnections, JsonOptions);
        File.WriteAllText(filePath, json);
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
