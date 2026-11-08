using AlphaCSV;
using System;
using Xunit;

namespace TestAlphaCSV;

public sealed class PropertyConverterTests {
    private readonly ITestOutputHelper _output;

    public PropertyConverterTests(ITestOutputHelper output) {
        _output = output;
    }

#pragma warning disable CA1707

    [Theory]
    [InlineData(null)]
    [InlineData(true)]
    public void ConvertValue_NullForNullableTarget_ReturnsNull(bool? useDbNull) {
        object? value = useDbNull is true ? DBNull.Value : null;

        Assert.Null(PropertyConverter.ConvertValue(value, typeof(int?)));
        Assert.Null(PropertyConverter.ConvertValue(value, typeof(string)));
    }

    [Fact]
    public void ConvertValue_NullForNonNullableTarget_Throws() {
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => PropertyConverter.ConvertValue(DBNull.Value, typeof(int)));

        Assert.Contains("Cannot assign null", exception.Message);
    }

    [Fact]
    public void ConvertValue_EnumTextWithSpaces_ParsesIgnoringCase() {
        object? result = PropertyConverter.ConvertValue("  fri day  ", typeof(DayOfWeek));

        Assert.Equal(DayOfWeek.Friday, result);
    }

    [Fact]
    public void ConvertValue_EmptyNullableEnum_ReturnsNull() {
        Assert.Null(PropertyConverter.ConvertValue("  ", typeof(DayOfWeek?)));
    }

    [Fact]
    public void ConvertValue_EmptyNonNullableEnum_Throws() {
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => PropertyConverter.ConvertValue(" ", typeof(DayOfWeek)));

        Assert.Contains("Cannot assign empty", exception.Message);
    }

    [Fact]
    public void ConvertValue_NumericEnum_ConvertsToEnum() {
        Assert.Equal(DayOfWeek.Tuesday, PropertyConverter.ConvertValue(2, typeof(DayOfWeek)));
    }

    [Fact]
    public void ConvertValue_GuidText_ParsesGuid() {
        Guid expected = Guid.NewGuid();

        Assert.Equal(expected, PropertyConverter.ConvertValue(expected.ToString(), typeof(Guid)));
        Assert.Equal(expected, PropertyConverter.ConvertValue(expected, typeof(Guid)));
    }

    [Fact]
    public void ConvertValue_TimeSpanText_ParsesInvariantValue() {
        TimeSpan expected = TimeSpan.FromHours(hours: 12);

        Assert.Equal(expected, PropertyConverter.ConvertValue("12:00:00", typeof(TimeSpan)));
        Assert.Equal(expected, PropertyConverter.ConvertValue(expected, typeof(TimeSpan)));
    }

    [Fact]
    public void ConvertValue_DateTimeOffsetText_ParsesInvariantValue() {
        DateTimeOffset expected = new(year: 2026, month: 1, day: 2, hour: 3, minute: 4, second: 5, offset: TimeSpan.Zero);

        Assert.Equal(expected, PropertyConverter.ConvertValue("2026-01-02T03:04:05+00:00", typeof(DateTimeOffset)));
        Assert.Equal(expected, PropertyConverter.ConvertValue(expected, typeof(DateTimeOffset)));
    }

    [Fact]
    public void ConvertValue_NumberToString_UsesInvariantCulture() {
        Assert.Equal("12", PropertyConverter.ConvertValue(12, typeof(string)));
    }

    [Fact]
    public void ConvertValue_NumberText_ConvertsToTargetType() {
        Assert.Equal(12, PropertyConverter.ConvertValue("12", typeof(int?)));
    }

#pragma warning restore CA1707
}
