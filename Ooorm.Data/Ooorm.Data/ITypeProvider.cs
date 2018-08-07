using Ooorm.Data.Reflection;
using System;
using System.Data;
using System.Security.Cryptography;

namespace Ooorm.Data
{
    public interface ITypeResolver
    {
        DbType DbType<TClrType>();
        DbType DbType(Type clrType);
        Type ClrType(DbType dbType);
        string DbTypeString(Column column);
        object ToDbValue(object value);
        object FromDbValue(object value, Type type);
        bool IsDbValueType(Type clrType);
    }

    public struct Checksum : IEquatable<Checksum>
    {
        public readonly static MD5 Alg = MD5.Create();

        public readonly bool IsRoot;
        private readonly ulong a;
        private readonly ulong b;

        public Checksum(byte[] data)
        {
            IsRoot = true;
            ulong value = 0;
            for (int i = 0; i < 8; i++)
                value |= (ulong)data[i] << (8 * i);
            a = value;
            value = 0;
            for (int i = 8; i < 15; i++)
                value |= (ulong)data[i] << (8 * (i - 8));
            b = value;
        }

        public Checksum(Checksum left, Checksum right)
        {
            IsRoot = false;
            a = (left.a & (~0ul >> 32)) | (right.a & (~0ul << 32));
            b = left.a ^ left.b ^ right.a ^ right.b;
        }

        public bool Equals(Checksum other) => a == other.a && b == other.b;

        public (bool left, bool right) CompareNode(Checksum other) =>
            Equals(other) ? (true, true) : ((a & (~0ul >> 32)) == (other.a & (~0ul >> 32)), (b & (~0ul << 32)) == (other.b & (~0ul << 32)));
    }

    public interface ITypeProvider
    {
        Type ClrType { get; }
        DbType DbType { get; }
        string DbTypeString { get; }

        object ToDbvalue(object value);
        object FromDbValue(object value);

        bool IsDbValueType();

        Checksum GetChecksum(object value, ITypeResolver resolver);
    }
}
