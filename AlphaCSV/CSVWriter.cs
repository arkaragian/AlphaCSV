using AlphaCSV.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using System.Text;

namespace AlphaCSV;

/// <summary>
/// A class that writes a CSV file to disk
/// </summary>
public class CSVWriter : ICSVWriter {

    private readonly IFileSystem FSInterface;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="fileSystem"></param>
    public CSVWriter(IFileSystem fileSystem) {
        FSInterface = fileSystem;
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public CSVWriter() : this(new FileSystem()) {
    }

    /// <summary>
    /// Writes a datatable to a CSV file.
    /// </summary>
    /// <param name="filename">The file where the csv data will be written to</param>
    /// <param name="data">The data to write</param>
    /// <param name="options">CSV Parsing options</param>
    /// <exception cref="InvalidOperationException"></exception>
    public void WriteCSV(string filename, DataTable data, CSVWriteOptions? options = null) {
        options ??= new CSVWriteOptions();

        StringBuilder sb = new();
        string? header = null;
        if (options.WriteHeaders) {
            for (int i = 0; i < data.Columns.Count; i++) {
                string colName = data.Columns[i].ColumnName;
                bool quoted = false;

                //If the field contains the file delimeter. We need to enclose it in quotes
                //however if now, the field contains both the delimeter and the quote then we
                //need to escape the quote.
                if (colName.Contains(options.CommonOptions.Delimeter) || colName.Contains('"') || options.QuoteFieldsWithoutDelimeter) {
                    _ = sb.Append(options.CommonOptions.QuoteCharacter);
                    quoted = true;
                }

                foreach (char c in colName) {
                    //If we are in a quoted field and we find a quote character inside the fields
                    //we must escape it according to RFC4180 section 2 item 7.
                    if (c == options.CommonOptions.QuoteCharacter && quoted) {
                        _ = sb.Append(c);
                    }
                    _ = sb.Append(c);
                }

                if (quoted) {
                    _ = sb.Append(options.CommonOptions.QuoteCharacter);
                    quoted = false;
                }

                if (i != data.Columns.Count - 1) {
                    _ = sb.Append(options.CommonOptions.Delimeter);
                }
            }
            _ = sb.Append('\r');
            _ = sb.Append('\n');
            header = sb.ToString();
            _ = sb.Clear();
        }

        List<string> lines = [];

        for (int i = 0; i < data.Rows.Count; i++) {
            for (int j = 0; j < data.Rows[i].ItemArray.Length; j++) {
                string field;
                object? obj = data.Rows[i].ItemArray[j];
                if (obj is null) {
                    field = "";
                } else {
                    field = obj.ToString() ?? "";
                }
                bool quoted = false;


                //If the field contains the file delimeter. We need to enclose it in quotes
                //however if now, the field contains both the delimeter and the quote then we
                //need to escape the quote.
                if (field.IndexOf(options.CommonOptions.Delimeter) >= 0 || field.IndexOf('"') >= 0 || options.QuoteFieldsWithoutDelimeter) {
                    sb.Append(options.CommonOptions.QuoteCharacter);
                    quoted = true;
                }

                foreach (char c in field) {
                    //If we are in a quoted field and we find a quote character inside the fields
                    //we must escape it according to RFC4180 section 2 item 7.
                    if (c == options.CommonOptions.QuoteCharacter && quoted) {
                        sb.Append(c);
                    }
                    sb.Append(c);
                }

                if (quoted) {
                    sb.Append(options.CommonOptions.QuoteCharacter);
                    quoted = false;
                }


                if (j != data.Rows[i].ItemArray.Length - 1) {
                    sb.Append(options.CommonOptions.Delimeter);
                }
            }
            sb.Append('\r');
            sb.Append('\n');
            lines.Add(sb.ToString());
            sb.Clear();
        }

        //Since we are using a stream we must first delete the file. If the file exists and the contents that we need to write are
        //fewer then the resuling file will the the contents that we wrote plus the previous contents that we did not override.
        if (FSInterface.File.Exists(filename)) {
            FSInterface.File.Delete(filename);
        }
        Stream fileStream = FSInterface.File.OpenWrite(filename);
        StreamWriter writer = new(fileStream, options.CommonOptions.FileEncoding, bufferSize: -1, leaveOpen: false);
        if (header != null) {
            writer.Write(header);
        }
        for (int i = 0; i < lines.Count; i++) {
            writer.Write(lines[i]);
        }
        writer.Flush();
        writer.Close();
    }

    public static string WriteType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(IEnumerable<T> input, CSVWriteOptions? options) {
        options ??= new CSVWriteOptions();

        Type genType = typeof(T);

        PropertyInfo[] properties = genType.GetProperties();

        List<string> strValues = [];

        foreach (PropertyInfo pi in properties) {

            MethodInfo? getter = pi.GetMethod;

            if (getter is null) {
                continue;
            }

            if (!getter.IsPublic) {
                continue;
            }

            strValues.Add(pi.Name);
        }

        StringBuilder builder = new();

        builder.Append(string.Join(",", strValues)).Append('\n');

        foreach(T i in input) {
            builder.Append(WriteTypeLine(i, options)).Append('\n');
        }

        return builder.ToString();

    }

    /// <summary>
    ///     Parses a CSV file containing values for the public set properties of
    ///     the specified type
    /// </summary>
    /// <typeparam name="T">
    ///     The type that we will receive
    /// </typeparam>
    /// <param name="path">
    ///     The path of the CSV File
    /// </param>
    /// <param name="options">
    ///     CSV Parsing options
    /// </param>
    /// <param name="validationPatterns">Validation Patterns</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string WriteTypeLine<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T input, CSVWriteOptions? options) {
        options ??= new CSVWriteOptions();

        Type genType = typeof(T);

        PropertyInfo[] properties = genType.GetProperties();

        List<string> strValues = [];

        foreach (PropertyInfo pi in properties) {

            MethodInfo? getter = pi.GetMethod;

            if (getter is null) {
                continue;
            }

            if (!getter.IsPublic) {
                continue;
            }


            object? val = getter.Invoke(input, parameters: null);

            if (val is null) {
                strValues.Add("");
                continue;
            }

            strValues.Add(FormatValue(val, options));

        }

        return string.Join(",", strValues);

    }

