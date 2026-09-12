using System;
using System.Collections.Generic;
using System.Reflection;

namespace AlphaCSV.Models;

/// <summary>
/// Associates a CSV field name and value type with the property path used to assign that value.
/// </summary>
/// <param name="name">The CSV field name, including dot-separated parent property names.</param>
/// <param name="propertyType">The type of the leaf property.</param>
/// <param name="propertyPath">The ordered property chain from the root object to the leaf property.</param>
public sealed class PropertyBinding(string name, Type propertyType, List<PropertyInfo> propertyPath) {
    public string Name { get; } = name;
    public Type PropertyType { get; } = propertyType;

    /// <summary>
    /// Assigns a converted CSV value to the bound leaf property, creating missing nested
    /// objects along the property path.
    /// </summary>
    /// <param name="instance">The root object on which the property path begins.</param>
    /// <param name="value">The converted value to assign to the leaf property.</param>
    public void SetValue(object instance, object? value) {
        object currentInstance = instance;
        for (int i = 0; i < propertyPath.Count; i++) {
            PropertyInfo property = propertyPath[i];
            if (i == propertyPath.Count - 1) {
                _ = property.SetMethod?.Invoke(currentInstance, [value]);
                return;
            }

            object? nestedInstance = property.GetValue(currentInstance);
            if (nestedInstance is null) {
                ConstructorInfo constructor = property.PropertyType.GetConstructor([])!;
                nestedInstance = constructor.Invoke(null);
                _ = property.SetMethod?.Invoke(currentInstance, [nestedInstance]);
            }
            currentInstance = nestedInstance;
        }
    }
}
