namespace DisposableEvents;

public readonly record struct FilterResult(bool Passed) {
    public bool Blocked => !Passed;
    
    public static FilterResult Pass => new FilterResult(true);
    public static FilterResult Block => new FilterResult(false);
    
    public static implicit operator FilterResult(bool shouldPass) => new FilterResult(shouldPass);
    public static implicit operator bool(FilterResult result) => result.Passed;
}