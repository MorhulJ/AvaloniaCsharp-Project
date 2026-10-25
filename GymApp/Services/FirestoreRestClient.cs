using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Google.Cloud.Firestore;

// Small Firestore REST client that authenticates as the signed-in Firebase user.
// Firestore Security Rules therefore apply; this client has no service-account key.
public sealed class FirestoreDb
{
    private readonly string _projectId;
    private readonly Func<Task<string>> _getIdToken;
    private static readonly HttpClient Http = new();

    private FirestoreDb(string projectId, Func<Task<string>> getIdToken)
    {
        _projectId = projectId;
        _getIdToken = getIdToken;
    }

    public static FirestoreDb Create(string projectId, Func<Task<string>> getIdToken) => new(projectId, getIdToken);
    public CollectionReference Collection(string path) => new(this, path);

    internal string DocumentUrl(string path) => $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents/{path}";

    internal async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? payload = null, string? query = null)
    {
        var url = DocumentUrl(path) + (string.IsNullOrEmpty(query) ? "" : "?" + query);
        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _getIdToken());
        if (payload is not null)
            request.Content = new StringContent(JsonSerializer.Serialize(new { fields = ToFirestoreFields(payload) }), System.Text.Encoding.UTF8, "application/json");
        return await Http.SendAsync(request);
    }

    internal static Dictionary<string, object> ToFirestoreFields(object value)
    {
        var element = JsonSerializer.SerializeToElement(value);
        var fields = new Dictionary<string, object>();
        foreach (var property in element.EnumerateObject()) fields[property.Name] = Encode(property.Value);
        return fields;
    }

    private static object Encode(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => new { stringValue = value.GetString() },
        JsonValueKind.Number when value.TryGetInt64(out var integer) => new { integerValue = integer.ToString(CultureInfo.InvariantCulture) },
        JsonValueKind.Number => new { doubleValue = value.GetDouble() },
        JsonValueKind.True => new { booleanValue = true },
        JsonValueKind.False => new { booleanValue = false },
        JsonValueKind.Null => new { nullValue = (string?)null },
        _ => throw new NotSupportedException("Nested Firestore values are not supported by this client.")
    };

    internal static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync();
        var method = response.RequestMessage?.Method.Method ?? "unknown";
        var url = response.RequestMessage?.RequestUri?.AbsolutePath ?? "unknown path";
        throw new HttpRequestException($"Firestore {method} {url} failed ({(int)response.StatusCode}): {body}", null, response.StatusCode);
    }

    internal static JsonElement ParseFields(JsonElement document) =>
        document.TryGetProperty("fields", out var fields) ? fields : default;
}

public sealed class CollectionReference
{
    private readonly FirestoreDb _db;
    private readonly string _path;
    internal CollectionReference(FirestoreDb db, string path) { _db = db; _path = path.Trim('/'); }
    public DocumentReference Document(string id) => new(_db, $"{_path}/{Uri.EscapeDataString(id)}");

    public async Task<QuerySnapshot> GetSnapshotAsync()
    {
        var results = new List<DocumentSnapshot>();
        string? pageToken = null;
        do
        {
            var query = "pageSize=1000" + (pageToken is null ? "" : "&pageToken=" + Uri.EscapeDataString(pageToken));
            using var response = await _db.SendAsync(HttpMethod.Get, _path, query: query);
            await FirestoreDb.EnsureSuccessAsync(response);
            using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            if (json.RootElement.TryGetProperty("documents", out var documents))
                foreach (var doc in documents.EnumerateArray()) results.Add(DocumentSnapshot.FromJson(doc));
            pageToken = json.RootElement.TryGetProperty("nextPageToken", out var next) ? next.GetString() : null;
        } while (!string.IsNullOrEmpty(pageToken));
        return new QuerySnapshot(results);
    }

