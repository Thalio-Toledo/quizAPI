using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizAPI.Helpers;

/// <summary>
/// Error class to represent an error message
/// </summary>
/// <param name="Message">
/// The error message
/// </param>
/// <param name="Extra">
/// An optional extra object to include with the error, such as an exception or other data
/// </param>
public record Error(string Message, object? Extra = null);

/// <summary>
/// Compound error class to represent two errors stacked together.
/// </summary>
/// <param name="Main"></param>
/// <param name="Other"></param>
public record CompoundError(Error Main, Error Other) : Error("Compound Error", new { Main, Other })
{
    /// <summary>
    /// Composes the current error with another error.
    /// </summary>
    /// <param name="with"></param>
    /// <returns></returns>
    public CompoundError Compose(Error with) => new(this, with);

    /// <summary>
    /// Flattens the error hierarchy to a single aggregate error.
    /// </summary>
    /// <returns></returns>
    public AggregateError Aggregate()
    {
        var all = new List<Error>(32);

        void AggrFrom(Error error)
        {
            if (error is CompoundError compound)
            {
                AggrFrom(compound.Main);
                AggrFrom(compound.Other);
            }
            else if (error is AggregateError aggregate)
            {
                foreach (var inner in aggregate.Errors)
                    AggrFrom(inner);
            }
            else
                all.Add(error);
        }

        AggrFrom(this);

        return new AggregateError(all);
    }
}

/// <summary>
/// Aggregate error class to represent multiple errors stacked together
/// </summary>
/// <param name="Errors"></param>
public record AggregateError(IReadOnlyList<Error> Errors) : Error(
    Message: string.Join('\n', Errors.Select(err => err.Message)),
    Extra: Errors.Select(err => err.Extra).ToArray()
)
{
    /// <summary>
    /// Flattens the error hierarchy to a single aggregate error.
    /// </summary>
    /// <returns></returns>
    public AggregateError Flatten()
    {
        var all = new List<Error>(32);

        void AggrFrom(Error error)
        {
            if (error is CompoundError compound)
            {
                AggrFrom(compound.Main);
                AggrFrom(compound.Other);
            }
            else if (error is AggregateError aggregate)
            {
                foreach (var inner in aggregate.Errors)
                    AggrFrom(inner);
            }
            else
                all.Add(error);
        }

        foreach (var error in Errors)
            AggrFrom(error);

        return new AggregateError(all);
    }
}