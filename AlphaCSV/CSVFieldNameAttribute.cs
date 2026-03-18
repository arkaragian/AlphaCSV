using System;

namespace AlphaCSV;

[AttributeUsage(AttributeTargets.Property)]
public class CSVFieldNameAttribute(string name) : Attribute {
    public string FieldName { get; } = name;
}