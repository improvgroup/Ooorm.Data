using FluentAssertions.Equivalency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ooorm.Data.TestUtils
{
    public class PublicOnly : IMemberSelectionRule
    {
        private static readonly PublicOnly _instance = new PublicOnly();
        public static Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> Rule<T>() => options => options.Using(_instance);

        public bool IncludesMembers => false;

        public IEnumerable<SelectedMemberInfo> SelectMembers(IEnumerable<SelectedMemberInfo> selectedMembers, IMemberInfo context, IEquivalencyAssertionOptions config) =>
            selectedMembers.Where(m => m.DeclaringType.GetMember(m.Name, BindingFlags.Instance | BindingFlags.Public).Any());
    }
}
