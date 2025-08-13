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

            _cachedType = Type.GetType(_assemblyQualifiedName);
            _typeCached = true;
            return _cachedType;
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

    public void Deserialize(NetworkReader reader)
    {
        _assemblyQualifiedName = reader.ReadString();
        _cachedType = null;
        _typeCached = false;
    }
}
