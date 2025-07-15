using System.Runtime.CompilerServices;

namespace DisposableEvents.Internal;

internal static class ThrowHelper {
    internal static void ThrowIfNull(object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null) {
        if (argument is null) {
            throw new ArgumentNullException(paramName);
        }
    }
}