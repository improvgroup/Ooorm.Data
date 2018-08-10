using Ooorm.Data.Reflection;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Ooorm.Data
{
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Default implementation of a predicate to sql transpiler
        /// </summary>
        internal static string ToSql<T>(this Expression<Func<T, bool>> predicate) => Where(predicate.Body);

        /// <summary>
        /// Default implementation of a predicate (with query parameter) to sql transpiler
        /// </summary>
        internal static string ToSql<T, TParam>(this Expression<Func<T, TParam, bool>> predicate, TParam param) => Where(predicate.Body, predicate.Parameters.Last().Name, param);

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
                    return "=";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.Convert:
                    return "";
                default:
                    throw new NotSupportedException($"Expression type {type} is not supported");
            }
        }

        internal static string Where(Expression exp, string paramName = null, object param = null)
        {
            var builder = new StringBuilder();
            BuildWhere(exp, builder, paramName, param);
            return builder.ToString();
        }

        internal static void BuildWhere(Expression exp, StringBuilder builder, string paramName, object param)
        {
            if (exp is ConstantExpression constant)
            {
                if (constant.Value is int intvalue)
                    builder.Append($"{intvalue}");
                else if (constant.Value is IdConvertable<int> dbval)
                    builder.Append($"{dbval.ToId()}");
                else if (constant.Value is IdConvertable<int?> dbref && dbref.ToId().HasValue)
                    builder.Append($"{dbref.ToId()}");
                else if (constant.Value is string text)
                    builder.Append($"'{text}'");
                else if (constant.Value is bool boolvalue)
                {
                    if (boolvalue)
                        builder.Append("1");
                    else
                        builder.Append("0");
                }
                else
                    throw new Exception($"Constants of type {constant.Value.GetType()} are not supported - use parameterization");
            }
            else if (exp is UnaryExpression unexp)
            {
                string operand = Operand(unexp.NodeType);
                if (string.IsNullOrEmpty(operand))
                    BuildWhere(unexp.Operand, builder, paramName, param);
                else
                {
                    builder.Append($"{operand} (");
                    BuildWhere(unexp.Operand, builder, paramName, param);
                    builder.Append(")");
                }
            }
            else if (exp is BinaryExpression binexp)
            {
                if (binexp.Right is ConstantExpression checknull && checknull.Value == null)
                {
                    if (binexp.NodeType == ExpressionType.Equal)
                    {

                        builder.Append("(");
                        BuildWhere(binexp.Left, builder, paramName, param);
                        builder.Append(" IS NULL)");
                    }
                    else if (binexp.NodeType == ExpressionType.NotEqual)
                    {
                        builder.Append("(");
                        BuildWhere(binexp.Left, builder, paramName, param);
                        builder.Append(" IS NOT NULL)");
                    }
                    else
                        throw new NotSupportedException("Null comparisons are only supported for == and !=");
                }
                else
                {
                    builder.Append($"(");
                    BuildWhere(binexp.Left, builder, paramName, param);
                    if (param != null && binexp.Right is MemberExpression poxaiuvgf && poxaiuvgf.Expression.ToString() == paramName)
                    {
                        var columns = param.GetType().GetColumns().Select(c => c.GetFrom(param)).ToArray();
                    }
                    if (param != null
                        && binexp.Right is MemberExpression paramexp)
                    {
                        if (paramexp.Expression.ToString() == paramName && param.GetType().GetColumns().Single(c => c.PropertyName == paramexp.Member.Name).GetFrom(param) == null)
                        {
                            builder.Append(" IS NULL)");
                        }
                        else
                        {
                            builder.Append($" {Operand(binexp.NodeType)} ");
                            BuildWhere(binexp.Right, builder, paramName, param);
                            builder.Append(")");
                        }
                    }
                    else
                    {
                        builder.Append($" {Operand(binexp.NodeType)} ");
                        BuildWhere(binexp.Right, builder, paramName, param);
                        builder.Append(")");
                    }
                }
            }
            else if (exp is TypeBinaryExpression typeexp)
            {
                if (typeexp.TypeOperand == typeof(DBNull))
                    throw new NotSupportedException($"Type operands are only supported for dbnull checking");
                BuildWhere(typeexp.Expression, builder, paramName, param);
                builder.Append(" IS NULL");
            }
            else if (exp is MemberExpression member)
            {
                if (((ParameterExpression)member.Expression).Name == paramName)
                    builder.Append($"@{member.Member.Name}");
                else
                    builder.Append($"[{member.Member.Name}]");
            }
            else if (exp is ParameterExpression parameter)
            {
                builder.Append($"@{parameter.Name}");
            }
        }
    }
}
