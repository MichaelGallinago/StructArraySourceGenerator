using System.Text;

namespace StructArraySourceGenerator;

internal static class StringBuilderExtensions
{
    internal static StringBuilder AddFields(this StringBuilder builder, byte number)
    {
        for (var i = 0; i < number; i++)
        {
            builder.Append("        private T _value").Append(i).AppendLine(";");
        }
        return builder;
    }
    
    internal static StringBuilder AddCasesToGetValue(this StringBuilder builder, byte number)
    {
        for (var i = 0; i < number; i++)
        {
            builder.Append("            ").Append(i).Append(" => _value").Append(i).AppendLine(",");
        }
        return builder.AppendLine("            _ => throw new System.IndexOutOfRangeException()");
    }
    
    internal static StringBuilder AddCasesToSetValue(this StringBuilder builder, byte number)
    {
        for (var i = 0; i < number; i++)
        {
            builder.Append("                case ")
                .Append(i).Append(": _value").Append(i).AppendLine(" = value; break;");
        }
        return builder.AppendLine("                default: throw new System.IndexOutOfRangeException();");
    }
}