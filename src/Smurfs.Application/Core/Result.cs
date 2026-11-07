using System.Diagnostics.CodeAnalysis;

namespace Smurfs.Application.Core;

/// <summary>
/// Represents an application outcome with a success value or an error message.
/// </summary>
/// <typeparam name="T">
/// The type of the non-null success payload.
/// </typeparam>
public sealed class Result<T> where T : notnull
{
    /// <summary>
    /// Gets whether the operation succeeded.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; private init; }

    /// <summary>
    /// Gets the payload on success. Check <see cref="IsSuccess" /> before reading it.
    /// </summary>
    public T? Value { get; private init; }

    /// <summary>
    /// Gets the message on failure and null on success.
    /// </summary>
    public string? Error { get; private init; }

    private Result()
    {
    }

    /// <summary>
    /// Creates a successful result carrying the supplied non-null value.
    /// </summary>
    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Result<T> { IsSuccess = true, Value = value };
    }

    /// <summary>
    /// Creates a failed result carrying the supplied non-null message.
    /// </summary>
    public static Result<T> Failure(string error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result<T> { IsSuccess = false, Error = error };
    }
}
