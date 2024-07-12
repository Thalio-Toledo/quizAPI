using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QuizAPI.Helpers;

/// <summary>
/// A result type.
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct Result<T>
{
    /// <summary>
    /// Gets the data (assuming the result is Ok)
    /// <example>
    /// Examples:
    /// <code>
    ///     Result&lt;int&gt; result = await GetValueAsync();
    ///     if(result.IsOk)
    ///         Console.WriteLine(result.Data); // Here we can access the data safely
    ///     else
    ///         Console.WriteLine(result.Data); // Here data will be probably null
    /// </code>
    /// </example>
    /// </summary>
    public T? Data { get; init; }
    /// <summary>
    /// Gets the error (assuming the result is an error)
    /// <example>
    /// Examples:
    /// <code>
    ///     Result&lt;int&gt; result = await GetValueAsync();
    ///     if(result.IsError)
    ///         Console.WriteLine($"Error is: {result.Error}"); // We know that the result is an error, so we can access the error safely
    ///     else
    ///         Console.WriteLine($"Error is: {result.Error}"); // Here error will be probably null
    /// </code>
    /// </example>
    /// </summary>
    public Error? Error { get; init; }

    /// <summary>
    /// Gets whether the result is Ok
    /// </summary>
    [MemberNotNullWhen(false, nameof(Error)), JsonIgnore]
    [MemberNotNullWhen(true, nameof(Data))]
    public bool IsOk => Error is null;
    /// <summary>
    /// Gets whether the result is an error
    /// </summary>
    [MemberNotNullWhen(true, nameof(Error)), JsonIgnore]
    [MemberNotNullWhen(false, nameof(Data))]
    public bool IsErr => Error is not null;

    /// <summary>
    /// Gets the status code of the result. Can be changed with <see cref="WithStatusCode(HttpStatusCode)"/>
    /// </summary>
    public HttpStatusCode StatusCode { get; init; }

    public Result(T data)
    {
        Data = data;
        Error = null;
        StatusCode = HttpStatusCode.OK;
    }

    public Result(Error error)
    {
        Data = default!;
        Error = error;
        StatusCode = HttpStatusCode.BadRequest;
    }

    /// <summary>
    /// If the result is ok, <paramref name="func"/> is called to transform the data into a new value. If the result is an error, the error is returned. If the result is an error, nothing happens.
    /// 
    /// <br/>
    /// <example>
    /// Example when everything is ok:
    /// <code>
    ///     Result&lt;int&gt; result = await GetValueAsync();
    ///     var resultTimes3 = result.MapOk(x => x * 3); // Here we multiply the data by 3 IF the result is Ok
    /// </code>
    /// Example when we have an error:
    /// <code>
    ///     Result&lt;int&gt; result = await GetValueAsyncWithError();
    ///     var resultTimes3 = result.MapOk(x => x * 3); // Here we do nothing, because the result is an error, but we don't need to know that to use this method, this is why it is useful.
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="R"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    public Result<R> MapOk<R>(Func<T, R> func) => IsOk switch
    {
        true => Result.RunSafe(Data, func, Error),
        false => Error
    };

    public ValueTask<Result<R>> MapOkAsync<R>(Func<T, ValueTask<R>> func)
    {
        if (IsOk)
        {
            var task = func(Data);
            if (task.IsCompleted)
                return ValueTask.FromResult(Result.Ok(task.Result));

            async ValueTask<Result<R>> Produce()
            {
                try
                {
                    return Result.Ok(await task);
                }
                catch (Exception ex)
                {
                    return Result.Error(ex, true);
                }
            }
            return Produce();
        }
        return ValueTask.FromResult(new Result<R>(Error));
    }

    /// <summary>
    /// If the result is an error, <paramref name="func"/> is called to transform the error into a new error. If the result is Ok, nothing happens.
    /// 
    /// <br/>
    /// <example>
    /// Examples:
    /// <code>
    ///     Result&lt;int&gt; result = await GetValueAsyncWithError();
    ///     var resultWithNewError = result.MapError(error => new Error("New error message")); // Here we create a new error with the message "New error message"
    /// </code>
    /// We can also use the previous error to create the new one:
    /// <code>
    ///     Result&lt;int&gt; resultWithNewError = result.MapError(error => new Error($"New error message: {error.Message}"));
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    public Result<T> MapError(Func<Error, Error> func)
    {
        if (!IsErr)
            return this;

        try
        {
            return func(Error);
        }
        catch (Exception ex)
        {
            return new AggregateError([Error, new Error(ex.Message, ex)]);
        }
    }

    /// <summary>
    /// If the result is ok, the data is returned. If the result is an error, <paramref name="other"/> is returned.
    /// 
    /// <br/>
    /// <example>
    /// Examples:
    /// <code>
    ///     Result&lt;int&gt; result = await GetValueAsync();
    ///     Console.WriteLine(result.OrElse(0)); // Here we will print the data if the result is Ok, otherwise 0
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public T OrElse(T other) => IsOk switch
    {
        true => Data,
        false => other
    };

    /// <summary>
    /// If the result is ok, the data is returned. If the result is an error, <paramref name="other"/> is called and the value is returned.
    /// 
    /// <br/>
    /// <example>
    /// Examples:
    /// <code>
    ///     Result&lt;int&gt; result = await GetValueAsync();
    ///     Console.WriteLine(result.OrElse(() => 0)); // Here we will print the data if the result is Ok, otherwise 0
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public T OrElse(Func<T> other) => IsOk switch
    {
        true => Data,
        false => other()
    };

    /// <summary>
    /// If the result is ok, the data is returned. If the result is an error, <paramref name="other"/> is returned.
    /// 
    /// <br/>
    /// <example>
    /// Examples:
    /// <code>
    ///     Result&lt;int&gt; result = await GetValueAsync();
    ///     Console.WriteLine(result.OrElse(Result.Error("Error"))); // Here we will print the data if the result is Ok, otherwise an error with the message "Error"
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public Result<T> OrElse(Result<T> other) => IsOk switch
    {
        true => this,
        false => other
    };

    /// <summary>
    /// Will call <paramref name="action"/> if the result is Ok.<br/>
    /// Contrary to <see cref="MapOk{R}(Func{T, R})"/> the returned result is not affected by the action. It also does not protect you from exceptions.
    /// 
    /// <br/>
    /// <example>
    /// Examples:
    /// <code>
    ///     Result&lt;int&gt; result = await GetValueAsync();
    ///     result.WhenOk(data => Console.WriteLine(data)); // Here we will print the data if the result is Ok
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result<T> WhenOk(Action<T> action)
    {
        if (IsOk) action(Data);
        return this;
    }

    /// <summary>
    /// Will call <paramref name="action"/> if the result is an error.<br/>
    /// Contrary to <see cref="MapError(Func{Error, Error})"/> the returned result is not affected by the action. It also does not protect you from exceptions.
    /// 
    /// <br/>
    /// <example>
    /// Examples:
    /// <code>
    ///     Result&lt;int&gt; result = await GetValueAsyncWithError();
    ///     result.WhenError(error => Console.WriteLine(error)); // Here we will print the error if the result is an error
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public Result<T> WhenError(Action<Error> action)
    {
        if (IsErr) action(Error!);
        return this;
    }

    /// <summary>
    /// Expects that the result is ok and returns it. If it is not, an <see cref="InvalidOperationException"/> is thrown with the message <paramref name="message"/>
    /// 
    /// <br/>
    /// <example>
    /// Examples:
    /// <code>
    ///     Result&lt;int&gt; result = await GetValueAsync();
    ///     Console.WriteLine(result.Expects()); // Here we will print the data if the result is Ok, otherwise throw an exception
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public T Expects(string message = "Result was not Ok") => IsOk switch
    {
        true => Data,
        false => throw new InvalidOperationException(message)
    };

    /// <summary>
    /// Tries to get the data if the result is Ok.
    /// 
    /// <br/>
    /// <example>
    /// Examples;
    /// <code>
    ///     Result&lt;int&gt; result = await GetValueAsync();
    ///     if(result.TryGetOk(out var data))
    ///         Console.WriteLine(data); // Here we can access the data safely
    ///     else
    ///         Console.WriteLine(data); // Here data will be probably null
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="ok"></param>
    /// <returns>
    /// <see langword="true"/> if the result is Ok, <see langword="false"/> otherwise.
    /// </returns>
    public bool TryGetOk([NotNullWhen(true)] out T? ok)
    {
        ok = IsOk ? Data : default;
        return IsOk;
    }

    /// <summary>
    /// Tries to get the error if the result is an error.
    /// 
    /// <br/>
    /// <example>
    /// Examples:
    /// <code>
    ///     Result&lt;int&gt; result = await GetValueAsyncWithError();
    ///     if(result.TryGetError(out var error))
    ///         Console.WriteLine(error); // Here we can access the error safely
    ///     else
    ///         Console.WriteLine(error); // Here error will be probably null
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="error"></param>
    /// <returns>
    /// <see langword="true"/> if the result is an error, <see langword="false"/> otherwise.
    /// </returns>
    public bool TryGetError([NotNullWhen(true)] out Error? error)
    {
        error = IsErr ? Error : default;
        return IsErr;
    }

    /// <summary>
    /// Will produce a new result with the <see cref="StatusCode"/> as <paramref name="statusCode"/>
    /// 
    /// <br/>
    /// <example>
    /// Examples:
    /// <code>
    ///     Result&lt;int&gt; result = await GetValueAsync();
    ///     return result.WithStatusCode(HttpStatusCode.NotFound); // Here we can change the status code to 404
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public Result<T> WithStatusCode(HttpStatusCode statusCode)
        => this with { StatusCode = statusCode };

    public static implicit operator Result<T>(Error error) => new(error);
    public static implicit operator Result<T>(Result.ResultError error) => new Result<T>(error.Error).WithStatusCode(error.StatusCode);
}