    /// <summary>Writes mapped objects without runtime property discovery.</summary>
    public static string WriteType<T>(IEnumerable<T> input, CSVTypeMap<T> map, CSVWriteOptions? options = null) {
        ArgumentNullException.ThrowIfNull(map);
        StringBuilder builder = new();
        List<string> names = [];
        foreach (CSVTypeField<T> field in map.Fields) {
            names.Add(field.Name);
        }
        builder.Append(string.Join(",", names)).Append('\n');
        foreach (T item in input) {
            builder.Append(WriteTypeLine(item, map, options)).Append('\n');
        }
        return builder.ToString();
    }

    /// <summary>Writes a mapped object without runtime property discovery.</summary>
    public static string WriteTypeLine<T>(T input, CSVTypeMap<T> map, CSVWriteOptions? options = null) {
        ArgumentNullException.ThrowIfNull(map);
        options ??= new CSVWriteOptions();
        List<string> values = [];
        foreach (CSVTypeField<T> field in map.Fields) {
            values.Add(FormatValue(field.GetValue(input), options));
        }
        return string.Join(",", values);
    }

    private static string FormatValue(object? value, CSVWriteOptions options) {
        string text = value switch {
            null => "",
            bool b => b ? "true" : "false",
            DateTime dt => dt.ToString(options.CommonOptions.DateTimeFormat, CultureInfo.InvariantCulture),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? ""
        };
        if (text.Contains('"')) {
            text = text.Replace("\"", "\"\"");
        }
        if (text.Contains(',') || text.Contains('"') || text.Contains('\r') || text.Contains('\n')) {
            text = $"\"{text}\"";
        }
        return text;
    }
}
