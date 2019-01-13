using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ooorm.Data.Core.Reflection
{
    public static class TypeExtensions
    {
        const BindingFlags PROPS = BindingFlags.Public | BindingFlags.Instance;

        internal static bool HasAttribute<TAttribute>(this Type type) where TAttribute : Attribute
        {
            var attr = type.GetCustomAttribute<TAttribute>();
            return attr != null;
        }

        internal static bool TryGetAttribute<TAttribute>(this Type type, out TAttribute attribute) where TAttribute : Attribute
        {
            var attr = type.GetCustomAttribute<TAttribute>();
            if (attr != null)
                attribute = attr;
            else
                attribute = default;
            return attr != null;
        }

        internal static bool HasAttribute<TAttribute>(this MemberInfo member) where TAttribute : Attribute
        {
            var attr = member.GetCustomAttribute<TAttribute>();
            return attr != null;
        }

        internal static bool TryGetAttribute<TAttribute>(this MemberInfo member, out TAttribute attribute) where TAttribute : Attribute
        {
            var attr = member.GetCustomAttribute<TAttribute>();
            if (attr != null)
                attribute = attr;
            else
                attribute = default;
            return attr != null;
        }

        internal static IEnumerable<Column<T>> GetColumns<T>(this T value, bool exceptId = false) where T : IDbItem
            => typeof(T).GetProperties(PROPS)
                    .Where(p => !p.HasAttribute<DbIgnoreAttribute>())
                    .Where(p => !(exceptId && (p.HasAttribute<IdAttribute>() || p.Name == nameof(IDbItem.ID))) )
                    .Select(p => new Column<T>(p));

        public static IEnumerable<Column> GetColumns(this Type type, bool exceptId = false)
        {
            var props = type.GetProperties(PROPS).ToArray();
            var fields = props.Where(p => !p.HasAttribute<DbIgnoreAttribute>()).ToArray();
            var notId = fields.Where(p => !(exceptId && (p.HasAttribute<IdAttribute>() || p.Name == nameof(IDbItem.ID))) ).ToArray();
            var columns = notId.Select(p => new Column(p)).ToArray();
            return columns;
        }

        internal static IEnumerable<Property<T>> GetDataProperties<T>(this T value)
            => typeof(T).GetProperties(PROPS)
                    .Select(p => new Property<T>(p));

        internal static IEnumerable<Property> GetDataProperties(this Type type)
            => type.GetProperties(PROPS)
                    .Select(p => new Property(p));
    }
}
