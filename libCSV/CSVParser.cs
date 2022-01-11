//
// Copyright (c) Aris Karagiannidis and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//


using System.Data;
using System.Text.RegularExpressions;

namespace libCSV {
    public static class CSVParser {
        /// <summary>
        /// Reads a CSV file with a defined schema.
        /// </summary>
        /// <param name="schema">An empty datatable that defines the schema of the file</param>
        /// <param name="path">The path to the file that will be loaded.</param>
        /// <param name="options">The user configurable options that define the behavior of the parser</param>
        /// <param name="validationPatterns">(Optinal Parameter)A list of regular expressions that the fields of the file will be validated against.
        /// <remarks>This argument will only be used if the relevant flag is enabled in the parse options.</remarks>
        /// </param>
        /// <returns></returns>
        public static DataTable ReadDefinedCSV(DataTable schema, string path, CSVParseOptions options = null, List<string> validationPatterns = null) {
            //Use the default options if the user does not provide them.
            if (options == null) {
                options = new CSVParseOptions();
            }


            //TODO: Perform a check for the file size. Maybe warn the user that there might be memory issues. 
            string[] lines = System.IO.File.ReadAllLines(path);
            DataTable table = schema.Clone();

            if (options.ValidateFields) {
                if (validationPatterns != null) {
                    if (validationPatterns.Count != schema.Columns.Count) {
                        throw GenerateInvalidOpEx("The number of validation patterns given do not match the number of columns in the file schema", options, validationPatterns);
                    }
                } else {
                    throw GenerateInvalidOpEx("Field validation was requested but no validation patterns were provided.", options, validationPatterns);
                }
            }
            //Keep track of the current row for informational purposes.
            int globalRow = 0;
            //This variable keeps track of the rows that we are processing. e.g if a file has 4 rows and the first is a comment row then the data row will start
            //incrementing after comment row.
            int dataRow = 0;
            foreach (string line in lines) {
                globalRow++;
                //This is a comment line. No need to deal with it. Move on.
                if (line[0] == options.CommentCharacter) {
                    continue;
                }
                dataRow++;
                //Split our fields with the delimeter.
                string[] fields = ParseLine(line, options);
                if (fields.Length != schema.Columns.Count) {
                    throw GenerateInvalidOpEx($"The number of columns found in the file: {fields.Length} do not match the number of columns declared in the schema {schema.Columns.Count}. Offending row:{globalRow}", options, validationPatterns);
                }
                //If we are in the first line
                if (dataRow == 1) {
                    if (options.CheckHeaders) {
                        //We need this to access the column name.
                        int index = 0;
                        foreach (string f in fields) {
                            if (!f.Equals(schema.Columns[index].ColumnName)) {
                                throw GenerateInvalidOpEx("Column Names do not match. Offending names are " + f + " and " + schema.Columns[index].ColumnName, options, validationPatterns);
                            }
                            index++;
                        }
                    }
                    continue;
                }

                //Before trying to do any operation verify that we have the correct number of fields.
                if (schema.Columns.Count != fields.Length) {
                    throw GenerateInvalidOpEx($"The number of fields present in the line {fields.Length} do not match the numer of fields defined in the schema {schema.Columns.Count}. Offending row:{globalRow}", options, validationPatterns);
                }

                if (options.ValidateFields) {
                    //Check that the file is correct
                    for (int i = 0; i < validationPatterns.Count; i++) {
                        string pattern = validationPatterns[i];
                        Regex rg = new Regex(pattern);
                        Match match = rg.Match(fields[i]);
                        if (!match.Success) {
                            throw GenerateInvalidOpEx($"Could not match the {pattern} with the field {i + 1} with field contents {fields[i]}. Offending row:{globalRow}", options, validationPatterns);
                        }
                    }
                }


                DataRow r = table.NewRow();
                for (int i = 0; i < table.Columns.Count; i++) {
                    if (schema.Columns[i].DataType == typeof(DateTime)) {
                        //If there are no options. Try a parsing. If there are options parse the stuff as needed.
                        //TODO: Log this with an ILogger
                        if (string.IsNullOrEmpty(options.DateTimeFormat)) {
                            DateTime theDate;
                            DateTime.TryParse(fields[i], out theDate);
                            r[i] = theDate;
                        } else {
                            r[i] = DateTime.ParseExact(fields[i], options.DateTimeFormat, null);
                        }
                    } else {
                        r[i] = Convert.ChangeType(fields[i], schema.Columns[i].DataType);
                    }

                }

                table.Rows.Add(r);
            }
            return table;
        }

        /// <summary>
        /// Parses a line and returns all the fields comprising this line.
        /// </summary>
        /// <param name="line">The line to be parsed</param>
        /// <param name="options">The parsing options</param>
        /// <returns>An array containing all the seperate fields that comprise the file.</returns>
        public static string[] ParseLine(string line, CSVParseOptions options) {
            List<string> fields = new List<string>();
            //This is quite a simple implementation. We iterrate through each character and assign it to a field.

            bool insideField = false;
            string currentField = "";
            for (int i = 0; i < line.Length; i++) {
                char c = line[i];
                //We hit a quote. This is either the start or the end of the field
                if (c == options.QuoteCharacter) {
                    //If we are the start of the field then the field is either empty or null.
                    //This is a sufficient indicator if we are at the start of the field.
                    //Note the insideField flag is required only when we use a quote.
                    if (string.IsNullOrEmpty(currentField)) {
                        insideField = true;
                    } else {
                        insideField = false;
                    }
                    //The delimeter could be the last character of the line. Check that and add the field to the list
                    if (i == line.Length - 1) {
                        fields.Add(currentField);
                    }
                    //continue;
                } else if (c == options.Delimeter) {
                    if (insideField) {
                        //If the CSV is quoted. Then the delimeter we encounter here is part of the field value.
                        //Otherwise we need to add the computed field to our list and prepare to calculate the next field.
                        currentField += c;
                    } else {
                        fields.Add(currentField);
                        currentField = "";
                        //Now we have now reset our state and we are ready to move on.
                        //However if we are at the end of the line this means that the
                        //last field does not have a value thus we need to add an empty string
                        //ourselves.
                        if (i == line.Length - 1) {
                            fields.Add(currentField);
                        }
                    }
                } else if (c == '\n') {
                    //We have reached the end of the line add whatever field we have to the list of fields
                    fields.Add(currentField);
                } else {
                    currentField += c;
                    //This means that we are at the end of the line Though the line is not \n terminated. Add whatever field is computed.
                    if (i == line.Length - 1) {
                        fields.Add(currentField);
                    }
                }
            }
            return fields.ToArray();
        }


        public static DataTable ReadSchemaFromFile(string filename) {
            throw new NotImplementedException("Not yet implemented");
        }

        /// <summary>
        /// Helper function to attach data in a new InvalidOperationException
        /// </summary>
        /// <param name="message">The message of the Exception</param>
        /// <param name="options">The Parsing options</param>
        /// <param name="validationPatterns">The validation parameters</param>
        /// <returns>An exception ready to the thrown from the program</returns>
        private static InvalidOperationException GenerateInvalidOpEx(string message, CSVParseOptions options = null, List<string> validationPatterns = null) {
            InvalidOperationException ex = new InvalidOperationException(message);
            if (options != null) {
                ex.Data.Add("CSVParserOptions", options);
            }
            if (validationPatterns != null) {
                ex.Data.Add("CSVParserValidationPatterns", validationPatterns);
            }
            return ex;
        }
    }//End of CSV Class
}//End of libCSV Namespace