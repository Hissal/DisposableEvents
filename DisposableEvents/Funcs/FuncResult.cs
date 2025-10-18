using System.Diagnostics.CodeAnalysis;

namespace DisposableEvents;

public readonly record struct FuncResult<T> {
    public T? Value { get; }
    
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue { get; }
    
    FuncResult(T? value, bool hasValue = true) {
        Value = value;
        HasValue = hasValue;
    }
    
    public static implicit operator FuncResult<T>(T value) => From(value);
    public static implicit operator T?(FuncResult<T> result) => result.Value;
    
    public static FuncResult<T> From(T value) => new FuncResult<T>(value, true);
    public static FuncResult<T> Null() => new FuncResult<T>(default, false);
    
    public bool TryGetValue([NotNullWhen(true)] out T? value) {
        value = HasValue ? Value : default;
        return HasValue;
    }
    
    public TResult Map<TResult>(System.Func<T, TResult> onValue, Func<TResult> onNull) => 
        HasValue ? onValue(Value) : onNull();
    
    public TResult Map<TState, TResult>(TState state, Func<TState, T, TResult> onValue, System.Func<TState, TResult> onNull) => 
        HasValue ? onValue(state, Value) : onNull(state);
    
    public TResult Map<TResult>(System.Func<T, TResult> onValue, TResult onNull) => 
        HasValue ? onValue(Value) : onNull;
    
    public TResult Map<TState, TResult>(TState state, Func<TState, T, TResult> onValue, TResult onNull) => 
        HasValue ? onValue(state, Value) : onNull;
    
    public void Switch(Action<T> onValue, Action onNull) {
        if (HasValue) onValue(Value!);
        else onNull();
    }

    public void Switch<TState>(TState state, Action<TState, T> onValue, Action<TState> onNull) {
        if (HasValue) onValue(state, Value!);
        else onNull(state);
    }
    
    public void OnValue(Action<T> onValue) { 
        if (HasValue) onValue(Value!);
    }
    public void OnValue<TState>(TState state, Action<TState, T> onValue) { 
        if (HasValue) onValue(state, Value!);
    }
    
    public void OnNull(Action onNull) { 
        if (!HasValue) onNull();
    }
    public void OnNull<TState>(TState state, Action<TState> onNull) { 
        if (!HasValue) onNull(state);
    }
    
    public override string ToString() => HasValue ? $"FuncResult<{typeof(T).Name}>[{Value}]" : $"FuncResult<{typeof(T).Name}>[null]";
}