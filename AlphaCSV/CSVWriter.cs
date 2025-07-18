﻿using AlphaCSV.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Abstractions;
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
                if (data.Rows[i].ItemArray[j] == null) {
                    field = "";
                } else {
                    field = data.Rows[i].ItemArray[j].ToString();
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
}