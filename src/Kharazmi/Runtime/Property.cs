namespace Kharazmi.Runtime
{
    public readonly struct Property
    {
        public Property(object value, string name, string path, string typeName, string baseType, bool isGeneric,
            string[]? argumentsType = null)
        {
            Value = value;
            Name = name;
            Path = path;
            TypeName = typeName;
            BaseType = baseType;
            IsGeneric = isGeneric;
            ArgumentsType = argumentsType;
        }

        public static Property For(object valueType, string name, string path, string typeName, string baseType,
            bool isGeneric, string[]? argumentsType = null) =>
            new(valueType, name, path, typeName, baseType, isGeneric, argumentsType);

        public object Value { get; }
        public string Name { get; }
        public string Path { get; }
        public string TypeName { get; }
        public string BaseType { get; }
        public bool IsGeneric { get; }
        public string[]? ArgumentsType { get; }
    }
}