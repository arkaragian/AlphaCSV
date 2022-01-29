//
// Copyright (c) Aris Karagiannidis and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//
using System.Data;

namespace libCSV {
    public partial class CSVParser {


        /// <summary>
        /// Parses a CSV file without a given schema.
        /// <remarks>This method will assume that all the fields are of string type.</remarks>
        /// </summary>
        /// <param name="path">The path of the file that will be read</param>
        /// <param name="options">(Optional argument) The options of the file that will be read.</param>
        /// <param name="validationPatterns">(Optional Argument) Validation patterns in terms of regular expressions</param>
        /// <returns>The contents of a CSV file in the form of a datatable</returns>
        public DataTable ParseSimpleCSV(string path, CSVParseOptions options = null, List<string> validationPatterns = null) {
            //Before we continue we need to make some assumptions for the file. e.g to know how many fields we need to parse.
            //Thus we read only the first line, deduce information there and try to move forward.

            //File.ReadLines makes use of lazy evaluation and doesn't read the whole file into an array of lines first.
            //https://stackoverflow.com/questions/27345854/read-only-first-line-from-a-text-file/27345927
            string FirstLine = FSInterface.File.ReadLines(path).First();

            if (options == null) {
                options = new CSVParseOptions();
            }

            string[] fields = ParseLine(FirstLine, options);
            DataTable schema = new DataTable();
            foreach (string field in fields) {
                DataColumn column = new DataColumn(field, typeof(string));
                schema.Columns.Add(column);
            }

            return ParseDefinedCSV(schema, path, options, null);
        }
    }
}