public static class Result
{
    /// <summary>
    /// Allows you to run safely an arbitrary function <paramref name="run"/> and catch any exception that might be thrown. If an exception is thrown, the result will be an error with the exception as the error message.
    /// <br/><br/>
    /// <example>
    /// Example 0:
    /// <code>
    ///     Result&lt;int&gt; result = Result.RunSafe(() => 5 / 0); // Here we try to divide by 0, which will throw an exception, so the result will be an error
    /// </code>
    /// The result of above will be an error with the message "Attempted to divide by zero."
    /// <br/><br/>
    /// Example 1:
    /// <code>
    ///     Result&lt;int&gt; result = Result.RunSafe(() => 5 / 1); // Here we divide by 1, which is safe, so the result will be Ok
    /// </code>
    /// The result of above will be Ok with the value 5.
    /// </example>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="run"></param>
    /// <param name="baseError"></param>
    /// <returns></returns>
    public static Result<T> RunSafe<T>(Func<T> run, Error? baseError = null)
    {
        try
        {
            return Ok(run());
        }
        catch (Exception ex)
        {
            if (baseError is null)
                return Error(ex, true);
            else
                return new AggregateError([baseError, new Error(ex.Message, ex)]);
        }
    }
    /// <summary>
    /// Allows you to run safely an arbitrary function <paramref name="run"/> and catch any exception that might be thrown. If an exception is thrown, the result will be an error with the exception as the error message.<br/>
    /// This variant allows you to pass an argument <paramref name="arg"/> to the function <paramref name="run"/>.
    /// <br/><br/>
    /// <example>
    /// Example 0:
    /// <code>
    ///     Result&lt;int&gt; result = Result.RunSafe(5, n => n / 0); // Here we try to divide by 0, which will throw an exception, so the result will be an error
    /// </code>
    /// The result of above will be an error with the message "Attempted to divide by zero."
    /// <br/><br/>
    /// Example 1:
    /// <code>
    ///     Result&lt;int&gt; result = Result.RunSafe(5, n => n / 1); // Here we divide by 1, which is safe, so the result will be Ok
    /// </code>
    /// The result of above will be Ok with the value 5.
    /// </example>
    /// </summary>
    /// <typeparam name="P"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="arg"></param>
    /// <param name="run"></param>
    /// <param name="baseError"></param>
    /// <returns></returns>
    public static Result<T> RunSafe<P, T>(P arg, Func<P, T> run, Error? baseError = null)
    {
        try
        {
            return Ok(run(arg));
        }
        catch (Exception ex)
        {
            if (baseError is null)
                return Error(ex, true);
            else
                return new AggregateError([baseError, new Error(ex.Message, ex)]);
        }
    }

