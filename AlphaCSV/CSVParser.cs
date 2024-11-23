//
// Copyright (c) Aris Karagiannidis and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//
using AlphaCSV.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;

namespace AlphaCSV;
/// <summary>
/// This is the primary class that is used in order to parse a CSV file.
/// </summary>
public class CSVParser : ICSVParser {

    private readonly IFileSystem FSInterface;

    /// <summary>
    /// Constructor with dependency injectio using file system abstractions.
    /// </summary>
    /// <param name="fileSystem"></param>
    public CSVParser(IFileSystem fileSystem) {
        FSInterface = fileSystem;
    }

    /// <summary>
    /// The default constructor
    /// </summary>
    public CSVParser() : this(new FileSystem()) {
    }

    /// <summary>
    /// Parses a CSV file without a given schema.
    /// <remarks>
    ///     This method will assume that all the fields are of string type. And
    ///     will store them as such.
    /// </remarks>
    /// </summary>
    /// <param name="path">The path of the file that will be read</param>
    /// <param name="options">(Optional argument) The options of the file that will be read.</param>
    /// <param name="validationPatterns">(Optional Argument) Validation patterns in terms of regular expressions</param>
    /// <returns>The contents of a CSV file in the form of a datatable</returns>
    public DataTable ParseSimpleCSV(string path, CSVParseOptions options = null, List<Func<string, bool>> validationPatterns = null) {
        //Before we continue we need to make some assumptions for the file. e.g to know how many fields we need to parse.
        //Thus we read only the first line, deduce information there and try to move forward.

        options ??= new CSVParseOptions();

        //File.ReadLines makes use of lazy evaluation and doesn't read the whole file into an array of lines first.
        //https://stackoverflow.com/questions/27345854/read-only-first-line-from-a-text-file/27345927
        string FirstLine = FSInterface.File.ReadLines(path, options.FileEncoding).First();


        string[] fields = ParseLine(FirstLine, options);
        DataTable schema = new();
        foreach (string field in fields) {
            DataColumn column = new(field, typeof(string));
            schema.Columns.Add(column);
        }

        return ParseDefinedCSV(schema, path, options, null);
    }

