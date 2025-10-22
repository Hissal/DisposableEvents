namespace DisposableEvents.Tests;

public static class ReflectionExtensions {
    public static TField GetPrivateField<TField>(this object obj, string fieldName) {
        var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field == null)
            throw new InvalidOperationException($"Field '{fieldName}' not found in type '{obj.GetType().FullName}'.");

        return (TField)field.GetValue(obj)!;
    }
    
    public static TField GetAnyPrivateFieldOfType<TField>(this object obj) => obj.GetAnyPrivateFieldOfType<TField>(out _);
    public static TField GetAnyPrivateFieldOfType<TField>(this object obj, out string fieldName) {
        var field = obj.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .FirstOrDefault(f => f.FieldType == typeof(TField));
        
        if (field == null)
            throw new InvalidOperationException($"Field of type '{typeof(TField).FullName}' not found in type '{obj.GetType().FullName}'.");

        fieldName = field.Name;
        return (TField)field.GetValue(obj)!;
    }
    
    public static TProperty GetPrivateProperty<TProperty>(this object obj, string propertyName) {
        var property = obj.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (property == null)
            throw new InvalidOperationException($"Property '{propertyName}' not found in type '{obj.GetType().FullName}'.");

        return (TProperty)property.GetValue(obj)!;
    }
    
    public static TProperty GetAnyPrivatePropertyOfType<TProperty>(this object obj) => obj.GetAnyPrivatePropertyOfType<TProperty>(out _);
    public static TProperty GetAnyPrivatePropertyOfType<TProperty>(this object obj, out string propertyName) {
        var property = obj.GetType().GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .FirstOrDefault(p => p.PropertyType == typeof(TProperty));
        
        if (property == null)
            throw new InvalidOperationException($"Property of type '{typeof(TProperty).FullName}' not found in type '{obj.GetType().FullName}'.");

        propertyName = property.Name;
        return (TProperty)property.GetValue(obj)!;
    }
    
    public static T GetPrivateFieldOrProperty<T>(this object obj, string name) {
        var type = obj.GetType();
        
        var field = type.GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null && field.FieldType == typeof(T)) {
            return (T)field.GetValue(obj)!;
        }
        
        var property = type.GetProperty(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (property != null && property.PropertyType == typeof(T)) {
            return (T)property.GetValue(obj)!;
        }
        
        throw new InvalidOperationException($"Field or property '{name}' of type '{typeof(T).FullName}' not found in type '{type.FullName}'.");
    }
    
    public static T GetAnyPrivateFieldOrPropertyOfType<T>(this object obj) => obj.GetAnyPrivateFieldOrPropertyOfType<T>(out _);
    public static T GetAnyPrivateFieldOrPropertyOfType<T>(this object obj, out string name) {
        var type = obj.GetType();
        
        var field = type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .FirstOrDefault(f => f.FieldType == typeof(T));
        if (field != null) {
            name = field.Name;
            return (T)field.GetValue(obj)!;
        }
        
        var property = type.GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .FirstOrDefault(p => p.PropertyType == typeof(T));
        if (property != null) {
            name = property.Name;
            return (T)property.GetValue(obj)!;
        }
        
        throw new InvalidOperationException($"Field or property of type '{typeof(T).FullName}' not found in type '{type.FullName}'.");
    }
}