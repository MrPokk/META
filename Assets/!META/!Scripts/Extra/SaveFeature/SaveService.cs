using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

public partial class SaveService
{
    [Serializable]
    private class SaveData
    {
        public Dictionary<SaveKey, object> SimpleData { get; set; } = new Dictionary<SaveKey, object>();
        public Dictionary<SaveKey, string> ComplexData { get; set; } = new Dictionary<SaveKey, string>();
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
    private static readonly string s_saveFilePath = Path.Combine(Application.persistentDataPath, s_addictiveSaveFilePath);

    public SaveService()
    {
        LoadData();
    }

    public static void Save(SaveKey key, object value)
    {
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

    public static T Load<T>(SaveKey key, T defaultValue = default)
    {
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

    public static float LoadFloat(SaveKey key, float defaultValue = 0)
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

    public static int LoadInt(SaveKey key, int defaultValue = 0)
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

    public static string LoadString(SaveKey key, string defaultValue = "")
    {
        if (s_saveData.SimpleData.TryGetValue(key, out var value))
        {
            return value.ToString();
        }
        return defaultValue;
    }

    public static bool LoadBool(SaveKey key, bool defaultValue = false)
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

    public static void Delete(SaveKey key)
    {
        s_saveData.SimpleData.Remove(key);
        s_saveData.ComplexData.Remove(key);
        SaveToFile();
    }

    public static bool HasKey(SaveKey key)
    {
        return s_saveData.SimpleData.ContainsKey(key) || s_saveData.ComplexData.ContainsKey(key);
    }

    public static void DeleteSaveFile()
    {
        if (File.Exists(s_saveFilePath))
        {
            File.Delete(s_saveFilePath);
        }
        s_saveData = new SaveData();
    }

    private static void SaveToFile()
    {
        try
        {
            var json = JsonConvert.SerializeObject(s_saveData, s_jsonSettings);

            File.WriteAllText(s_saveFilePath, json, Encoding.UTF8);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Save] Save error: {e.Message}");
        }
    }

    private static void LoadData()
    {
        if (!File.Exists(s_saveFilePath))
        {
            s_saveData = new SaveData();
            Debug.Log($"[Save] No save file found, starting fresh. Expected path: {s_saveFilePath}");
            return;
        }

        try
        {
            var json = File.ReadAllText(s_saveFilePath, Encoding.UTF8);
            s_saveData = JsonConvert.DeserializeObject<SaveData>(json, s_jsonSettings) ?? new SaveData();
            Debug.Log($"[Save] Data loaded from: {s_saveFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Save] Load error: {e.Message}");
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
