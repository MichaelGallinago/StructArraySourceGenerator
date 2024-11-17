using Microsoft.CodeAnalysis;

namespace StructArraySourceGenerator;

internal static class AttributeDataExtensions
{
    internal static T GetConstructorArgument<T>(this AttributeData attribute, int index)
    {
        return (T)attribute.ConstructorArguments[index].Value!;
    }
}   
