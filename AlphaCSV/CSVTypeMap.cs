using System;
using System.Collections.Generic;

namespace AlphaCSV;

/// <summary>
/// Defines CSV fields without runtime property discovery, so typed CSV operations can be trimmed for Native AOT.
/// </summary>
public sealed class CSVTypeMap<T> {
    private readonly Func<T> _factory;
    private readonly List<CSVTypeField<T>> _fields = [];

    public CSVTypeMap(Func<T> factory) {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public CSVTypeMap<T> AddField<TValue>(string name, Func<T, TValue> getter, Action<T, TValue> setter) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(getter);
        ArgumentNullException.ThrowIfNull(setter);
        if (_fields.Exists(field => field.Name == name)) {
            throw new ArgumentException($"A field named {name} is already mapped.", nameof(name));
        }

        _fields.Add(new CSVTypeField<T>(name, typeof(TValue), value => getter(value),
            (instance, value) => setter(instance, (TValue)value!)));
        return this;
    }

    internal T Create() => _factory();

    internal IReadOnlyList<CSVTypeField<T>> Fields => _fields;
}

internal sealed record CSVTypeField<T>(string Name, Type ValueType, Func<T, object?> GetValue, Action<T, object?> SetValue);
