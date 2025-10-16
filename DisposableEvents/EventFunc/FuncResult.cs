namespace DisposableEvents;

public readonly struct FuncResult<T> {
    public T Value { get; }
    public bool IsSuccess { get; }
    
    public FuncResult(T value, bool isSuccess = true) {
        Value = value;
        IsSuccess = isSuccess;
    }

    public FuncResult() {
        Value = default!;
        IsSuccess = false;
    }
    
    public static implicit operator FuncResult<T>(T value) => Success(value);
    public static implicit operator T(FuncResult<T> result) => result.Value;
    
    public static FuncResult<T> Success(T value) => new FuncResult<T>(value, true);
    public static FuncResult<T> Failure(T value = default!) => new FuncResult<T>(value, false);
    
    public void Deconstruct(out T value, out bool isSuccess) {
        value = Value;
        isSuccess = IsSuccess;
    }
    
    public TResult Map<TResult>(Func<T, TResult> onSuccess, Func<T, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(Value);
    
    public TResult Map<TResult>(Func<T, TResult> onSuccess, Func<TResult> onFailure) => 
        IsSuccess ? onSuccess(Value) : onFailure();
    
    public TResult Map<TResult>(Func<T, TResult> onSuccess, TResult onFailure) => 
        IsSuccess ? onSuccess(Value) : onFailure;
}