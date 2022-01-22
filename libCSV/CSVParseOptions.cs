//
// Copyright (c) Aris Karagiannidis and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

namespace libCSV {
    public class CSVParseOptions {
        /// <summary>
        /// The delimeter that will be used in the CSV file.
        /// </summary>
        public char Delimeter { get; set; }

        /// <summary>
        /// Determines the comment character. Lines starting with this character will be ignored.
        /// </summary>
        public char CommentCharacter { get; set; }

        /// <summary>
        /// Indicates the characted that comprises the quotes
        /// <remarks>The character is null if we don't have quoted fields</remarks>
        /// </summary>
        public char? QuoteCharacter { get; set; }

        /// <summary>
        /// Indicate if the file contains headers
        /// </summary>
        public bool ContainsHeaders { get; set; }

        /// <summary>
        /// A flag that determines if the CSV header names should be validated against the schema.
        /// </summary>
        public bool CheckHeaders { get; set; }

        /// <summary>
        /// A flag that determines if the fields will be validated against regular expression patterns.
        /// </summary>
        public bool ValidateFields { get; set; }

        /// <summary>
        /// Defines the format of the date time.
        /// <remarks>
        /// This format is empty by default.
        /// </remarks>
        /// </summary>
        public string DateTimeFormat { get; set; }

        /// <summary>
        /// Indicates wether the last field of the file lines can be empty. (eg when there is no data on the last field).
        /// <remarks>
        /// Some programs may not output anything if the last field that they are trying to output is empty not even an empty quote.
        /// This however might be in conflict with the schema that is why it can controlled by this option.
        /// </remarks>
        /// </summary>
        public bool AllowEmptyLastField { get; set; }

        public CSVParseOptions() {
            Delimeter = ',';
            CommentCharacter = '#';
            QuoteCharacter = '"';
            CheckHeaders = false;
            ValidateFields = false;
            DateTimeFormat = "";
            AllowEmptyLastField = true;
        }

    }
}
