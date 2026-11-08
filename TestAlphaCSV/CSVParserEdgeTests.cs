using AlphaCSV;
using System;
using System.Data;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace TestAlphaCSV;

public sealed class CSVParserEdgeTests {
    private readonly ITestOutputHelper _output;

    public CSVParserEdgeTests(ITestOutputHelper output) {
        _output = output;
    }

#pragma warning disable CA1707

    [Fact]
    public void ParseDefinedCSV_CommentBeforeHeader_SkipsCommentAndChecksHeader() {
        DataTable schema = new();
        _ = schema.Columns.Add("Value", typeof(string));
        MockFileSystem fileSystem = new();
        fileSystem.AddFile("test.csv", new MockFileData("# comment\nValue\ncontent\n"));
        CSVParseOptions options = new() { CheckHeaders = true };

        DataTable result = new CSVParser(fileSystem).ParseDefinedCSV(schema, "test.csv", options);

        Assert.Single(result.Rows);
        Assert.Equal("content", result.Rows[0][0]);
    }

    [Fact]
    public void ParseDefinedCSV_HeaderMismatch_ThrowsWithOptions() {
        DataTable schema = new();
        _ = schema.Columns.Add("Expected", typeof(string));
        MockFileSystem fileSystem = new();
        fileSystem.AddFile("test.csv", new MockFileData("Actual\nvalue\n"));
        CSVParseOptions options = new() { CheckHeaders = true };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => new CSVParser(fileSystem).ParseDefinedCSV(schema, "test.csv", options));

