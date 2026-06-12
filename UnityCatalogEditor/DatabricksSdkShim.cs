using Azure.Core;
using Azure.Identity;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Databricks.Sdk;

public sealed class DatabricksConfig
{
    public string Host { get; set; } = string.Empty;

    public string AzureTenantId { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;
}

public sealed class WorkspaceClient : IDisposable
{
    private readonly DatabricksApiClient apiClient;

    public WorkspaceClient(DatabricksConfig config)
    {
        apiClient = new DatabricksApiClient(config);
        Catalogs = new CatalogsClient(apiClient);
        Schemas = new SchemasClient(apiClient);
        Grants = new GrantsClient(apiClient);
        Tables = new TablesClient(apiClient);
        Volumes = new VolumesClient(apiClient);
    }

    public CatalogsClient Catalogs { get; }

    public SchemasClient Schemas { get; }

    public GrantsClient Grants { get; }

    public TablesClient Tables { get; }

    public VolumesClient Volumes { get; }

    public void Dispose()
    {
        apiClient.Dispose();
    }
}

public sealed class CatalogsClient(DatabricksApiClient apiClient)
{
    public Task<IReadOnlyList<CatalogInfo>> ListAsync(CancellationToken cancellationToken = default)
    {
        return apiClient.ListAllAsync<CatalogInfo>(
            "/api/2.1/unity-catalog/catalogs",
            "catalogs",
            null,
            cancellationToken);
    }

    public Task DeleteAsync(string catalogName, bool force = false, CancellationToken cancellationToken = default)
    {
        return apiClient.DeleteAsync(
            $"/api/2.1/unity-catalog/catalogs/{Uri.EscapeDataString(catalogName)}",
            new Dictionary<string, string?>
            {
                ["force"] = force ? "true" : null
            },
            cancellationToken);
    }
}

public sealed class SchemasClient(DatabricksApiClient apiClient)
{
    public Task<IReadOnlyList<SchemaInfo>> ListAsync(string catalogName, CancellationToken cancellationToken = default)
    {
        return apiClient.ListAllAsync<SchemaInfo>(
            "/api/2.1/unity-catalog/schemas",
            "schemas",
            new Dictionary<string, string?>
            {
                ["catalog_name"] = catalogName
            },
            cancellationToken);
    }

    public Task CreateAsync(string name, string catalogName, CancellationToken cancellationToken = default)
    {
        return apiClient.PostAsync(
            "/api/2.1/unity-catalog/schemas",
            new CreateSchemaRequest
            {
                Name = name,
                CatalogName = catalogName
            },
            cancellationToken);
    }

    public Task DeleteAsync(string fullSchemaName, bool force = false, CancellationToken cancellationToken = default)
    {
        return apiClient.DeleteAsync(
            $"/api/2.1/unity-catalog/schemas/{Uri.EscapeDataString(fullSchemaName)}",
            new Dictionary<string, string?>
            {
                ["force"] = force ? "true" : null
            },
            cancellationToken);
    }
}

public sealed class GrantsClient(DatabricksApiClient apiClient)
{
    public Task<IReadOnlyList<PrivilegeAssignmentInfo>> GetAsync(
        string securableType,
        string fullName,
        CancellationToken cancellationToken = default)
    {
        return apiClient.ListAllAsync<PrivilegeAssignmentInfo>(
            $"/api/2.1/unity-catalog/permissions/{Uri.EscapeDataString(securableType)}/{Uri.EscapeDataString(fullName)}",
            "privilege_assignments",
            null,
            cancellationToken);
    }

    public Task<IReadOnlyList<PrivilegeAssignmentInfo>> GetEffectiveAsync(
        string securableType,
        string fullName,
        CancellationToken cancellationToken = default)
    {
        return apiClient.ListAllAsync<PrivilegeAssignmentInfo>(
            $"/api/2.1/unity-catalog/effective-permissions/{Uri.EscapeDataString(securableType)}/{Uri.EscapeDataString(fullName)}",
            "privilege_assignments",
            null,
            cancellationToken);
    }

    public Task UpdateAsync(
        string securableType,
        string fullName,
        IReadOnlyList<PrivilegeChange> changes,
        CancellationToken cancellationToken = default)
    {
        return apiClient.PatchAsync(
            $"/api/2.1/unity-catalog/permissions/{Uri.EscapeDataString(securableType)}/{Uri.EscapeDataString(fullName)}",
            new UpdatePrivilegesRequest
            {
                Changes = changes.ToList()
            },
            cancellationToken);
    }
}

public sealed class TablesClient(DatabricksApiClient apiClient)
{
    public Task<IReadOnlyList<TableInfo>> ListAsync(string catalogName, string schemaName, CancellationToken cancellationToken = default)
    {
        return apiClient.ListAllAsync<TableInfo>(
            "/api/2.1/unity-catalog/tables",
            "tables",
            new Dictionary<string, string?>
            {
                ["catalog_name"] = catalogName,
                ["schema_name"] = schemaName
            },
            cancellationToken);
    }

