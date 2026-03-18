using System;
using System.Globalization;

namespace AlphaCSV;

public static class PropertyConverter {


    public static object? ConvertValue(object? rawValue, Type targetType) {
        if (rawValue is null || rawValue is DBNull) {
            if (!targetType.IsValueType || Nullable.GetUnderlyingType(targetType) is not null) {
                return null;
            }

            throw new InvalidOperationException($"Cannot assign null to non-nullable type {targetType.FullName}.");
        }

        Type effectiveType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (effectiveType.IsEnum) {
            if (rawValue is string enumText) {
                if (string.IsNullOrWhiteSpace(enumText)) {
                    if (Nullable.GetUnderlyingType(targetType) is not null) {
                        return null;
                    }
                    throw new InvalidOperationException($"Cannot assign empty value to enum type {effectiveType.FullName}.");
                }

                string normalized = enumText.Replace(" ", string.Empty).Trim();

                return Enum.Parse(effectiveType, normalized, ignoreCase: true);
            }

            object numericValue = Convert.ChangeType(
                rawValue,
                Enum.GetUnderlyingType(effectiveType),
                CultureInfo.InvariantCulture);

            return Enum.ToObject(effectiveType, numericValue);
        }

        if (effectiveType == typeof(Guid)) {
            if (rawValue is string guidText) {
                return Guid.Parse(guidText);
            }
        }

        if (effectiveType == typeof(TimeSpan)) {
            if (rawValue is string timeSpanText) {
                return TimeSpan.Parse(timeSpanText, CultureInfo.InvariantCulture);
            }
        }

        if (effectiveType == typeof(DateTimeOffset)) {
            if (rawValue is string dtoText) {
                return DateTimeOffset.Parse(dtoText, CultureInfo.InvariantCulture);
            }
        }

        if (effectiveType == typeof(string)) {
            return Convert.ToString(rawValue, CultureInfo.InvariantCulture);
        }

        return Convert.ChangeType(rawValue, effectiveType, CultureInfo.InvariantCulture);
    }


}