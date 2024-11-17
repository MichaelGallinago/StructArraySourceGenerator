using System;

namespace StructArrayAttributes;

[System.Diagnostics.Conditional("StructArray_Attributes")]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class StructArrayAttribute(string name, string @namespace, byte size) : Attribute
{
    public string Name { get; } = name;
    public string Namespace { get; } = @namespace;
    public byte Size { get; } = size;
}
