using System;

namespace StructArrayAttributes;

[System.Diagnostics.Conditional("StructArray_Attributes")]
[AttributeUsage(AttributeTargets.Struct)]
public sealed class StructArrayAttribute(byte size) : Attribute
{
    public byte Size { get; } = size;
}
