using Ooorm.Data.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ooorm.Data.Reflection
{
    public static class TypeExtensions
    {
        const BindingFlags PROPS = BindingFlags.Public | BindingFlags.Instance;

        public static bool HasAttribute<TAttribute>(this Type type) where TAttribute : Attribute
        {
            var attr = type.GetCustomAttribute<TAttribute>();
            return attr != null;
        }

        public static bool TryGetAttribute<TAttribute>(this Type type, out TAttribute attribute) where TAttribute : Attribute
        {
            var attr = type.GetCustomAttribute<TAttribute>();
            if (attr != null)
                attribute = attr;
            else
                attribute = default;
            return attr != null;
        }

        public static bool HasAttribute<TAttribute>(this MemberInfo member) where TAttribute : Attribute
        {
            var attr = member.GetCustomAttribute<TAttribute>();
            return attr != null;
        }

        public static bool TryGetAttribute<TAttribute>(this MemberInfo member, out TAttribute attribute) where TAttribute : Attribute
        {
            var attr = member.GetCustomAttribute<TAttribute>();
            if (attr != null)
                attribute = attr;
            else
                attribute = default;
            return attr != null;
        }

        public static IEnumerable<Column<T>> GetColumns<T>(this T value, bool exceptId = false)
            => typeof(T).GetProperties(PROPS)
                    .Where(p => p.HasAttribute<ColumnAttribute>())
                    .Where(p => !(exceptId && p.HasAttribute<IdAttribute>()))
                    .Select(p => new Column<T>(p));

        public static IEnumerable<Column> GetColumns(this Type type, bool exceptId = false)
        {
            var props = type.GetProperties(PROPS).ToArray();
            var fields = props.Where(p => p.HasAttribute<ColumnAttribute>()).ToArray();
            var notId = fields.Where(p => !(exceptId && p.HasAttribute<IdAttribute>())).ToArray();
            var columns = notId.Select(p => new Column(p)).ToArray();
            return columns;
        }

        public static IEnumerable<Property<T>> GetDataProperties<T>(this T value)
            => typeof(T).GetProperties(PROPS)
                    .Select(p => new Property<T>(p));

        public static IEnumerable<Property> GetDataProperties(this Type type)
            => type.GetProperties(PROPS)
                    .Select(p => new Property(p));
    }
}