    /// <summary>
    /// Parses a CSV file with a defined schema.
    /// </summary>
    /// <param name="schema">An empty datatable that defines the schema of the file</param>
    /// <param name="path">The path to the file that will be loaded.</param>
    /// <param name="options">(Optional Parameter)The user configurable options that define the behavior of the parser</param>
    /// <param name="validationPatterns">(Optinal Parameter)A list of regular expressions that the fields of the file will be validated against.
    /// <remarks>This argument will only be used if the relevant flag is enabled in the parse options.</remarks>
    /// </param>
    /// <returns>The contents of the CSV file inside a datatable</returns>
    public DataTable ParseDefinedCSV(DataTable schema, string path, CSVParseOptions options = null, List<Func<string, bool>> validationPatterns = null) {
        //Use the default options if the user does not provide them.
        options ??= new CSVParseOptions();

        //TODO: Do not read the complete file. Instead read one row at a time
        string[] lines = FSInterface.File.ReadAllLines(path, options.FileEncoding);
        DataTable table = schema.Clone();

        if (options.ValidateFields) {
            if (validationPatterns is not null && validationPatterns.Count != schema.Columns.Count) {
                throw GenerateInvalidOpException("The number of validation patterns given do not match the number of columns in the file schema", options);
            } else {
                throw GenerateInvalidOpException("Field validation was requested but no validation patterns were provided.", options);
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
                throw GenerateInvalidOpException($"The number of columns found in the file: {fields.Length} do not match the number of columns declared in the schema {schema.Columns.Count}. Offending row:{globalRow}", options);
            }
            //If we are in the first line
            if (dataRow == 1) {
                if (options.CheckHeaders) {
                    //We need this to access the column name.
                    int index = 0;
                    foreach (string f in fields) {
                        if (!f.Equals(schema.Columns[index].ColumnName, StringComparison.Ordinal)) {
                            throw GenerateInvalidOpException("Column Names do not match. Offending names are " + f + " and " + schema.Columns[index].ColumnName, options);
                        }
                        index++;
                    }
                }
                continue;
            }

            //Before trying to do any operation verify that we have the correct number of fields.
            if (schema.Columns.Count != fields.Length) {
                throw GenerateInvalidOpException($"The number of fields present in the line {fields.Length} do not match the numer of fields defined in the schema {schema.Columns.Count}. Offending row:{globalRow}", options);
            }

            if (options.ValidateFields) {
                //Check that the file is correct
                for (int i = 0; i < validationPatterns.Count; i++) {
                    bool correct = validationPatterns[i](fields[i]);
                    if (!correct) {
                        throw GenerateInvalidOpException($"Could not validate field {i + 1} with contents {fields[i]} did not pass validation. Offending row:{globalRow}", options);
                    }
                }
            }


            DataRow r = table.NewRow();
            for (int i = 0; i < table.Columns.Count; i++) {
                switch (table.Columns[i].DataType.ToString()) {
                    case "System.DateTime":
                        //If there are no options. Try a parsing. If there are options parse the stuff as needed.
                        //TODO: Log this with an ILogger
                        if (string.IsNullOrEmpty(options.DateTimeFormat)) {
                            bool ok = DateTime.TryParse(fields[i], out DateTime theDate);
                            if (ok) {
                                r[i] = theDate;
                            } else {
                                r[i] = DateTime.MinValue;
                            }
                        } else {
                            r[i] = DateTime.ParseExact(fields[i], options.DateTimeFormat, null);
                        }
                        break;
                    case "System.Double":
                    case "System.Decimal":
                    case "System.Single":
                        //TODO: This could be a performance bottleneck since we are potentially
                        //processing thousands of strings.
                        if (options.DecimalSeperator != '.') {
                            fields[i] = fields[i].Replace(options.DecimalSeperator, '.');
                        }
                        r[i] = Convert.ChangeType(fields[i], schema.Columns[i].DataType, CultureInfo.InvariantCulture);
                        break;
                    default:
                        r[i] = Convert.ChangeType(fields[i], schema.Columns[i].DataType, CultureInfo.InvariantCulture);
                        break;
                }
            }
            table.Rows.Add(r);
        }
        return table;
    }


    /// <summary>
    /// Parses a CSV file containing values for the public set properties of the specified type
    /// </summary>
    /// <typeparam name="T">The type that we will receive</typeparam>
    /// <param name="path">The path of the CSV File</param>
    /// <param name="options">CSV Parsing options</param>
    /// <param name="validationPatterns">Validation Patterns</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public List<T> ParseType<T>(string path, CSVParseOptions options = null, List<Func<string, bool>> validationPatterns = null) {
        Type genType = typeof(T);
        ConstructorInfo constructor = genType.GetConstructor(Array.Empty<Type>());
        PropertyInfo[] properties = genType.GetProperties();


        List<Type> propertyTypes = new();
        List<MethodInfo> propertySetMethods = new();
        List<string> propertNames = new();

        List<Tuple<Type, MethodInfo>> comprisingTypes = new();
        foreach (PropertyInfo pi in properties) {
            if (pi.CanWrite) {
                MethodInfo info = pi.GetSetMethod();
                if (info is not null) {
                    propertyTypes.Add(pi.PropertyType);
                    propertySetMethods.Add(pi.GetSetMethod());
                    propertNames.Add(pi.Name);
                }
            }
        }

        for (int i = 0; i < propertNames.Count; i++) {
            Console.WriteLine("Index: {0} Property: {1}", i, propertNames[0]);
        }

        DataTable table = ParseSimpleCSV(path, options, validationPatterns);

        if (table.Columns.Count != propertyTypes.Count) {
            throw new InvalidOperationException($"The number of parsed columns ({table.Columns.Count}) do not match the number of Type properties {propertyTypes.Count}");
        }

        List<T> result = new(table.Rows.Count);
        foreach (DataRow row in table.Rows) {
            object GenericInstance = constructor.Invoke(null);
            for (int i = 0; i < table.Columns.Count; i++) {
                string name = table.Columns[i].ColumnName;
                //We cannot be sure that the CSV field order is correct especially when dealing with derived classes.
                //Thus we need to read the file first and then correlate the column name to the field of the class that we
                //need to instantiate.
                int indexToUse = propertNames.IndexOf(name);
                if (indexToUse is -1) {
                    throw new InvalidOperationException($"There is no property with name \"{name}\" for type {nameof(T)}");
                }
                object covertedValue = Convert.ChangeType(row[i], propertyTypes[indexToUse], CultureInfo.InvariantCulture);
                _ = propertySetMethods[indexToUse].Invoke(GenericInstance, parameters: new object[] { covertedValue });
            }
            result.Add((T)GenericInstance);
        }

