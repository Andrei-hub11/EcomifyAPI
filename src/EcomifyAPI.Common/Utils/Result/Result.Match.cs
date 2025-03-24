namespace EcomifyAPI.Common.Utils.Result;

public partial class Result<T> : Result
{
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<IReadOnlyList<IError>, TResult> onFailure)
    {
        if (this is Result<T> result)
        {
            return !result.IsFailure ? onSuccess(result.Value) : onFailure(result.Errors);
        }
        else
        {
            throw new InvalidOperationException("Match called on non-generic Result.");
        }
    }
}