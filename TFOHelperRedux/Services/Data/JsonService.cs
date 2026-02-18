using System.IO;
using System.Text.Json;
using System.Text.Encodings.Web;
using Serilog;

namespace TFOHelperRedux.Services.Data;

public static class JsonService
{
    private static readonly ILogger _log = Log.ForContext(typeof(JsonService));
    
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
        _log.Verbose("Чтение JSON из {Path}", path);
        
        if (!File.Exists(path))
        {
            _log.Debug("Файл не найден: {Path}", path);
            return default;
        }
        
        try
        {
            var json = File.ReadAllText(path);
            _log.Verbose("Прочитано {Bytes} байт из {Path}", new FileInfo(path).Length, path);
            
            var result = JsonSerializer.Deserialize<T>(json, _opts);
            _log.Verbose("JSON успешно десериализован в {Type}", typeof(T).Name);
            
            return result;
        }
        catch (JsonException ex)
        {
            _log.Error(ex, "Ошибка десериализации JSON из {Path}", path);
            throw;
        }
        catch (IOException ex)
        {
            _log.Error(ex, "Ошибка чтения файла {Path}", path);
            throw;
        }
    }

    public static void Save<T>(string path, T data)
    {
        _log.Verbose("Запись JSON в {Path}", path);
        
        try
        {
            var json = JsonSerializer.Serialize(data, _opts);
            File.WriteAllText(path, json);
            _log.Verbose("Записано {Bytes} байт в {Path}", new FileInfo(path).Length, path);
        }
        catch (JsonException ex)
        {
            _log.Error(ex, "Ошибка сериализации JSON в {Path}", path);
            throw;
        }
        catch (IOException ex)
        {
            _log.Error(ex, "Ошибка записи файла {Path}", path);
            throw;
        }
    }
}
