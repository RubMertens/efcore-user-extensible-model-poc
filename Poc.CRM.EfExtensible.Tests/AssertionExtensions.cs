using FluentAssertions;
using FluentAssertions.Primitives;
using Poc.CRM.EfExtensible.Web.Application;

namespace Poc.CRM.EfExtensible.Tests;

public static class ResultExtensions
{
    public static ResultAssertions<T> Should<T>(this Result<T> result) => new(result);
}

public class ResultAssertions<T>(Result<T> result) : ReferenceTypeAssertions<Result<T>, ResultAssertions<T>>(result)
{
    protected override string Identifier { get; } = "Result";


    public AndConstraint<ResultAssertions<T>> SucceedWith(T expected)
    {
        result.Success.Should().BeTrue();
        if (expected is not null)
        {
            result.Data.Should().BeEquivalentTo(expected);
        }

        return new AndConstraint<ResultAssertions<T>>(this);
    }
    public AndConstraint<ResultAssertions<T>> Succeed()
    {
        result.Success.Should().BeTrue();
        return new AndConstraint<ResultAssertions<T>>(this);
    }

    public AndConstraint<ResultAssertions<T>> FailWith(DomainError error) 
    {
        result.Success.Should().BeFalse();
        result.Data.Should().Be(default(T));
        result.Error.Should().BeEquivalentTo(error);
        return new AndConstraint<ResultAssertions<T>>(this);
    }
}