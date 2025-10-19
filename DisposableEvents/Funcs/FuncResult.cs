using System.Diagnostics.CodeAnalysis;

namespace DisposableEvents;

public readonly record struct FuncResult<TValue> {
    public TValue? Value { get; init; }
    
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue { get; init; }
    
    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsNull => !HasValue;
    
    FuncResult(TValue? value, bool hasValue = true) {
        Value = value;
        HasValue = hasValue;
    }
    
    public static implicit operator FuncResult<TValue>(TValue value) => From(value);
    public static implicit operator TValue?(FuncResult<TValue> result) => result.Value;
    
    public static FuncResult<TValue> From(TValue value) => new FuncResult<TValue>(value, true);
    public static FuncResult<TValue> Null() => new FuncResult<TValue>(default, false);
    
    public TValue? GetValueOrDefault() => HasValue ? Value : default;
    public TValue GetValueOrDefault(TValue defaultValue) => HasValue ? Value : defaultValue;
    
    public bool TryGetValue([NotNullWhen(true)] out TValue? value) {
        value = HasValue ? Value : default;
        return HasValue;
    }
    
    public TResult Match<TResult>(Func<TValue, TResult> onValue, Func<TResult> onNull) => 
        HasValue ? onValue(Value) : onNull();
    
    public TResult Match<TState, TResult>(TState state, Func<TState, TValue, TResult> onValue, Func<TState, TResult> onNull) => 
        HasValue ? onValue(state, Value) : onNull(state);
    
    public TResult Match<TResult>(Func<TValue, TResult> onValue, TResult onNull) => 
        HasValue ? onValue(Value) : onNull;
    
    public TResult Match<TState, TResult>(TState state, Func<TState, TValue, TResult> onValue, TResult onNull) => 
        HasValue ? onValue(state, Value) : onNull;
    
    public FuncResult<TResult> Select<TResult>(Func<TValue, FuncResult<TResult>> selector) =>
        HasValue ? selector(Value) : FuncResult<TResult>.Null();
    
    public FuncResult<TResult> Select<TState, TResult>(TState state, Func<TState, TValue, FuncResult<TResult>> selector) =>
        HasValue ? selector(state, Value) : FuncResult<TResult>.Null();
    
    public void Switch(Action<TValue> onValue, Action onNull) {
        if (HasValue) onValue(Value!);
        else onNull();
    }

    public void Switch<TState>(TState state, Action<TState, TValue> onValue, Action<TState> onNull) {
        if (HasValue) onValue(state, Value!);
        else onNull(state);
    }
    
    public FuncResult<TValue> OnValue(Action<TValue> onValue) { 
        if (HasValue) onValue(Value!);
        return this;
    }
    public FuncResult<TValue> OnValue<TState>(TState state, Action<TState, TValue> onValue) { 
        if (HasValue) onValue(state, Value!);
        return this;
    }
    
    public FuncResult<TValue> OnNull(Action onNull) { 
        if (!HasValue) onNull();
        return this;
    }
    public FuncResult<TValue> OnNull<TState>(TState state, Action<TState> onNull) { 
        if (!HasValue) onNull(state);
        return this;
    }
    
    public override string ToString() => HasValue ? $"FuncResult<{typeof(TValue).Name}>[{Value}]" : $"FuncResult<{typeof(TValue).Name}>[null]";
}