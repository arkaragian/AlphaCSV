// Copyright (c) Aris Karagiannidis and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace AlphaCSV;

public class CSVParseOptions {


    /// <summary>
    /// Defines common options that apply to both read and write operations
    /// of CSV files.
    /// </summary>
    public CSVOptions CommonOptions { get; set; } = new CSVOptions();


    /// <summary>
    /// Determines the comment character. Lines starting with this character will be ignored.
    /// </summary>
    public char CommentCharacter { get; set; } = '#';

    /// <summary>
    /// Indicate if the file contains headers
    /// </summary>
    public bool ContainsHeaders { get; set; } = true;

    /// <summary>
    /// A flag that determines if the CSV header names should be validated
    /// against the schema.
    /// </summary>
    public bool CheckHeaders { get; set; }

    /// <summary>
    /// A flag that determines if the fields will be validated against regular expression patterns.
    /// </summary>
    public bool ValidateFields { get; set; }

    /// <summary>
    /// Inidcates if any whitespace will be trimmed from the front and back side
    /// of all fields
    /// </summary>
    public bool TrimFields { get; set; }

    /// <summary>
    /// Indicates wether the last field of the file lines can be empty. (eg when
    /// there is no data on the last field).
    /// </summary>
    ///
    /// <remarks>
    /// Some programs may not output anything if the last field that they are trying
    /// to output is empty not even an empty quote. This however might be in conflict
    /// with the schema that is why it can controlled by this option.
    /// </remarks>
    public bool AllowEmptyLastField { get; set; } = true;
}