    public async Task<DocumentReference> AddAsync(object data)
    {
        using var response = await _db.SendAsync(HttpMethod.Post, _path, data);
        await FirestoreDb.EnsureSuccessAsync(response);
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var name = json.RootElement.GetProperty("name").GetString()!;
        return new DocumentReference(_db, name[(name.LastIndexOf('/') + 1)..]);
    }
}

public sealed class DocumentReference
{
    private readonly FirestoreDb _db;
    private readonly string _path;
    internal DocumentReference(FirestoreDb db, string path) { _db = db; _path = path.Trim('/'); }
    public string Id => Uri.UnescapeDataString(_path[( _path.LastIndexOf('/') + 1)..]);
    public CollectionReference Collection(string path) => new(_db, $"{_path}/{path}");

    public async Task<DocumentSnapshot> GetSnapshotAsync()
    {
        using var response = await _db.SendAsync(HttpMethod.Get, _path);
        if (response.StatusCode == HttpStatusCode.NotFound) return DocumentSnapshot.Missing;
        await FirestoreDb.EnsureSuccessAsync(response);
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return DocumentSnapshot.FromJson(json.RootElement);
    }

    public async Task SetAsync(object data)
    {
        using var response = await _db.SendAsync(HttpMethod.Patch, _path, data);
        await FirestoreDb.EnsureSuccessAsync(response);
    }

    public async Task UpdateAsync(IDictionary<string, object> data)
    {
        var mask = string.Join("&", data.Keys.Select(k => "updateMask.fieldPaths=" + Uri.EscapeDataString(k)));
        using var response = await _db.SendAsync(HttpMethod.Patch, _path, data, mask);
        await FirestoreDb.EnsureSuccessAsync(response);
    }

    public async Task DeleteAsync()
    {
        using var response = await _db.SendAsync(HttpMethod.Delete, _path);
        await FirestoreDb.EnsureSuccessAsync(response);
    }
}

public sealed class QuerySnapshot
{
    public IReadOnlyList<DocumentSnapshot> Documents { get; }
    internal QuerySnapshot(IReadOnlyList<DocumentSnapshot> documents) => Documents = documents;
}

public sealed class DocumentSnapshot
{
    private readonly JsonElement _fields;
    public static DocumentSnapshot Missing => new(false, "", default);
    public bool Exists { get; }
    public string Id { get; }
    private DocumentSnapshot(bool exists, string id, JsonElement fields) { Exists = exists; Id = id; _fields = fields; }
    internal static DocumentSnapshot FromJson(JsonElement doc)
    {
        var name = doc.GetProperty("name").GetString()!;
        // The response JsonDocument is disposed by the caller after this method.
        // Clone the fields so this snapshot can safely be read afterwards.
        return new DocumentSnapshot(true, name[(name.LastIndexOf('/') + 1)..], FirestoreDb.ParseFields(doc).Clone());
    }
    public bool ContainsField(string field) => _fields.ValueKind == JsonValueKind.Object && _fields.TryGetProperty(field, out _);
    public T GetValue<T>(string field)
    {
        if (!ContainsField(field)) throw new KeyNotFoundException($"Firestore field '{field}' is missing.");
        var value = _fields.GetProperty(field);
        var raw = value.TryGetProperty("stringValue", out var s) ? s.GetString()
            : value.TryGetProperty("integerValue", out var i) ? i.GetString()
            : value.TryGetProperty("doubleValue", out var d) ? d.GetDouble().ToString(CultureInfo.InvariantCulture)
            : value.TryGetProperty("booleanValue", out var b) ? b.GetBoolean().ToString()
            : null;
        if (raw is null) return default!;
        var target = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        if (target == typeof(string)) return (T)(object)raw;
        if (target.IsEnum) return (T)Enum.ToObject(target, Convert.ToInt32(raw, CultureInfo.InvariantCulture));
        return (T)Convert.ChangeType(raw, target, CultureInfo.InvariantCulture);
    }
}
