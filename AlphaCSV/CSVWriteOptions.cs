using System.Text;

namespace AlphaCSV;

/// <summary>
/// Represents the options for a CSV writer
/// </summary>
public class CSVWriteOptions : BaseCSVOptions {

    /// <summary>
    /// A flag that determines if the CSV header names should be written to the file.
    /// </summary>
    public bool WriteHeaders { get; set; }

    /// <summary>
    /// A Flag that forces field quotation even if the field does not contain the delimeter.
    /// </summary>
    public bool QuoteFieldsWithoutDelimeter { get; set; }

    /// <summary>
    /// Specifices the encoding of the file that is written.
    /// </summary>
    /// <remarks>
    /// Default value is UTF8
    /// </remarks>
    public Encoding Encoding { get; init; }

    public CSVWriteOptions() {
        Delimeter = ',';
        QuoteCharacter = '"';
        QuoteFieldsWithoutDelimeter = false;
        WriteHeaders = true;
        DateTimeFormat = "dd-MMM-yyyy";
        DecimalSeperator = '.';
        Encoding = Encoding.UTF8;
    }
}