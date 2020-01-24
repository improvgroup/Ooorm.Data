using Ooorm.Data.Matching;
using Ooorm.Data.Reflection;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Ooorm.Data
{
    public static class ExpressionExtensions
    {
        internal static string ToSql<T>(this Expression<Func<T>> constructor) => Matcher(constructor);

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
                else if (constant.Value is IdConvertable dbval)
                    builder.Append($"{dbval.ToId()}");                
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

        internal static string Matcher<T>(Expression<Func<T>> foo)
        {
            StringBuilder builder = new StringBuilder("WHERE ");            

            void parseConstant(ConstantExpression constant)
            {
                if (constant.Value is int intvalue)
                    builder.Append($"{intvalue}");
                else if (constant.Value is long longvalue)
                    builder.Append($"{longvalue}");
                else if (constant.Value is IdConvertable dbval)
                    builder.Append($"{dbval.ToId()}");                
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

            void parseConvert(UnaryExpression unary)
            {
                if (unary.NodeType == ExpressionType.Convert && unary.Method.ToString().Contains("Explicit"))
                {
                    if (unary.Type.BaseType == typeof(ExpressionDecorator<,>))
                        builder.Append(" ").Append((string)unary.Type.GetMethod("Operand").Invoke(null, new object[0])).Append(" ");
                }
                if (unary.Operand is UnaryExpression inner)
                    parseConvert(inner);
                else if (unary.Operand is ConstantExpression constant)
                    parseConstant(constant);
            }

            void parse(Expression expression)
            {
                if (expression is MemberInitExpression assign)
                {        
                    if (assign.Bindings.Count == 0)
                    {
                        builder.Append("1 = 1");
                        return;
                    }

                    bool list = false;
                    foreach (var binding in assign.Bindings)
                    {
                        if (binding is MemberAssignment assignment)
                        {
                            if (list)
                                builder.Append(" AND ");
                            builder.Append($"{assignment.Member.Name}");
                            if (assignment.Expression is ConstantExpression constant)
                            {
                                builder.Append(" = ");
                                parseConstant(constant);
                            }
                            else if (assignment.Expression is UnaryExpression unary)
                                parseConvert(unary);
                            else if (assignment.Expression is MethodCallExpression call && call.Method.Name == "In"/*nameof(DbItem<,>.In)*/)
                            {
                                builder.Append(" = ");
                                if (call.Object is MemberExpression member)
                                {
                                    var value = GetMemberExpressionValue(member);
                                    var id = Expression.Constant(((IdConvertable)value).ToId());
                                    parseConstant(id);
                                }
                            }
                            else if (assignment.Expression is MemberExpression member)
                            {
                                builder.Append(" = ");
                                parseConstant(Expression.Constant(GetMemberExpressionValue(member)));
                            }
                            list = true;
                        }
                    }
                }
            }

            parse(foo.Body);
            
            return builder.ToString();
        }

        public static object GetMemberExpressionValue(MemberExpression member)
        {
            if (member.Expression is ConstantExpression leaf)
            {
                var item = leaf.Value;
                var entity = item.GetType();
                if (entity.GetProperty(member.Member.Name) is PropertyInfo property)
                    return property.GetValue(item);
                else
                    return entity.GetField(member.Member.Name).GetValue(item);
            }
            else if (member.Expression is MemberExpression next)
            {
                var item = GetMemberExpressionValue(next);
                var entity = item.GetType();
                if (entity.GetProperty(member.Member.Name) is PropertyInfo property)
                    return property.GetValue(item);
                else
                    return entity.GetField(member.Member.Name).GetValue(item);
            }
            else
                throw new NotImplementedException();            
        }
    }
}