    /// <summary>
    /// Creates a new Ok result with the data <paramref name="ok"/>
    /// 
    /// <br/>
    /// <example>
    /// Examples:
    /// <code>
    ///     // Here we create a new Ok result with the value 5
    ///     Result&lt;int&gt; result = Result.Ok(5);
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ok"></param>
    /// <returns></returns>
    public static Result<T> Ok<T>(T ok) => new(ok);
    /// <summary>
    /// Creates a new Error result with the error <paramref name="error"/>
    /// 
    /// <br/>
    /// <example>
    /// Examples:
    /// <code>
    ///     // Here we create a new Error result with the error "Error message"
    ///     Result&lt;int&gt; result = Result.Error(new Error("Error message"));
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result<T> Error<T>(Error error) => new(error);
    /// <summary>
    /// Creates a new Error result with the error message as <paramref name="message"/>
    /// 
    /// <br/>
    /// <example>
    /// Examples:
    /// <code>
    ///     // Here we create a new Error result with the error "Error message"
    ///     Result&lt;int&gt; result = Result.Error("Error message");
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static ResultError Error(string message) => new(new Error(message));
    /// <summary>
    /// Creates a new Error result with the error message as <see cref="Exception.Message"/> from the exception <paramref name="exception"/>.<br/>Optionally, includes the exception itself as an <see cref="Error.Extra"/> when <paramref name="includeException"/>
    /// 
    /// <br/>
    /// <example>
    /// Examples:
    /// <code>
    ///     // Here we create a new Error result with the error "Error message"
    ///     Result&lt;int&gt; result = Result.Error(new Exception("Error message"));
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="exception">
    /// The exception to create the error from
    /// </param>
    /// <param name="includeException">
    /// Whether to include the exception itself in the error as an <see cref="Error.Extra"/>
    /// </param>
    /// <returns></returns>
    public static ResultError Error(Exception exception, bool includeException = false)
        => includeException switch
        {
            true => new ResultError(new Error(exception.Message, exception)),
            false => new ResultError(new Error(exception.Message))
        };

