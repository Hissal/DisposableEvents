#if NETSTANDARD2_0

using System.Reflection;

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices {
    internal static class RuntimeHelpers {
        public static bool IsReferenceOrContainsReferences<T>() {
            return IsReferenceOrContainsReferences(typeof(T));
        }

        static bool IsReferenceOrContainsReferences(Type type) {
            // Reference types always return true
            if (!type.IsValueType) {
                return true;
            }

            // Primitive types and enums do not contain references
            if (type.IsPrimitive || type.IsEnum) {
                return false;
            }

            // Check each field of the value type
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)) {
                var fieldType = field.FieldType;
                if (IsReferenceOrContainsReferences(fieldType)) {
                    return true;
                }
            }

            return false;
        }
    }
}

#endif