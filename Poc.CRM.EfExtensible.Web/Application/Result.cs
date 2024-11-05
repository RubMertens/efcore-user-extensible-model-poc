namespace Poc.CRM.EfExtensible.Web.Application;

/// <summary>
/// Result object commonly returned by handlers to indicate success or failure.
/// Static methods are provided to create a successful or failed result.
/// </summary>
/// <typeparam name="TResult"></typeparam>
public class Result<TResult>
{
    private Result(TResult? data)
    {
        Data = data;
    }

    public TResult Data { get; init; }
    public DomainError? Error { get; set; }
    public bool Success { get; init; }
    public bool Failed => !Success;

    public static Result<TResult> Succeed(TResult result) => new(result) { Success = true };
    public static Result<TResult> Fail(DomainError error) => new(default) { Success = false, Error = error };
}