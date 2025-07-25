//
// Copyright (c) Aris Karagiannidis and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//


using AlphaCSV;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Abstractions.TestingHelpers;
using System.Text;

namespace TestAlphaCSV {
    /// <summary>
    /// This class performs basic testing of the functionality of the parser.
    /// To do this we have implemented a hierarchy of IEnumerable methods
    /// whose combinations generate a multitude of inputs towards the test methods.
    /// </summary>
    [TestClass]
    public partial class CSVReaderTests {

        #region Termination Definitions
        /// <summary>
        /// This enum is passed for information reasons only it is not supposed to be used in any logic.
        /// This is because we cannot print non printing characters.
        /// </summary>
        public enum TerminationType {
            None,
            Newline,
            CRLF
        }

        /// <summary>
        /// A look up table that helps us print information.
        /// </summary>
        private static Dictionary<string, TerminationType> terminationLUT = new Dictionary<string, TerminationType> {
            {"\n", TerminationType.Newline},
            {"\r\n", TerminationType.CRLF},
        };

        /// <summary>
        /// An IEnumerable method that returns all the ways that a Header line can be terminated.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> HeaderTermination() {
            yield return "\n";
            yield return "\r\n";
        }
        /// <summary>
        /// An IEnumerable method that returns all the ways that a record line can be terminated.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string?> LineTermination() {
            yield return null;
            yield return "\n";
            yield return "\r\n";
        }
        #endregion

        #region Field Line Definitions
        /// <summary>
        /// An IEnumerable that defines all the ways that simple record can be defined.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> SimpleFields() {
            CSVParseOptions options = new CSVParseOptions();

            string parserInput;
            string[] expectedParseResult;

            parserInput = "Hello";
            expectedParseResult = new string[] { "Hello" };
            yield return new object[] { parserInput, expectedParseResult, options };

            parserInput = "Hello,";
            expectedParseResult = new string[] { "Hello", "" };
            yield return new object[] { parserInput, expectedParseResult, options };

            parserInput = "Hello,World";
            expectedParseResult = new string[] { "Hello", "World" };
            yield return new object[] { parserInput, expectedParseResult, options };


            parserInput = "Hello,,";
            expectedParseResult = new string[] { "Hello", "", "" };
            yield return new object[] { parserInput, expectedParseResult, options };


            parserInput = "Hello,,World";
            expectedParseResult = new string[] { "Hello", "", "World" };
            yield return new object[] { parserInput, expectedParseResult, options };

        }

        /// <summary>
        /// An IEnumerable that defines all the ways that quoted record can be defined.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> QuotedFields() {
            CSVParseOptions options = new CSVParseOptions();

            string parserInput;
            string[] expectedParseResult;

            parserInput = "\"Hello\"";
            expectedParseResult = new string[] { "Hello" };
            yield return new object[] { parserInput, expectedParseResult, options };

            parserInput = "\"Hello\",";
            expectedParseResult = new string[] { "Hello", "" };
            yield return new object[] { parserInput, expectedParseResult, options };

            parserInput = "\"Hello\",\"\"";
            expectedParseResult = new string[] { "Hello", "" };
            yield return new object[] { parserInput, expectedParseResult, options };

            parserInput = "\"Hello\",\"World\"";
            expectedParseResult = new string[] { "Hello", "World" };
            yield return new object[] { parserInput, expectedParseResult, options };

            parserInput = "\"He,llo\",\"World\"";
            expectedParseResult = new string[] { "He,llo", "World" };
            yield return new object[] { parserInput, expectedParseResult, options };

            parserInput = "\"Hello\",\"\",\"404001286301\"";
            expectedParseResult = new string[] { "Hello", "", "404001286301" };
            yield return new object[] { parserInput, expectedParseResult, options };

            parserInput = "\"Hello\",\"\",\"\"";
            expectedParseResult = new string[] { "Hello", "", "" };
            yield return new object[] { parserInput, expectedParseResult, options };

        }

        /// <summary>
        /// An IEnumerable that defines how mixed simple and quoted records can be defined.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> QuotedSimpleFieldMix() {
            CSVParseOptions options = new CSVParseOptions();

            string line;
            string[] fields;

            line = "\"He,llo\",World";
            fields = new string[] { "He,llo", "World" };
            yield return new object[] { line, fields, options };

            line = "Hello,\"World\"";
            fields = new string[] { "Hello", "World" };
            yield return new object[] { line, fields, options };

            line = "Hello,\"Wo,rld\"";
            fields = new string[] { "Hello", "Wo,rld" };
            yield return new object[] { line, fields, options };
        }

        public static IEnumerable<object[]> MixedTypeFields() {
            CSVParseOptions options = new CSVParseOptions();

            string line;
            object[] expectedFields;

            line = "Hello,1156";
            expectedFields = new object[] { "Hello", 1156 };
            yield return new object[] { line, expectedFields, options };

            line = "1156,Hello";
            expectedFields = new object[] { 1156, "Hello" };
            yield return new object[] { line, expectedFields, options };

            line = "1156,Hello,1.45";
            expectedFields = new object[] { 1156, "Hello", 1.45 };
            yield return new object[] { line, expectedFields, options };

            options.CommonOptions.DecimalSeperator = ',';
            line = "1156,Hello,\"1,45\"";
            expectedFields = new object[] { 1156, "Hello", 1.45 };
            yield return new object[] { line, expectedFields, options };

            line = "1156,Hello,\"1,45\"";
            expectedFields = new object[] { 1156, "Hello", 1.45m }; //With Decimal
            yield return new object[] { line, expectedFields, options };

            line = "1156,Hello,\"1,45\"";
            expectedFields = new object[] { 1156, "Hello", 1.45f }; //With float
            yield return new object[] { line, expectedFields, options };
        }
        #endregion

        #region Input Generators
        /*The input generators bellow make use of the IEnumerator defined above to generate all the required
         * Combinations. Those in turn are yield returned to the tests.
         */

        /// <summary>
        /// An IEnumerable method that creates all the possible input record combinations for the records described in the Mehtod parameter. It returns
        ///     <list type="number">
        ///         <item>
        ///             The contents of the CSV file
        ///         </item>
        ///         <item>
        ///             The expected datatable to test the assertion
        ///         </item>
        ///         <item>
        ///             The CSVParse options
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="Method">The method that returns the required fields</param>
        /// <returns></returns>
        private static IEnumerable<object[]> InputGenerator(IEnumerable<object[]> Method) {

            //Create a template for generic columns that will be used to generate the datatable
            Dictionary<int, string> headerDictionary = new Dictionary<int, string>() {
                {1,"ColumnA" },
                {2,"ColumnA,ColumnB" },
                {3,"ColumnA,ColumnB,ColumnC" },
                {4,"ColumnA,ColumnB,ColumnC,ColumnD" },
            };

            Dictionary<int, string[]> ExpectedHeaderFields = new Dictionary<int, string[]>() {
                {1,new string[]{"ColumnA"}},
                {2,new string[]{"ColumnA", "ColumnB"}},
                {3,new string[]{"ColumnA", "ColumnB","ColumnC"}},
                {4,new string[]{"ColumnA", "ColumnB","ColumnC","ColumnD"}},
            };

            StringBuilder fileBuilder = new StringBuilder();
            foreach (object[] data in Method) {
                foreach (string headerTermination in HeaderTermination()) {
                    foreach (string? termination in LineTermination()) {
                        fileBuilder.Clear();
                        int numberOfFields = ((object[])data[1]).Length; //Add the header
                        //For number of expected fields. Build a CSV input file.
                        fileBuilder.Append(headerDictionary[numberOfFields]); //Add the header
                        fileBuilder.Append(headerTermination);
                        fileBuilder.Append((string)data[0]); //Add the fields line
                        if (termination is not null) {
                            fileBuilder.Append(termination); //Add line termination
                        }

                        DataTable expectedResult = new DataTable();
                        string[] headers = ExpectedHeaderFields[numberOfFields];
                        for (int i = 0; i < numberOfFields; i++) {
                            expectedResult.Columns.Add(headers[i], ((object[])data[1])[i].GetType());
                        }

                        DataRow r = expectedResult.NewRow();
                        for (int i = 0; i < numberOfFields; i++) {
                            r[i] = ((object[])data[1])[i];
                        }

                        expectedResult.Rows.Add(r);

                        TerminationType headTerm = terminationLUT[headerTermination];
                        TerminationType lineTerm;
                        if (termination is null) {
                            lineTerm = TerminationType.None;
                        } else {
                            lineTerm = terminationLUT[termination];
                        }

                        //We return the file input. The expected headers, the expected fields, and the parse options
                        yield return new object[] { fileBuilder.ToString(), expectedResult, data[2], headTerm, lineTerm };
                    }
                }
            }
        }


        public static IEnumerable<object[]> SimpleInputGenerator() {
            IEnumerable<object[]> tests = InputGenerator(SimpleFields());
            foreach (object[] testCase in tests) {
                yield return testCase;
            }
        }

        public static IEnumerable<object[]> QuotedInputGenerator() {
            IEnumerable<object[]> tests = InputGenerator(QuotedFields());
            foreach (object[] testCase in tests) {
                yield return testCase;
            }
        }

        public static IEnumerable<object[]> QuotedSimpleMixInputGenerator() {
            IEnumerable<object[]> tests = InputGenerator(QuotedSimpleFieldMix());
            foreach (object[] testCase in tests) {
                yield return testCase;
            }
        }

        public static IEnumerable<object[]> MixedStringIntegersInputGenerator() {
            IEnumerable<object[]> tests = InputGenerator(MixedTypeFields());
            foreach (object[] testCase in tests) {
                yield return testCase;
            }
        }
        #endregion



        [DataTestMethod]
        [DynamicData(nameof(SimpleInputGenerator), DynamicDataSourceType.Method)]
        [DynamicData(nameof(QuotedInputGenerator), DynamicDataSourceType.Method)]
        [DynamicData(nameof(QuotedSimpleMixInputGenerator), DynamicDataSourceType.Method)]
        public void TestSimpleParse(string input, DataTable expectedResult, CSVParseOptions options, TerminationType headerTermination, TerminationType lineTermination) {
            //Arrange
            Console.WriteLine($"Input:\n{input}\n");
            Console.WriteLine($"Header termination: {headerTermination}, Record Termination: {lineTermination}");
            MockFileSystem fs = new MockFileSystem();
            MockFileData mockInputFile = new MockFileData(input);
            string path = @"C:\test.csv";
            fs.AddFile(path, mockInputFile);


            //Act
            CSVParser parser = new CSVParser(fs); //Inject dependency here
            DataTable table = parser.ParseSimpleCSV(path, options);

            //Assert
            AssertDataTable.AreEqual(expectedResult, table);
        }

        [DataTestMethod]
        [DynamicData(nameof(SimpleInputGenerator), DynamicDataSourceType.Method)]
        [DynamicData(nameof(QuotedInputGenerator), DynamicDataSourceType.Method)]
        [DynamicData(nameof(QuotedSimpleMixInputGenerator), DynamicDataSourceType.Method)]
        [DynamicData(nameof(MixedStringIntegersInputGenerator), DynamicDataSourceType.Method)]
        public void TestDefinedParse(string input, DataTable expectedResult, CSVParseOptions options, TerminationType headerTermination, TerminationType lineTermination) {
            //Arrange
            Console.WriteLine($"Input:\n{input}\n");
            Console.WriteLine($"Header termination: {headerTermination}, Record Termination: {lineTermination}");

            Console.WriteLine("Schema Definition:");
            foreach (DataColumn c in expectedResult.Columns) {
                Console.Write($"{c.DataType}\t");
            }
            Console.Write("\n");

            MockFileSystem fs = new MockFileSystem();
            MockFileData mockInputFile = new MockFileData(input);
            string path = @"C:\test.csv";
            fs.AddFile(path, mockInputFile);



            //Act
            CSVParser parser = new CSVParser(fs); //Inject dependency here
            //Since we have the expected result we just clone the schema instead of building it by hand.
            DataTable table = parser.ParseDefinedCSV(expectedResult.Clone(), path, options);

            //Assert
            AssertDataTable.AreEqual(expectedResult, table);
        }


