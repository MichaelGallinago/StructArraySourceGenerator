using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using StructArrayAttributes;

namespace StructArraySourceGenerator;

[Generator]
internal class StructArrayGenerator : IIncrementalGenerator
{
    private record struct StructArrayData(string Name, string Namespace, byte Size);

    private const string AttributeName = nameof(StructArrayAttribute);
    private const string FullAttributeName = nameof(StructArrayAttributes) + "." + AttributeName;
    
    private static readonly StringBuilder FileBuilder = new();
    
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
    
    private static void Build(SourceProductionContext context, ImmutableArray<StructArrayData?> structArrays)
    {
        if (structArrays.IsDefaultOrEmpty) return;
        
        IEnumerable<StructArrayData> filteredStructArrays = structArrays
            .Where(item => item.HasValue)
            .Select(item => item!.Value);
        
        FileBuilder.Clear();
        FileBuilder.AppendLine("using System;");
        FileBuilder.AppendLine("using System.Runtime.CompilerServices;");
        
        Parallel.ForEach(filteredStructArrays, static structArray =>
        {
            string structArrayString = CreateStructArrayString(structArray);
            lock (FileBuilder)
            {
                FileBuilder.AppendLine(structArrayString);
            }
        });
        
        context.AddSource("StructArray.g.cs", SourceText.From(FileBuilder.ToString(), Encoding.UTF8));
    }

    private static string CreateStructArrayString(StructArrayData structArray) => new StringBuilder().AppendLine(
$$"""

namespace {{structArray.Namespace}}
{
    [Serializable]
    public struct {{structArray.Name}}<T>
    {
        public const byte Length = {{structArray.Size.ToString()}};
      
"""
).AddFields(structArray.Size).AppendLine(
"""

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetValue(index);
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set => SetValue(index, value);
        }

        public T[] ToArray()
        {
            var array = new T[Length];
            for (int i = 0; i < Length; i++) 
            { 
                array[i] = GetValue(i);
            }
            return array;
        }

        private T GetValue(int index) => index switch
        {
"""
).AddCasesToGetValue(structArray.Size).AppendLine(
"""
        };

        private void SetValue(int index, T value)
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
