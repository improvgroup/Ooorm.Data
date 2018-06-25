using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Ooorm.Data.QueryProviders
{
    public class SqlServerQueryProvider<T> : IQueryProvider<T> where T : IDbItem
    {
        public string DeleteSql<TParam>(int id)
        {
            throw new NotImplementedException();
        }

        public string DeleteSql(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public string DeleteSql<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam parameters)
        {
            throw new NotImplementedException();
        }

        public string InsertSql<TParam>(TParam parameters)
        {
            throw new NotImplementedException();
        }

        public string ReadSql(int id)
        {
            throw new NotImplementedException();
        }

        public string ReadSql(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public string ReadSql<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam parameters)
        {
            throw new NotImplementedException();
        }

        public string UpdateSql<TParam>(int id, TParam parameters)
        {
            throw new NotImplementedException();
        }

        public string WhereClause<TParam>(Expression<Func<T, TParam, bool>> predicate)
        {
            return $"WHERE {Where(predicate.Body, predicate.Parameters.Last().Name)}";
        }

        public string WhereClause(Expression<Func<T, bool>> predicate)
        {
            return $"WHERE {Where(predicate.Body)}";
        }

        private static string Operand(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.Not:
                    return "NOT";
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Equal:
                    return "==";
                case ExpressionType.NotEqual:
                    return "!=";
                default:
                    throw new NotSupportedException($"Expression type {type} is not supported");
            }
        }

        private string Where(Expression exp, string paramName = null)
        {
            var builder = new StringBuilder();
            BuildWhere(exp, builder, paramName);
            return builder.ToString();
        }


        private void BuildWhere(Expression exp, StringBuilder builder, string paramName = null)
        {
            if (exp is ConstantExpression constant)
            {
                if (constant.Value is int intvalue)
                    builder.Append($"{intvalue}");
                else if (constant.Value is bool boolvalue)
                {
                    if (boolvalue)
                        builder.Append("1=1");
                    else
                        builder.Append("1=0");
                }
                else
                    throw new Exception($"Constants of type {constant.Value.GetType()} are not supported - use parameterization");
            }
            else if (exp is UnaryExpression unexp)
            {
                builder.Append($"{Operand(unexp.NodeType)} (");
                BuildWhere(unexp.Operand, builder, paramName);
                builder.Append(")");
            }
            else if (exp is BinaryExpression binexp)
            {
                if (binexp.Right is ConstantExpression checknull && checknull.Value == null)
                {
                    if (binexp.NodeType == ExpressionType.Equal)
                    {

                        builder.Append("(");
                        BuildWhere(binexp.Left, builder, paramName);
                        builder.Append(" IS NULL)");
                    }
                    else if (binexp.NodeType == ExpressionType.NotEqual)
                    {
                        builder.Append("(");
                        BuildWhere(binexp.Left, builder, paramName);
                        builder.Append(" IS NOT NULL)");
                    }
                    else
                        throw new NotSupportedException("Null comparisons are only supported for == and !=");
                }
                else
                {
                    builder.Append($"(");
                    BuildWhere(binexp.Left, builder, paramName);
                    builder.Append($" {Operand(binexp.NodeType)} ");
                    BuildWhere(binexp.Right, builder, paramName);
                    builder.Append(")");
                }
            }
            else if (exp is TypeBinaryExpression typeexp)
            {
                if (typeexp.TypeOperand == typeof(DBNull))
                    throw new NotSupportedException($"Type operands are only supported for dbnull checking");
                BuildWhere(typeexp.Expression, builder, paramName);
                builder.Append(" IS NULL");
            }
            else if (exp is MemberExpression member)
            {
                if (((ParameterExpression)member.Expression).Name == paramName)
                    builder.Append($"@{member.Member.Name}");
                else
                    builder.Append($"[{member.Member.Name}]");
            }
        }
    }
}