    public Task DeleteAsync(string fullTableName, CancellationToken cancellationToken = default)
    {
        return apiClient.DeleteAsync(
            $"/api/2.1/unity-catalog/tables/{Uri.EscapeDataString(fullTableName)}",
            null,
            cancellationToken);
    }
}

public sealed class VolumesClient(DatabricksApiClient apiClient)
{
    public Task<IReadOnlyList<VolumeInfo>> ListAsync(string catalogName, string schemaName, CancellationToken cancellationToken = default)
    {
        return apiClient.ListAllAsync<VolumeInfo>(
            "/api/2.1/unity-catalog/volumes",
            "volumes",
            new Dictionary<string, string?>
            {
                ["catalog_name"] = catalogName,
                ["schema_name"] = schemaName
            },
            cancellationToken);
    }

    public Task DeleteAsync(string fullVolumeName, CancellationToken cancellationToken = default)
    {
        return apiClient.DeleteAsync(
            $"/api/2.1/unity-catalog/volumes/{Uri.EscapeDataString(fullVolumeName)}",
            null,
            cancellationToken);
    }
}

public sealed class CatalogInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public sealed class SchemaInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public sealed class TableInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("full_name")]
    public string? FullName { get; set; }

    [JsonPropertyName("table_type")]
    public string? TableType { get; set; }
}

public sealed class VolumeInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("full_name")]
    public string? FullName { get; set; }

    [JsonPropertyName("volume_type")]
    public string? VolumeType { get; set; }
}

public sealed class PrivilegeAssignmentInfo
{
    [JsonPropertyName("principal")]
    public string Principal { get; set; } = string.Empty;

    [JsonPropertyName("privileges")]
    public List<GrantedPrivilege> Privileges { get; set; } = [];
}

public sealed class PrivilegeChange
{
    [JsonPropertyName("principal")]
    public string Principal { get; set; } = string.Empty;

    [JsonPropertyName("add")]
    public List<string>? Add { get; set; }

    [JsonPropertyName("remove")]
    public List<string>? Remove { get; set; }
}

[JsonConverter(typeof(GrantedPrivilegeJsonConverter))]
public sealed record GrantedPrivilege(string Name);

internal sealed class GrantedPrivilegeJsonConverter : JsonConverter<GrantedPrivilege>
{
    public override GrantedPrivilege? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new GrantedPrivilege(reader.GetString() ?? string.Empty);
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected privilege string or object.");
        }

        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        if (root.TryGetProperty("privilege", out var privilegeElement) &&
            privilegeElement.ValueKind == JsonValueKind.String)
        {
            return new GrantedPrivilege(privilegeElement.GetString() ?? string.Empty);
        }

        if (root.TryGetProperty("name", out var nameElement) &&
            nameElement.ValueKind == JsonValueKind.String)
        {
            return new GrantedPrivilege(nameElement.GetString() ?? string.Empty);
        }

        return new GrantedPrivilege(string.Empty);
    }

    public override void Write(Utf8JsonWriter writer, GrantedPrivilege value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("privilege", value.Name);
        writer.WriteEndObject();
    }
}

internal sealed class CreateSchemaRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("catalog_name")]
    public string CatalogName { get; set; } = string.Empty;
}

internal sealed class UpdatePrivilegesRequest
{
    [JsonPropertyName("changes")]
    public List<PrivilegeChange> Changes { get; set; } = [];
}

public sealed class DatabricksApiClient : IDisposable
{
    private const int PageSize = 150;
    private const string DatabricksScope = "2ff814a6-3304-4ab8-85cb-cd0e6f879c1d/.default";
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient httpClient;
    private readonly TokenCredential tokenCredential;
    private readonly SemaphoreSlim tokenLock = new(1, 1);
    private AccessToken cachedToken;

