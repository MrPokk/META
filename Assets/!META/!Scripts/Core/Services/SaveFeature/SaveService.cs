using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

public static class SaveService
{
    [Serializable]
    private class SaveData
    {
        public Dictionary<string, object> SimpleData { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, string> ComplexData { get; set; } = new Dictionary<string, string>();
    }

    private static readonly JsonSerializerSettings s_jsonSettings = new()
    {
        Formatting = Formatting.Indented,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        NullValueHandling = NullValueHandling.Include,
        TypeNameHandling = TypeNameHandling.Auto
    };

    private static SaveData s_saveData = new();
    private static readonly string s_addictiveSaveFilePath = "data.json";
    private static string s_saveFilePath = Path.Combine(Application.persistentDataPath, s_addictiveSaveFilePath);
    private static bool s_isInitialized = false;

    public static void Initialize(string filePath = null)
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            s_saveFilePath = filePath;
        }
        
        LoadData();
        s_isInitialized = true;
    }

    public static void Save(string key, object value)
    {
        if (!s_isInitialized) Initialize();
        
        if (IsSimpleType(value))
        {
            s_saveData.SimpleData[key] = value;
        }
        else
        {
            var json = JsonConvert.SerializeObject(value, s_jsonSettings);
            s_saveData.ComplexData[key] = json;
        }
        
        SaveToFile();
    }

    public static T Load<T>(string key, T defaultValue = default)
    {
        if (!s_isInitialized) Initialize();
        
        if (IsSimpleType(typeof(T)))
        {
            if (s_saveData.SimpleData.TryGetValue(key, out var value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
        }
        else
        {
            if (s_saveData.ComplexData.TryGetValue(key, out var json))
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(json, s_jsonSettings);
                }
                catch
                {
                    return defaultValue;
                }
            }
        }
        
        return defaultValue;
    }

    public static float LoadFloat(string key, float defaultValue = 0)
    {
        if (s_saveData.SimpleData.TryGetValue(key, out var value))
        {
            return value switch
            {
                float f => f,
                int i => i,
                double d => (float)d,
                string s when float.TryParse(s, out var result) => result,
                _ => defaultValue
            };
        }
        return defaultValue;
    }

    public static int LoadInt(string key, int defaultValue = 0)
    {
        if (s_saveData.SimpleData.TryGetValue(key, out var value))
        {
            return value switch
            {
                int i => i,
                float f => (int)f,
                double d => (int)d,
                string s when int.TryParse(s, out var result) => result,
                _ => defaultValue
            };
        }
        return defaultValue;
    }

    public static string LoadString(string key, string defaultValue = "")
    {
        if (s_saveData.SimpleData.TryGetValue(key, out var value))
        {
            return value.ToString();
        }
        return defaultValue;
    }

    public static bool LoadBool(string key, bool defaultValue = false)
    {
        if (s_saveData.SimpleData.TryGetValue(key, out var value))
        {
            return value switch
            {
                bool b => b,
                int i => i != 0,
                float f => f != 0,
                string s => s.ToLower() == "true",
                _ => defaultValue
            };
        }
        return defaultValue;
    }

    public static void Delete(string key)
    {
        s_saveData.SimpleData.Remove(key);
        s_saveData.ComplexData.Remove(key);
        SaveToFile();
    }

    public static bool HasKey(string key)
    {
        return s_saveData.SimpleData.ContainsKey(key) || s_saveData.ComplexData.ContainsKey(key);
    }

    public static void ClearAll()
    {
        s_saveData.SimpleData.Clear();
        s_saveData.ComplexData.Clear();
        SaveToFile();
    }

    public static void DeleteSaveFile()
    {
        if (File.Exists(s_saveFilePath))
        {
            File.Delete(s_saveFilePath);
        }
        s_saveData = new SaveData();
        s_isInitialized = false;
    }

    private static void SaveToFile()
    {
        try
        {
            var json = JsonConvert.SerializeObject(s_saveData, s_jsonSettings);
            var directory = Path.GetDirectoryName(s_saveFilePath);
            
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(s_saveFilePath, json, Encoding.UTF8);
        }
        catch (Exception e)
        {
            Debug.LogError($"Save error: {e.Message}");
        }
    }

    private static void LoadData()
    {
        if (!File.Exists(s_saveFilePath))
        {
            s_saveData = new SaveData();
            return;
        }

        try
        {
            var json = File.ReadAllText(s_saveFilePath, Encoding.UTF8);
            s_saveData = JsonConvert.DeserializeObject<SaveData>(json, s_jsonSettings) ?? new SaveData();
        }
        catch (Exception e)
        {
            Debug.LogError($"Load error: {e.Message}");
            s_saveData = new SaveData();
        }
    }

    private static bool IsSimpleType(object value)
    {
        if (value == null) return true;
        return IsSimpleType(value.GetType());
    }

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive 
            || type == typeof(string) 
            || type == typeof(decimal)
            || type.IsEnum
            || type == typeof(DateTime)
            || type == typeof(TimeSpan);
    }
}
