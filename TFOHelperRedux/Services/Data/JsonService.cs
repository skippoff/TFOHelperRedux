using System.IO;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace TFOHelperRedux.Services.Data;

public static class JsonService
{
    private static readonly JsonSerializerOptions _opts = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static T? Load<T>(string path)
    {
        if (!File.Exists(path)) return default;
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(json, _opts);
    }

    public static void Save<T>(string path, T data)
    {
        var json = JsonSerializer.Serialize(data, _opts);
        File.WriteAllText(path, json);
    }
}
