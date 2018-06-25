﻿using Ooorm.Data.Attributes;
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

        public static IEnumerable<ColumnInfo<T>> GetColumns<T>(this T value)
            => typeof(T).GetProperties(PROPS)
                    .Where(p => p.HasAttribute<ColumnAttribute>())
                    .Select(p => new ColumnInfo<T>(p));

        public static IEnumerable<ColumnInfo> GetColumns(this Type type)
            => type.GetProperties(PROPS)
                    .Where(p => p.HasAttribute<ColumnAttribute>())
                    .Select(p => new ColumnInfo(p));
    }
}