    public DatabricksApiClient(DatabricksConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.Host))
        {
            throw new ArgumentException("Host is required.", nameof(config));
        }

        var normalizedHost = NormalizeHost(config.Host);
        httpClient = new HttpClient
        {
            BaseAddress = new Uri(normalizedHost, UriKind.Absolute)
        };

        tokenCredential = new ClientSecretCredential(
            config.AzureTenantId,
            config.ClientId,
            config.ClientSecret);
    }

    public async Task<IReadOnlyList<T>> ListAllAsync<T>(
        string path,
        string itemsPropertyName,
        IReadOnlyDictionary<string, string?>? queryParameters,
        CancellationToken cancellationToken = default)
    {
        var results = new List<T>();
        string? pageToken = null;

        do
        {
            var pageQuery = queryParameters is null
                ? new Dictionary<string, string?>()
                : new Dictionary<string, string?>(queryParameters);

            pageQuery["max_results"] = PageSize.ToString();
            pageQuery["page_token"] = pageToken;

            var document = await GetJsonAsync(path, pageQuery, cancellationToken).ConfigureAwait(false);

            if (document.RootElement.TryGetProperty(itemsPropertyName, out var itemsElement) &&
                itemsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var itemElement in itemsElement.EnumerateArray())
                {
                    var item = itemElement.Deserialize<T>(SerializerOptions);
                    if (item is not null)
                    {
                        results.Add(item);
                    }
                }
            }

            pageToken = document.RootElement.TryGetProperty("next_page_token", out var nextPageTokenElement)
                ? nextPageTokenElement.GetString()
                : null;
        }
        while (!string.IsNullOrWhiteSpace(pageToken));

        return results;
    }

    public Task PostAsync<TBody>(string path, TBody body, CancellationToken cancellationToken = default)
        where TBody : class
    {
        return SendAsync(HttpMethod.Post, path, null, body, cancellationToken);
    }

    public Task PatchAsync<TBody>(string path, TBody body, CancellationToken cancellationToken = default)
        where TBody : class
    {
        return SendAsync(HttpMethod.Patch, path, null, body, cancellationToken);
    }

    public Task DeleteAsync(string path, IReadOnlyDictionary<string, string?>? queryParameters, CancellationToken cancellationToken = default)
    {
        return SendAsync<object?>(HttpMethod.Delete, path, queryParameters, null, cancellationToken);
    }

    private async Task<JsonDocument> GetJsonAsync(
        string path,
        IReadOnlyDictionary<string, string?>? queryParameters,
        CancellationToken cancellationToken)
    {
        using var response = await SendRequestAsync<object?>(HttpMethod.Get, path, queryParameters, null, cancellationToken).ConfigureAwait(false);
        return await ReadJsonDocumentAsync(response, cancellationToken).ConfigureAwait(false);
    }

    private async Task SendAsync<TBody>(
        HttpMethod method,
        string path,
        IReadOnlyDictionary<string, string?>? queryParameters,
        TBody? body,
        CancellationToken cancellationToken)
    {
        using var response = await SendRequestAsync(method, path, queryParameters, body, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> SendRequestAsync<TBody>(
        HttpMethod method,
        string path,
        IReadOnlyDictionary<string, string?>? queryParameters,
        TBody? body,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(method, BuildRequestUri(path, queryParameters));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false));

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body, SerializerOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (cachedToken.ExpiresOn > DateTimeOffset.UtcNow.AddMinutes(5))
        {
            return cachedToken.Token;
        }

        await tokenLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (cachedToken.ExpiresOn <= DateTimeOffset.UtcNow.AddMinutes(5))
            {
                cachedToken = await tokenCredential.GetTokenAsync(
                    new TokenRequestContext([DatabricksScope]),
                    cancellationToken).ConfigureAwait(false);
            }

            return cachedToken.Token;
        }
        finally
        {
            tokenLock.Release();
        }
    }

    private Uri BuildRequestUri(string path, IReadOnlyDictionary<string, string?>? queryParameters)
    {
        var builder = new UriBuilder(new Uri(httpClient.BaseAddress!, path));

        if (queryParameters is null || queryParameters.Count == 0)
        {
            return builder.Uri;
        }

        var queryParts = new List<string>();
        foreach (var (key, value) in queryParameters)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            queryParts.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
        }

        builder.Query = string.Join("&", queryParts);
        return builder.Uri;
    }

    private static async Task<JsonDocument> ReadJsonDocumentAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonDocument.Parse(body);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var message = string.IsNullOrWhiteSpace(body)
            ? $"Databricks request failed with {(int)response.StatusCode} ({response.ReasonPhrase})."
            : $"Databricks request failed with {(int)response.StatusCode} ({response.ReasonPhrase}): {body}";

        throw new HttpRequestException(message, null, response.StatusCode);
    }

    private static string NormalizeHost(string host)
    {
        host = host.Trim();
        if (!host.Contains("://", StringComparison.Ordinal))
        {
            host = "https://" + host;
        }

        return host.TrimEnd('/');
    }

    public void Dispose()
    {
        httpClient.Dispose();
        tokenLock.Dispose();
    }
}
