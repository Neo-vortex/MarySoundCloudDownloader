using OneOf;

namespace MarySoundCloudDownloader.Helpers;

/// <summary>
/// This is the request result class.
/// </summary>
/// <typeparam name="T">Type of the result on successful execution</typeparam>
[GenerateOneOf]
public partial class RequestResult<T> : OneOfBase<T, Exception>
{
    /// <summary>
    /// Indicates whether the request was successful.
    /// </summary>
    public bool IsSuccessful => IsT0;

    /// <summary>
    /// Indicates whether the request has failed.
    /// </summary>
    public bool IsFailed => IsT1;

    /// <summary>
    /// Gets the error if the request has failed.
    /// </summary>
    public Exception Error
    {
        get
        {
            if (IsFailed) return AsT1;
            throw new InvalidOperationException("There is no error since the request is successful.");
        }
    }

    /// <summary>
    /// Gets the actual value of the request result.
    /// </summary>
    public new T Value
    {
        get
        {
            if (IsSuccessful) return AsT0;
            throw new InvalidOperationException("There is no value since the request has failed.");
        }
    }

    /// <summary>
    /// Throws the exception if the request result has failed.
    /// </summary>
    /// <exception cref="Exception">Throws the exception that caused the failure.</exception>
    public void EnsureSuccess()
    {
        if (IsFailed) throw AsT1;
    }

    /// <summary>
    /// Tries to get the value if the request is successful.
    /// </summary>
    /// <param name="value">The output value if the request is successful.</param>
    /// <returns>True if the request is successful, otherwise false.</returns>
    public bool TryGetValue(out T? value)
    {
        if (IsSuccessful)
        {
            value = AsT0;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Tries to get the error if the request has failed.
    /// </summary>
    /// <param name="error">The output error if the request has failed.</param>
    /// <returns>True if the request has failed, otherwise false.</returns>
    public bool TryGetError(out Exception? error)
    {
        if (IsFailed)
        {
            error = AsT1;
            return true;
        }

        error = null;
        return false;
    }

    /// <summary>
    ///     Creates a successful request result.
    /// </summary>
    /// <param name="value">The value of the successful request.</param>
    /// <returns>A successful request result.</returns>
    public static RequestResult<T> Success(T? value)
    {
        return new RequestResult<T>(value);
    }

    /// <summary>
    ///     Creates a failed request result.
    /// </summary>
    /// <param name="error">The error message of the failed request.</param>
    /// <returns>A failed request result.</returns>
    public static RequestResult<T> Fail(string error)
    {
        return new RequestResult<T>(new Exception(error));
    }

    /// <summary>
    ///     Creates a failed request result.
    /// </summary>
    /// <param name="error">The exception of the failed request.</param>
    /// <returns>A failed request result.</returns>
    public static RequestResult<T> Fail(Exception error)
    {
        return new RequestResult<T>(error);
    }

    /// <summary>
    ///     Asynchronously ensures the request result is successful.
    /// </summary>
    /// <returns>A task that completes when the request result is ensured to be successful.</returns>
    public async Task EnsureSuccessAsync()
    {
        await Task.Run(EnsureSuccess);
    }

    /// <summary>
    ///     Converts the request result to a string representation.
    /// </summary>
    /// <returns>A string representation of the request result.</returns>
    public override string ToString()
    {
        return IsSuccessful ? $"Success: {Value}" : $"Error: {Error.Message}";
    }

    /// <summary>
    ///     Attempts to get the value asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the result value.</returns>
    public async Task<T?> TryGetValueAsync()
    {
        return await Task.Run(() => TryGetValue(out var value) ? value : default);
    }

    /// <summary>
    ///     Attempts to get the error asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the error.</returns>
    public async Task<Exception?> TryGetErrorAsync()
    {
        return await Task.Run(() => TryGetError(out var error) ? error : null);
    }
}
