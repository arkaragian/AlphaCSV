using AlphaCSV;
using System;
using System.Data;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace TestAlphaCSV;

public sealed class CSVWriterEdgeTests {
    private readonly ITestOutputHelper _output;

    public CSVWriterEdgeTests(ITestOutputHelper output) {
        _output = output;
    }

#pragma warning disable CA1707

    [Fact]
    public void WriteCSV_HeaderContainsQuote_EscapesHeader() {
        DataTable table = new();
        _ = table.Columns.Add("A\"B", typeof(string));
        MockFileSystem fileSystem = new();

        new CSVWriter(fileSystem).WriteCSV("test.csv", table);

        Assert.Equal("\"A\"\"B\"\r\n", fileSystem.File.ReadAllText("test.csv"));
    }

    [Fact]
    public void WriteCSV_ExistingFile_ReplacesOldContents() {
        DataTable table = new();
        _ = table.Columns.Add("Value", typeof(string));
        _ = table.Rows.Add("new");
        MockFileSystem fileSystem = new();
        fileSystem.AddFile("test.csv", new MockFileData("an old file with more contents"));

        new CSVWriter(fileSystem).WriteCSV("test.csv", table);

        Assert.Equal("Value\r\nnew\r\n", fileSystem.File.ReadAllText("test.csv"));
    }

    [Fact]
    public void WriteCSV_HeadersDisabled_WritesOnlyDataRows() {
        DataTable table = new();
        _ = table.Columns.Add("Value", typeof(string));
        _ = table.Rows.Add("content");
        MockFileSystem fileSystem = new();
        CSVWriteOptions options = new() { WriteHeaders = false };

        new CSVWriter(fileSystem).WriteCSV("test.csv", table, options);

        Assert.Equal("content\r\n", fileSystem.File.ReadAllText("test.csv"));
    }

    [Fact]
    public void WriteTypeLine_MappedNullValue_WritesEmptyField() {
        CSVTypeMap<NullableRecord> map = new(() => new NullableRecord());
        _ = map.AddField("Value", record => record.Value, (record, value) => record.Value = value);

        Assert.Equal("", CSVWriter.WriteTypeLine(new NullableRecord(), map));
    }

    [Fact]
    public void WriteTypeLine_MappedValueWithOptions_WritesValue() {
        CSVTypeMap<NullableRecord> map = new(() => new NullableRecord());
        _ = map.AddField("Value", record => record.Value, (record, value) => record.Value = value);

        Assert.Equal("content", CSVWriter.WriteTypeLine(new NullableRecord { Value = "content" }, map, new CSVWriteOptions()));
    }

    [Fact]
    public void WriteCSV_ValueToStringReturnsNull_WritesEmptyField() {
        DataTable table = new();
        _ = table.Columns.Add("Value", typeof(object));
        _ = table.Rows.Add(new NullTextValue());
        MockFileSystem fileSystem = new();

        new CSVWriter(fileSystem).WriteCSV("test.csv", table);

        Assert.Equal("Value\r\n\r\n", fileSystem.File.ReadAllText("test.csv"));
    }

    [Fact]
    public void WriteTypeLine_MappedValueToStringReturnsNull_WritesEmptyField() {
        CSVTypeMap<NullTextRecord> map = new(() => new NullTextRecord());
        _ = map.AddField("Value", record => record.Value, (record, value) => record.Value = value);

        Assert.Equal("", CSVWriter.WriteTypeLine(new NullTextRecord(), map));
    }

    [Fact]
    public void WriteCSV_DefaultFileSystem_WritesReadableFile() {
        string path = Path.Combine(Path.GetTempPath(), $"alphacsv-{Guid.NewGuid():N}.csv");
        DataTable table = new();
        _ = table.Columns.Add("Value", typeof(string));
        _ = table.Rows.Add("content");

        try {
            new CSVWriter().WriteCSV(path, table);

            DataTable parsed = new CSVParser().ParseSimpleCSV(path);
            Assert.Equal("content", parsed.Rows[0][0]);
        } finally {
            File.Delete(path);
        }
    }

    private sealed class NullableRecord {
        public string? Value { get; set; }
    }

    private sealed class NullTextRecord {
        public NullTextValue Value { get; set; } = new();
    }

    private sealed class NullTextValue {
        public override string? ToString() => null;
    }

#pragma warning restore CA1707
}
