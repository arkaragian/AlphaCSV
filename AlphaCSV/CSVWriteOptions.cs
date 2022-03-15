using System;
using System.Collections.Generic;
using System.Text;

namespace AlphaCSV {
    public class CSVWriteOptions : BaseCSVOptions {

        /// <summary>
        /// A flag that determines if the CSV header names should be written to the file.
        /// </summary>
        public bool WriteHeaders { get; set; }

        /// <summary>
        /// A Flag that forces field quotation even if the field does not contain the delimeter.
        /// </summary>
        public bool QuoteFieldsWithoutDelimeter { get; set; }

        public CSVWriteOptions() {
            Delimeter = ',';
            QuoteCharacter = '"';
            QuoteFieldsWithoutDelimeter = false;
            WriteHeaders = true;
            DateTimeFormat = "dd-MMM-yyyy";
            DecimalSeperator = '.';
        }
    }
}
