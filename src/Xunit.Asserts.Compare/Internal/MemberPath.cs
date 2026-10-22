using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Xunit.Asserts.Compare.Internal;

/// <summary>
/// Turns member-access lambdas such as <c>x =&gt; x.Address.City</c> into the members they touch.
/// </summary>
internal static class MemberPath
{
    /// <summary>
    /// Returns the members along the access path, outermost first
    /// (<c>x =&gt; x.Address.City</c> gives <c>[Address, City]</c>).
    /// </summary>
    public static IReadOnlyList<MemberInfo> GetMembers(LambdaExpression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var members = new List<MemberInfo>();
        var current = StripConversions(expression.Body);

        while (current is MemberExpression memberExpression)
        {
            members.Add(memberExpression.Member);
            current = StripConversions(memberExpression.Expression);
        }

        if (members.Count == 0 || current is not ParameterExpression)
        {
            throw new ArgumentException(
                $"Expression '{expression}' must be a member access on the lambda parameter, such as 'x => x.Name' or 'x => x.Address.City'.",
                nameof(expression));
        }

        members.Reverse();
        return members;
    }

    /// <summary>
    /// Returns the last member of the access path (<c>x =&gt; x.Address.City</c> gives <c>City</c>).
    /// </summary>
    public static MemberInfo GetLeafMember(LambdaExpression expression)
    {
        var members = GetMembers(expression);
        return members[members.Count - 1];
    }

    // Value-type members are boxed when the lambda returns object, which wraps them in a Convert node.
    private static Expression? StripConversions(Expression? expression)
    {
        while (expression is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked or ExpressionType.TypeAs } unary)
        {
            expression = unary.Operand;
        }

        return expression;
    }
}
