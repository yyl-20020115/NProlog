using System.Text;

namespace Org.NProlog.Core.Terms;

public static class StringUtils
{
    public static string PatchDoubleString(this double d) => PatchDoubleString(d.ToString());

    public static string PatchDoubleString(this string d) => d.Contains('.') ? d : d + ".0";

    public static string ToString(this IEnumerable<string> set)
    {
        var builder = new StringBuilder();
        builder.Append('[');
        builder.Append(string.Join(", ", set));
        builder.Append(']');
        return builder.ToString();
    }

}
