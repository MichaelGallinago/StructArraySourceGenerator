using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace StructArraySourceGenerator;

[Generator]
public class StructArrayGenerator : IIncrementalGenerator
{
    private record struct StructArrayData(string Name, string Namespace, byte Size);
    
    private const string GeneratorNamespace = nameof(StructArrayGenerator);
    private const string AttributeName = nameof(StructArrayAttribute);
    private const string FullAttributeName = $"{GeneratorNamespace}.{AttributeName}";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<StructArrayData?>> provider = 
            context.SyntaxProvider.ForAttributeWithMetadataName(
                    FullAttributeName,
                    static (_, _) => true,
                    static (ctx, _) => GetSemanticTargetForGeneration(ctx))
                .Collect();

        context.RegisterSourceOutput(provider, Build);
    }
    
    private static StructArrayData? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
    {
        AttributeData? attribute = context.Attributes
            .FirstOrDefault(a => a.AttributeClass?.Name == AttributeName);
        
        if (attribute == null) return null;
        
        var size = attribute.GetConstructorArgument<byte>(2);
        if (size == 0) return null;
        
        return new StructArrayData(
            attribute.GetConstructorArgument<string>(0),
            attribute.GetConstructorArgument<string>(1),
            size);
    }
    
    private static void Build(
        SourceProductionContext context, ImmutableArray<StructArrayData?> structArrays)
    {
        if (structArrays.IsDefaultOrEmpty) return;
        
        IEnumerable<StructArrayData> filteredStructArrays = structArrays
            .Where(item => item.HasValue)
            .Select(item => item!.Value);
        
        Parallel.ForEach(filteredStructArrays, structArray => context.AddSource(
            "StructArray.g.cs", 
            SourceText.From(CreateFileString(structArray), 
            Encoding.UTF8)));
    }

    private static string CreateFileString(StructArrayData structArray) => new StringBuilder().AppendLine(
$$"""
namespace {{structArray.Namespace}}
{
    public struct {{structArray.Name}}<T>
    {
        public const byte Length => {{structArray.Size}};
        
"""
        ).AddFields(structArray.Size).AppendLine(
"""

        public T this[int index]
        {
            get => GetValue(index);
            set => SetValue(index, value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T GetValue<T>(int index) => index switch
        {
"""
        ).AddCasesToGetValue(structArray.Size).AppendLine(
"""
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetValue(int index, byte value)
        {
            switch (index)
            {
"""
        ).AddCasesToSetValue(structArray.Size).AppendLine(
"""
            }
        }
    }
}
"""
        ).ToString();
}