    /// <summary>
    /// A type that is used to not need to specify the type of the ok value when creating a new error result of type <see cref="Result{T}"/>
    /// </summary>
    /// <param name="Error"></param>
    /// <param name="StatusCode"></param>
    public readonly record struct ResultError(Error Error, HttpStatusCode StatusCode = HttpStatusCode.InternalServerError)
    {
        /// <summary>
        /// Will produce a new result error with the <see cref="StatusCode"/> as <paramref name="statusCode"/>
        /// <br/><br/><br/>
        /// <b>Similar to <see cref="Result{T}.WithStatusCode(HttpStatusCode)"/>:</b><br/>
        /// <inheritdoc cref="Result{T}.WithStatusCode(HttpStatusCode)"/>
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public ResultError WithStatusCode(HttpStatusCode statusCode)
            => this with { StatusCode = statusCode };
    }
}

public static class ResultExtensions
{
    /// <summary>
    /// Flattens a result of a result into a single result.
    /// <example>
    /// Examples:
    /// <code>
    ///     Result&lt;Result&lt;int&gt;&gt; result = await GetResultAsync();
    ///     var superResult = Result.Ok(result); // Here we have a result of a result (Result&lt;Result&lt;int&gt;&gt;)
    ///     var flattenedResult = superResult.Flatten(); // Here we flatten the result into a single result (Result&lt;int&gt;)
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <returns></returns>
    public static Result<T> Flatten<T>(this Result<Result<T>> self)
    {
        if (self.IsErr)
            return self.Error;
        var inner = self.Data;
        return inner;
    }
    /// <summary>
    /// Flattens a result of a result into a single result, then maps the data with <paramref name="func"/>.
    /// <example>
    /// Examples:
    /// <code>
    ///     Result&lt;Result&lt;int&gt;&gt; result = await GetResultAsync();
    ///     var superResult = Result.Ok(result); // Here we have a result of a result (Result&lt;Result&lt;int&gt;&gt;)
    ///     var flattenedResult = superResult.FlatMapOk(x => x * 3); // Here we flatten the result into a single result (Result&lt;int&gt;) and multiply the data by 3
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="R"></typeparam>
    /// <param name="self"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static Result<R> FlatMapOk<T, R>(this Result<Result<T>> self, Func<T, R> func)
        => self.MapOk(x => x.MapOk(func)).Flatten();

