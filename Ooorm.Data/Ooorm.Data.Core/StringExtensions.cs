using System.Text;

namespace Ooorm.Data.Core
{
    public static class StringExtensions
    {
        internal static StringBuilder Append(this string source, string data)
            => new StringBuilder(source).Append(data);

        internal static StringBuilder AppendLine(this string source, string data)
            => new StringBuilder(source).AppendLine(data);
    }
}