        return result;

    }

    /// <summary>
    /// Parses a line and returns all the fields comprising this line.
    /// </summary>
    /// <param name="line">The line to be parsed</param>
    /// <param name="options">The parsing options</param>
    /// <returns>An array containing all the seperate fields that comprise the file.</returns>
    private static string[] ParseLine(string line, CSVParseOptions options) {
        List<string> fields = new();
        //This is quite a simple implementation. We iterrate through each character and assign it to a field.

        bool insideField = false;
        string currentField = "";
        char? previous = null;
        for (int i = 0; i < line.Length; i++) {
            char c = line[i];
            //We hit a quote. This is either the start or the end of the field
            if (c == options.QuoteCharacter) {
                //If we are at the start of the field then the field is either empty or null.
                //This is not a sufficient indicator if we are at the start of the field. Becasuse we might
                //be dealing with an empty quoted field. Note the insideField flag is required only when we
                //use a quote.
                if (string.IsNullOrEmpty(currentField) && previous != options.QuoteCharacter) {
                    insideField = true;
                } else {
                    //If we have a null field and the previous character is the quote character then we can assume that
                    //We have enciuntered a empty quoted field. Thus we change the state back to a "non inside field" state.
                    insideField = false;
                }
                previous = c;
                //The delimeter could be the last character of the line. Check that and add the field to the list
                if (i == line.Length - 1) {
                    fields.Add(currentField);
                    previous = c;
                }
                //continue;
            } else if (c == options.Delimeter) {
                if (insideField) {
                    //If the CSV is quoted. Then the delimeter we encounter here is part of the field value.
                    //Otherwise we need to add the computed field to our list and prepare to calculate the next field.
                    currentField += c;
                } else {
                    fields.Add(currentField);
                    previous = c;
                    currentField = "";
                    //Now we have now reset our state and we are ready to move on.
                    //However if we are at the end of the line this means that the
                    //last field does not have a value thus we need to add an empty string
                    //ourselves.
                    if (i == line.Length - 1 && options.AllowEmptyLastField) {
                        fields.Add(currentField);
                    }
                }
            } else if (c == '\n' | c == '\r') {
                //Ommit the \r character. Do not add in the field.
                if (c == '\n') {
                    //We have reached the end of the line add whatever field we have to the list of fields
                    if (string.IsNullOrEmpty(currentField)) {
                        if (options.AllowEmptyLastField) {
                            fields.Add(currentField);
                            previous = c;
                        }
                    } else {
                        fields.Add(currentField);
                        previous = c;
                    }
                }
            } else {
                currentField += c;
                //This means that we are at the end of the line Though the line is not \n terminated. Add whatever field is computed.
                if (i == line.Length - 1) {
                    fields.Add(currentField);
                    previous = c;
                }
            }
        }
        return fields.ToArray();
    }


    /*
    //TODO: Implement this in the future.
    public static DataTable ReadSchemaFromFile(string filename) {
        throw new NotImplementedException("Not yet implemented");
    }
    */

    /// <summary>
    /// Helper function to create an invalid operation exception and attach data to that exception
    /// </summary>
    /// <param name="message">The message of the Exception</param>
    /// <param name="options">The Parsing options</param>
    /// <param name="validationPatterns">The validation parameters</param>
    /// <returns>An exception ready to the thrown from the program</returns>
    private static InvalidOperationException GenerateInvalidOpException(string message, CSVParseOptions options = null, List<string> validationPatterns = null) {
        InvalidOperationException ex = new(message);
        if (options != null) {
            ex.Data.Add("CSVParserOptions", options);
        }
        if (validationPatterns != null) {
            ex.Data.Add("CSVParserValidationPatterns", validationPatterns);
        }
        return ex;
    }
}//End of CSV Class
 //End of libCSV Namespace