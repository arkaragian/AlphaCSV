namespace AlphaCSV;

/// <summary>
/// Represents the options for a CSV writer
/// </summary>
public sealed class CSVWriteOptions {

    /// <summary>
    /// Defines common options that apply to both read and write operations
    /// of CSV files.
    /// </summary>
    public CSVOptions CommonOptions { get; set; } = new CSVOptions() {
        DateTimeFormat = "dd-MMM-yyyy"
    };

    /// <summary>
    /// A flag that determines if the CSV header names should be written to the file.
    /// </summary>
    public bool WriteHeaders { get; set; } = true;

    /// <summary>
    /// A Flag that forces field quotation even if the field does not contain the delimeter.
    /// </summary>
    public bool QuoteFieldsWithoutDelimeter { get; set; }

}