#if false
        //This is a single debug method aiming to provide a scrapbook in order to debug a test.
        [TestMethod]
        public void DebugSingleCase() {
            //Arrange
            MockFileSystem fs = new MockFileSystem();
            MockFileData mockInputFile = new MockFileData("ColumnA,ColumnB,ColumnC\n1156,Hello,\"1,45\"");
            string path = @"C:\test.csv";
            fs.AddFile(path, mockInputFile);

            DataTable expectedResult = new DataTable();
            expectedResult.Columns.Add(new DataColumn("ColumnA", typeof(int)));
            expectedResult.Columns.Add(new DataColumn("ColumnB", typeof(string)));
            expectedResult.Columns.Add(new DataColumn("ColumnC", typeof(double)));

            DataRow r = expectedResult.NewRow();
            r[0] = (int)1156;
            r[1] = "Hello";
            r[2] = 1.45;

            expectedResult.Rows.Add(r);

            //Act
            CSVParser parser = new CSVParser(fs); //Inject dependency here
            //Since we have the expected result we just clone the schema instead of building it by hand.
            CSVParseOptions options = new CSVParseOptions {
                DecimalSeperator = ','
            };
            DataTable table = parser.ParseDefinedCSV(expectedResult.Clone(), path, options);

            //Assert
            AssertDataTable.AreEqual(expectedResult, table);
        }
#endif

    }
}