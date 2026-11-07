namespace Smurfs.Application.Core;

/// <summary>
/// Represents an application outcome with a success value or an error message.
/// </summary>
public class Result<T>
{
    /// <summary>
    /// Gets or sets whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the payload. Read it only after checking for success.
    /// </summary>
    public T Value { get; set; }

    /// <summary>
    /// Gets or sets the message associated with a failed operation.
    /// </summary>
    public string Error { get; set; }

    /// <summary>
    /// Creates a successful result carrying the supplied value.
    /// </summary>
    public static Result<T> Success(T value) => new Result<T> { IsSuccess = true, Value = value };

    /// <summary>
    /// Creates a failed result carrying the supplied message.
    /// </summary>
    public static Result<T> Failure(string error) => new Result<T> { IsSuccess = false, Error = error };
}
