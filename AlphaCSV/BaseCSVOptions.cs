using System;
using System.Collections.Generic;
using System.Text;

namespace AlphaCSV {
    /// <summary>
    /// Options that are common to both parsing and writing of CSV files.
    /// </summary>
    public class BaseCSVOptions {
        /// <summary>
        /// The delimeter that will be used in the CSV file.
        /// </summary>
        public char Delimeter { get; set; }

        /// <summary>
        /// Indicates the characted that comprises the quotes
        /// <remarks>The character is null if we don't have quoted fields</remarks>
        /// </summary>
        public char QuoteCharacter { get; set; }

        /// <summary>
        /// Defines the format of the date time.
        /// <remarks>
        /// This format is empty by default.
        /// </remarks>
        /// </summary>
        public string DateTimeFormat { get; set; }

        /// <summary>
        /// Defines the decimal seperator for parsing or writing non integer numbers.
        /// </summary>
        public char DecimalSeperator { get; set; }
    }
}
