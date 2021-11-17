﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public bool ContainsHeaders { get; set; }

        /// <summary>
        /// A flag that determines if the CSV header names should be validated against the schema.
        /// </summary>
        public bool CheckHeaders { get; set; }

        /// <summary>
        /// A flag that determines if the fields will be validated against regular expression patterns.
        /// </summary>
        public bool ValidateFields { get; set; }

        public CSVParseOptions() {
            Delimeter = ',';
            CommentCharacter = '#';
            CheckHeaders = false;
            ValidateFields = false;
        }

    }
}