    public static Result<(A left, B right)> Zip<A, B>(this (Result<A> a, Result<B> b) options) => options switch
    {
        { a.IsErr: true, b.IsErr: true } => new AggregateError([options.a.Error!, options.b.Error!]),
        { a.IsErr: true } => options.a.Error,
        { b.IsErr: true } => options.b.Error,
        _ => Result.Ok((options.a.Data, options.b.Data))
    };
}

public static class ResultIterExtensions
{
    /// <summary>
    /// Flattens the data of each result in the enumerable.<br/><br/>
    /// <b>Similar to <see cref="ResultExtensions.Flatten{T}(Result{Result{T}})"/>:</b><br/>
    /// <inheritdoc cref="ResultExtensions.Flatten{T}(Result{Result{T}})"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <returns></returns>
    public static IEnumerable<Result<T>> Flatten<T>(this IEnumerable<Result<Result<T>>> self)
    {
        foreach (var item in self)
            yield return item.Flatten();
    }

    /// <summary>
    /// Maps the data of each result in the enumerable with <paramref name="func"/>.
    /// <example>
    /// Examples:
    /// <code>
    ///     // Assumes [Ok(3), Error, Ok(5)]
    ///     Result&lt;int&gt;[] results = await GetManyResultsAsync();
    ///     var mappedResults = results.MapOkEach(x => x * 3); // Here we multiply each item data by 3 IF each result is Ok
    /// </code>
    /// The result will be [Ok(9), Error, Ok(15)].
    /// </example>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="R"></typeparam>
    /// <param name="self"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static IEnumerable<Result<R>> MapOkEach<T, R>(this IEnumerable<Result<T>> self, Func<T, R> func)
    {
        foreach (var item in self)
            yield return item.MapOk(func);
    }
    /// <summary>
    /// Try to get the data of each result in the enumerable. If any result is an error, the method will return the collection of errors in the enumerable. If all results are ok, the method will return the collection of data in the enumerable.
    /// <example><br/>
    /// <b>When any item is an error:</b>
    /// <code>
    ///     // Assumes [Ok(3), Error, Ok(5)]
    ///     Result&lt;int&gt;[] results = await GetManyResultsAsync();
    ///     var allOk = results.AllOk();
    /// </code>
    /// The result will be Error.<br/><br/>
    /// 
    /// <b>When all items are ok:</b>
    /// <code>
    ///     // Assumes [Ok(3), Ok(5), Ok(7)]
    ///     Result&lt;int&gt;[] results = await GetManyResultsAsync();
    ///     var allOk = results.AllOk();
    /// </code>
    /// The result will be Ok([3, 5, 7]).
    /// </example>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <returns></returns>
    public static Result<IEnumerable<T>> AllOk<T>(this IEnumerable<Result<T>> self)
    {
        List<Error>? errors = null;
        List<T>? ok = null;
        foreach (var item in self)
        {
            if (item.IsErr)
            {
                errors ??= [];
                errors.Add(item.Error!);
            }
            if (item.IsOk)
            {
                ok ??= [];
                ok.Add(item.Data);
            }
        }

        if (errors is not null)
            return new AggregateError(errors);

        if (ok is null or { Count: 0 })
            return Result.Error("Empty");

        return Result.Ok(ok.AsEnumerable());
    }
    /// <summary>
    /// Try to get the data of each result in the enumerable. If any result is ok, the method will return the collection of data in the enumerable. If all results are errors, the method will return the collection of errors in the enumerable.
    /// <example><br/>
    /// <b>When any item is an error:</b>
    /// <code>
    ///     // Assumes [Ok(3), Error, Ok(5)]
    ///     Result&lt;int&gt;[] results = await GetManyResultsAsync();
    ///     var allOk = results.AllOk();
    /// </code>
    /// The result will be Ok([3, 5]).<br/><br/>
    /// 
    /// <b>When all items are ok:</b>
    /// <code>
    ///     // Assumes [Error, Error, Error]
    ///     Result&lt;int&gt;[] results = await GetManyResultsAsync();
    ///     var allOk = results.AllOk();
    /// </code>
    /// The result will be Error.
    /// </example>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <returns></returns>
    public static Result<IEnumerable<T>> AnyOk<T>(this IEnumerable<Result<T>> self)
    {
        List<Error>? errors = null;
        List<T>? ok = null;
        foreach (var item in self)
        {
            if (item.IsErr)
            {
                errors ??= [];
                errors.Add(item.Error!);
            }
            if (item.IsOk)
            {
                ok ??= [];
                ok.Add(item.Data);
            }
        }

        if (ok is not null)
            return Result.Ok(ok.AsEnumerable());

        if (errors is null or { Count: 0 })
            return Result.Error("Empty");

        return new AggregateError(errors);
    }
}

