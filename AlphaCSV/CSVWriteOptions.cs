using System;
using System.Collections.Generic;
using System.Text;

namespace AlphaCSV {
    public class CSVWriteOptions : BaseCSVOptions {

        /// <summary>
        /// A flag that determines if the CSV header names should be written to the file.
        /// </summary>
        public bool WriteHeaders { get; set; }

        public CSVWriteOptions() {
            Delimeter = ',';
            QuoteCharacter = '"';
            WriteHeaders = true;
            DateTimeFormat = "dd-MMM-yyyy";
            DecimalSeperator = '.';
        }
    }
}