        Assert.Contains("Column Names do not match", exception.Message);
        Assert.Same(options, exception.Data["CSVParserOptions"]);
    }

    [Fact]
    public void ParseDefinedCSV_MissingValidators_ThrowsWithOptions() {
        DataTable schema = new();
        _ = schema.Columns.Add("Value", typeof(string));
        MockFileSystem fileSystem = new();
        fileSystem.AddFile("test.csv", new MockFileData("Value\ncontent\n"));
        CSVParseOptions options = new() { ValidateFields = true };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => new CSVParser(fileSystem).ParseDefinedCSV(schema, "test.csv", options));

        Assert.Contains("no validation patterns", exception.Message);
        Assert.Same(options, exception.Data["CSVParserOptions"]);
    }

    [Fact]
    public void ParseDefinedCSV_ValidatorCountMismatch_Throws() {
        DataTable schema = new();
        _ = schema.Columns.Add("Value", typeof(string));
        MockFileSystem fileSystem = new();
        fileSystem.AddFile("test.csv", new MockFileData("Value\ncontent\n"));
        CSVParseOptions options = new() { ValidateFields = true };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => new CSVParser(fileSystem).ParseDefinedCSV(schema, "test.csv", options, []));

        Assert.Contains("number of validation patterns", exception.Message);
    }

    [Fact]
    public void ParseDefinedCSV_DateAndDecimalFields_UseConfiguredFormats() {
        DataTable schema = new();
        _ = schema.Columns.Add("Date", typeof(DateTime));
        _ = schema.Columns.Add("Amount", typeof(decimal));
        MockFileSystem fileSystem = new();
        fileSystem.AddFile("test.csv", new MockFileData("Date;Amount\n2026/02/03;12,5\n"));
        CSVParseOptions options = new() {
            CommonOptions = new CSVOptions {
                Delimeter = ';',
                DateTimeFormat = "yyyy/MM/dd",
                DecimalSeperator = ',',
            },
        };

        DataTable result = new CSVParser(fileSystem).ParseDefinedCSV(schema, "test.csv", options);

        Assert.Equal(new DateTime(year: 2026, month: 2, day: 3), result.Rows[0][0]);
        Assert.Equal(12.5m, result.Rows[0][1]);
    }

    [Fact]
    public void ParseDefinedCSV_UnparseableDate_UsesMinimumValue() {
        DataTable schema = new();
        _ = schema.Columns.Add("Date", typeof(DateTime));
        MockFileSystem fileSystem = new();
        fileSystem.AddFile("test.csv", new MockFileData("Date\ninvalid\n"));

        DataTable result = new CSVParser(fileSystem).ParseDefinedCSV(schema, "test.csv");

        Assert.Equal(DateTime.MinValue, result.Rows[0][0]);
    }

    [Fact]
    public void ParseDefinedCSV_DefaultDecimalSeparator_ParsesInvariantNumber() {
        DataTable schema = new();
        _ = schema.Columns.Add("Amount", typeof(decimal));
        MockFileSystem fileSystem = new();
        fileSystem.AddFile("test.csv", new MockFileData("Amount\n12.5\n"));

        DataTable result = new CSVParser(fileSystem).ParseDefinedCSV(schema, "test.csv");

        Assert.Equal(12.5m, result.Rows[0][0]);
    }

    [Fact]
    public void ParseSimpleCSV_TrimmedQuotedField_ReturnsTrimmedValue() {
        MockFileSystem fileSystem = new();
        fileSystem.AddFile("test.csv", new MockFileData("Value\n\" hello \"\n"));
        CSVParseOptions options = new() { TrimFields = true };

        DataTable result = new CSVParser(fileSystem).ParseSimpleCSV("test.csv", options);

        Assert.Equal("hello", result.Rows[0][0]);
    }

    [Fact]
    public void ParseSimpleCSV_TrimmedFieldsSeparatedByDelimiter_ReturnsTrimmedValues() {
        MockFileSystem fileSystem = new();
        fileSystem.AddFile("test.csv", new MockFileData("First,Second\n  hello  ,  world  \n"));
        CSVParseOptions options = new() { TrimFields = true };

        DataTable result = new CSVParser(fileSystem).ParseSimpleCSV("test.csv", options);

        Assert.Equal("hello", result.Rows[0][0]);
        Assert.Equal("world", result.Rows[0][1]);
    }

    [Fact]
    public void ParseDefinedCSV_ColumnCountMismatch_ThrowsWithRowNumber() {
        DataTable schema = new();
        _ = schema.Columns.Add("First", typeof(string));
        _ = schema.Columns.Add("Second", typeof(string));
        MockFileSystem fileSystem = new();
        fileSystem.AddFile("test.csv", new MockFileData("First,Second\nvalue\n"));

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => new CSVParser(fileSystem).ParseDefinedCSV(schema, "test.csv"));

        Assert.Contains("Offending row:2", exception.Message);
    }

    [Fact]
    public void ParseType_TypeHasNoPublicDefaultConstructor_Throws() {
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => new CSVParser().ParseType<NoDefaultConstructor>("unused.csv"));

        Assert.Contains("does not have a constructor", exception.Message);
    }

    [Fact]
    public void ParseType_ColumnCountIsEnforced_ThrowsForUnmappedColumn() {
        MockFileSystem fileSystem = new();
        fileSystem.AddFile("test.csv", new MockFileData("Name,Extra\nAda,value\n"));
        CSVParseOptions options = new() { EnforceColumnCount = true };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => new CSVParser(fileSystem).ParseType<NamedRecord>("test.csv", options));

        Assert.Contains("number of parsed columns", exception.Message);
    }

    [Fact]
    public void ParseType_UnknownColumnIsIgnored_WhenColumnCountIsNotEnforced() {
        MockFileSystem fileSystem = new();
        fileSystem.AddFile("test.csv", new MockFileData("Name,Extra\nAda,value\n"));

        NamedRecord record = Assert.Single(new CSVParser(fileSystem).ParseType<NamedRecord>("test.csv"));

        Assert.Equal("Ada", record.Name);
    }

    [Fact]
    public void ParseType_MappedColumnCountIsEnforced_Throws() {
        MockFileSystem fileSystem = new();
        fileSystem.AddFile("test.csv", new MockFileData("Name,Extra\nAda,value\n"));
        CSVTypeMap<NamedRecord> map = new(() => new NamedRecord());
        _ = map.AddField("Name", record => record.Name, (record, value) => record.Name = value);
        CSVParseOptions options = new() { EnforceColumnCount = true };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => new CSVParser(fileSystem).ParseType("test.csv", map, options));

        Assert.Contains("number of parsed columns", exception.Message);
    }

    [Fact]
    public void ParseType_ReadOnlyPropertyIsIgnored_AndAttributeMapsField() {
        MockFileSystem fileSystem = new();
        fileSystem.AddFile("test.csv", new MockFileData("display_name\nAda\n"));

        AttributedRecord record = Assert.Single(new CSVParser(fileSystem).ParseType<AttributedRecord>("test.csv"));

        Assert.Equal("Ada", record.Name);
        Assert.Equal("fixed", record.ReadOnly);
    }

    [Fact]
    public void ParseType_NestedCycle_ThrowsBeforeReadingFile() {
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => new CSVParser().ParseType<CyclicRecord>("unused.csv"));

        Assert.Contains("cycle", exception.Message);
    }

    [Fact]
    public void ParseType_InitializedNestedObject_PreservesExistingObject() {
        MockFileSystem fileSystem = new();
        fileSystem.AddFile("test.csv", new MockFileData("Child.Value\nnew\n"));

        InitializedParent record = Assert.Single(new CSVParser(fileSystem).ParseType<InitializedParent>("test.csv"));

        Assert.Equal("new", record.Child.Value);
        Assert.Equal("kept", record.Child.Other);
    }

    private sealed class NoDefaultConstructor(string value) {
        public string Value { get; } = value;
    }

    private sealed class NamedRecord {
        public string Name { get; set; } = "";
    }

    private sealed class AttributedRecord {
        [CSVFieldName("display_name")]
        public string Name { get; set; } = "";

        public string ReadOnly => "fixed";
    }

    private sealed class CyclicRecord {
        public CyclicRecord? Child { get; set; }
    }

    private sealed class InitializedParent {
        public InitializedChild Child { get; set; } = new() { Other = "kept" };
    }

    private sealed class InitializedChild {
        public string Value { get; set; } = "";
        public string Other { get; set; } = "";
    }

#pragma warning restore CA1707
}