public static class ResultAsyncExtensions
{
    /// <inheritdoc cref="Result{T}.MapOk{R}(Func{T, R})" />
    public static async Task<Result<R>> MapOk<T, R>(this Task<Result<T>> resultTask, Func<T, R> func)
    {
        var result = await resultTask;
        return result.MapOk(func);
    }
    /// <inheritdoc cref="MapOk{T, R}(Task{Result{T}}, Func{T, R})"/>
    public static async ValueTask<Result<R>> MapOk<T, R>(this ValueTask<Result<T>> resultTask, Func<T, R> func)
    {
        var result = await resultTask;
        return result.MapOk(func);
    }

    /// <inheritdoc cref="Result{T}.MapError(Func{Error, Error})" />
    public static async Task<Result<T>> MapError<T>(this Task<Result<T>> resultTask, Func<Error, Error> func)
    {
        var result = await resultTask;
        return result.MapError(func);
    }
    /// <inheritdoc cref="MapError{T}(Task{Result{T}}, Func{Error, Error})"/>
    public static async ValueTask<Result<T>> MapError<T>(this ValueTask<Result<T>> resultTask, Func<Error, Error> func)
    {
        var result = await resultTask;
        return result.MapError(func);
    }

    /// <inheritdoc cref="Result{T}.OrElse(T)" />
    public static async Task<T> OrElse<T>(this Task<Result<T>> resultTask, T other)
    {
        var result = await resultTask;
        return result.OrElse(other);
    }
    /// <inheritdoc cref="OrElse{T}(Task{Result{T}}, T)" />
    public static async ValueTask<T> OrElse<T>(this ValueTask<Result<T>> resultTask, T other)
    {
        var result = await resultTask;
        return result.OrElse(other);
    }

    /// <inheritdoc cref="Result{T}.OrElse(Func{T})" />
    public static async Task<T> OrElse<T>(this Task<Result<T>> resultTask, Func<T> other)
    {
        var result = await resultTask;
        return result.OrElse(other);
    }
    /// <inheritdoc cref="OrElse{T}(Task{Result{T}}, Func{T})" />
    public static async ValueTask<T> OrElse<T>(this ValueTask<Result<T>> resultTask, Func<T> other)
    {
        var result = await resultTask;
        return result.OrElse(other);
    }

    /// <inheritdoc cref="Result{T}.OrElse(Result{T})" />
    public static async Task<Result<T>> OrElse<T>(this Task<Result<T>> resultTask, Result<T> other)
    {
        var result = await resultTask;
        return result.OrElse(other);
    }
    /// <inheritdoc cref="OrElse{T}(Task{Result{T}}, Result{T})" />
    public static async ValueTask<Result<T>> OrElse<T>(this ValueTask<Result<T>> resultTask, Result<T> other)
    {
        var result = await resultTask;
        return result.OrElse(other);
    }

