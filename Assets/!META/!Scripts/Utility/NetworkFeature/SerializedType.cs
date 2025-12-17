using System;
using Mirror;

[Serializable]
public struct SerializedType : IEquatable<SerializedType>
{
   public string _assemblyQualifiedName;

    private Type _cachedType;
    private bool _typeCached;

    public SerializedType(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        
        _assemblyQualifiedName = type.AssemblyQualifiedName;
        _cachedType = type;
        _typeCached = true;
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок получения типа из строки
    // Критично для десериализации типов из сети - может возникнуть при несовпадении версий
    public Type Type
    {
        get
        {
            if (_typeCached)
                return _cachedType;

            if (string.IsNullOrEmpty(_assemblyQualifiedName))
            {
                _cachedType = null;
                _typeCached = true;
                return null;
            }

            try
            {
                _cachedType = Type.GetType(_assemblyQualifiedName);
                // Проверка на null тип - может быть если тип не найден в текущей сборке
                if (_cachedType == null)
                {
                    LoggerUtility.Warning($"Failed to get type from assembly qualified name: {_assemblyQualifiedName}");
                }
                _typeCached = true;
                return _cachedType;
            }
            catch (Exception ex)
            {
                // Логируем ошибку получения типа - может быть при проблемах с загрузкой сборок
                LoggerUtility.Error($"Error getting type from {_assemblyQualifiedName}: {ex.Message}\n{ex.StackTrace}");
                _cachedType = null;
                _typeCached = true;
                return null;
            }
        }
    }

    public bool IsValid => Type != null;

    public bool Equals(SerializedType other) => 
        _assemblyQualifiedName == other._assemblyQualifiedName;

    public override bool Equals(object obj) => 
        obj is SerializedType other && Equals(other);

    public override int GetHashCode() => 
        _assemblyQualifiedName?.GetHashCode() ?? 0;

    public static implicit operator Type(SerializedType serializedType) => 
        serializedType.Type;

    public static implicit operator SerializedType(Type type) => 
        new SerializedType(type);

    public override string ToString() => 
        _assemblyQualifiedName ?? "NULL";

    // Методы для сериализации/десериализации Mirror
    public void Serialize(NetworkWriter writer)
    {
        writer.WriteString(_assemblyQualifiedName ?? string.Empty);
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок десериализации типа из сети
    // Критично для сетевой синхронизации - может возникнуть при поврежденных данных
    public void Deserialize(NetworkReader reader)
    {
        try
        {
            _assemblyQualifiedName = reader.ReadString();
            _cachedType = null;
            _typeCached = false;
        }
        catch (Exception ex)
        {
            // Логируем ошибку десериализации - может быть при поврежденных сетевых данных
            LoggerUtility.Error($"Error deserializing SerializedType: {ex.Message}\n{ex.StackTrace}");
            _assemblyQualifiedName = string.Empty;
            _cachedType = null;
            _typeCached = false;
        }
    }
}
