namespace BusTicketReservation.Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;
    public List<string> Errors { get; private set; } = new();

    private Result(bool isSuccess, T? value, string errorMessage, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        Errors = errors ?? new List<string>();
    }

    public static Result<T> Success(T value) => new(true, value, string.Empty);

    public static Result<T> Failure(string errorMessage) => new(false, default, errorMessage);

    public static Result<T> Failure(List<string> errors) => new(false, default, string.Empty, errors);

    public static Result<T> Failure(string errorMessage, List<string> errors) => new(false, default, errorMessage, errors);
}

public class Result
{
    public bool IsSuccess { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;
    public List<string> Errors { get; private set; } = new();

    private Result(bool isSuccess, string errorMessage, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Errors = errors ?? new List<string>();
    }

    public static Result Success() => new(true, string.Empty);

    public static Result Failure(string errorMessage) => new(false, errorMessage);

    public static Result Failure(List<string> errors) => new(false, string.Empty, errors);

    public static Result Failure(string errorMessage, List<string> errors) => new(false, errorMessage, errors);
}