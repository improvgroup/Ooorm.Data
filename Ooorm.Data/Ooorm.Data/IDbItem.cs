using Ooorm.Data.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ooorm.Data
{
    public interface IDbItem
    {
        [Id]
        [Column(nameof(ID))]
        int ID { get; set; }
    }

    public struct DbVal<T> where T : IDbItem
    {
        private readonly int value;

        private DbVal(int v) => value = v;

        public static implicit operator DbVal<T>(int v) => new DbVal<T>(v);

        public static implicit operator int(DbVal<T> v) => v.value;
    }

    public struct DbRef<T> where T : IDbItem
    {
        private readonly int? value;

        public bool IsNull => !value.HasValue;

        private DbRef(int? v) => value = v;

        public static implicit operator DbRef<T>(int? v) => new DbRef<T>(v);

        public static implicit operator int?(DbRef<T> v) => v.value;
    }
}