    /// <inheritdoc cref="Result{T}.Expects(string)"/>" />
    public static async Task<T> Expects<T>(this Task<Result<T>> resultTask, string message = "Result was not Ok")
    {
        var result = await resultTask;
        return result.Expects(message);
    }
    /// <inheritdoc cref="Expects{T}(Task{Result{T}}, string)"/>" />
    public static async ValueTask<T> Expects<T>(this ValueTask<Result<T>> resultTask, string message = "Result was not Ok")
    {
        var result = await resultTask;
        return result.Expects(message);
    }

    /// <inheritdoc cref="ResultExtensions.Flatten{T}(Result{Result{T}})"/>
    public static async Task<Result<T>> Flatten<T>(this Task<Result<Result<T>>> resultTask)
        => (await resultTask).Flatten();
    /// <inheritdoc cref="Flatten{T}(Task{Result{Result{T}}})"/>
    public static async ValueTask<Result<T>> Flatten<T>(this ValueTask<Result<Result<T>>> resultTask)
        => (await resultTask).Flatten();

    /// <inheritdoc cref="ResultExtensions.FlatMapOk{T, R}(Result{Result{T}}, Func{T, R})"/>
    public static async Task<Result<R>> FlatMapOk<T, R>(this Task<Result<Result<T>>> resultTask, Func<T, R> func)
        => (await resultTask).FlatMapOk(func);
    /// <inheritdoc cref="FlatMapOk{T, R}(Task{Result{Result{T}}}, Func{T, R})"/>
    public static async ValueTask<Result<R>> FlatMapOk<T, R>(this ValueTask<Result<Result<T>>> resultTask, Func<T, R> func)
        => (await resultTask).FlatMapOk(func);
}

public static class ResultAsyncIterExtensions
{
    /// <inheritdoc cref="ResultIterExtensions.Flatten{T}(IEnumerable{Result{Result{T}}})"/>
    public static async Task<IEnumerable<Result<T>>> Flatten<T>(this Task<IEnumerable<Result<Result<T>>>> resultTask)
        => (await resultTask).Flatten();
    /// <inheritdoc cref="Flatten{T}(Task{IEnumerable{Result{Result{T}}}})"/>
    public static async ValueTask<IEnumerable<Result<T>>> Flatten<T>(this ValueTask<IEnumerable<Result<Result<T>>>> resultTask)
        => (await resultTask).Flatten();

    /// <inheritdoc cref="ResultIterExtensions.MapOkEach{T, R}(IEnumerable{Result{T}}, Func{T, R})"/>
    public static async Task<IEnumerable<Result<R>>> MapOkEach<T, R>(this Task<IEnumerable<Result<T>>> resultTask, Func<T, R> func)
        => (await resultTask).MapOkEach(func);
    /// <inheritdoc cref="MapOkEach{T, R}(Task{IEnumerable{Result{T}}}, Func{T, R})"/>
    public static async ValueTask<IEnumerable<Result<R>>> MapOkEach<T, R>(this ValueTask<IEnumerable<Result<T>>> resultTask, Func<T, R> func)
        => (await resultTask).MapOkEach(func);

    /// <inheritdoc cref="ResultIterExtensions.AllOk{T}(IEnumerable{Result{T}})"/>
    public static async Task<Result<IEnumerable<T>>> AllOk<T>(this Task<IEnumerable<Result<T>>> resultTask)
        => await resultTask.AllOk();
    /// <inheritdoc cref="AllOk{T}(Task{IEnumerable{Result{T}}})"/>
    public static async ValueTask<Result<IEnumerable<T>>> AllOk<T>(this ValueTask<IEnumerable<Result<T>>> resultTask)
        => await resultTask.AllOk();

    /// <inheritdoc cref="ResultIterExtensions.AnyOk{T}(IEnumerable{Result{T}})"/>
    public static async Task<Result<IEnumerable<T>>> AnyOk<T>(this Task<IEnumerable<Result<T>>> resultTask)
        => await resultTask.AnyOk();
    /// <inheritdoc cref="AnyOk{T}(Task{IEnumerable{Result{T}}})"/>
    public static async ValueTask<Result<IEnumerable<T>>> AnyOk<T>(this ValueTask<IEnumerable<Result<T>>> resultTask)
        => await resultTask.AnyOk();
}
