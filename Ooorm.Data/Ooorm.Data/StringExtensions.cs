using System.Text;

namespace Ooorm.Data
{
    public static class StringExtensions
    {
        public static StringBuilder Append(this string source, string data)
            => new StringBuilder(source).Append(data);
        
        public static StringBuilder AppendLine(this string source, string data)
            => new StringBuilder(source).AppendLine(data);
    }
}
