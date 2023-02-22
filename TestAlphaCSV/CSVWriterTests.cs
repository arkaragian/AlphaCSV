//
// Copyright (c) Aris Karagiannidis and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using AlphaCSV;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.IO.Abstractions.TestingHelpers;
using System.Text;

namespace TestAlphaCSV {
    [TestClass]
    public class CSVWriterTests {

        /// <summary>
        /// Retuns a DataTable of strings with the given size.
        /// </summary>
        /// <param name="rows">The number of rows</param>
        /// <param name="columns">The number of columns</param>
        /// <param name="headers">(Optional Argument) custom headers
        ///     <remarks>
        ///         If this is supplied, it's length should be equal
        ///         to the columns parameter.
        ///     </remarks>
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the headers are specified but their
        ///     number is not equal to the number specified in the columns argument
        /// </exception>
        /// <returns></returns>
        public DataTable GetSimpleStringTable(int rows, int columns, string[] headers = null) {
            DataTable table = new DataTable();

            if (headers != null && headers.Length != columns) {
                throw new InvalidOperationException($"The number of headers {headers.Length} does not match the number of columns {columns}");
            } else {
                headers = new string[columns];
                StringBuilder headerBuilder = new StringBuilder();
                for (int i = 0; i < columns; i++) {
                    headerBuilder.Append("Header");
                    headerBuilder.Append(i);
                    headers[i] = headerBuilder.ToString();
                    headerBuilder.Clear();
                }
            }

            foreach (string header in headers) {
                table.Columns.Add(new DataColumn(header, typeof(string)));
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < rows; i++) {
                DataRow r = table.NewRow();
                for (int j = 0; j < headers.Length; j++) {
                    sb.Append("cell");
                    sb.Append(i);
                    sb.Append('.');
                    sb.Append(j);
                    r[j] = sb.ToString();
                }
                table.Rows.Add(r);
                sb.Clear();
            }
            return table;
        }

        /// <summary>
        /// Returns a fixed 2x2 table
        /// <code>
        ///     ColumnA ColumnB
        ///     Hello   World
        ///     Hello2  World2
        /// </code>
        /// </summary>
        /// <returns></returns>
        public DataTable GetSimpleTable() {
            DataTable table = new DataTable();

            table.Columns.Add(new DataColumn("ColumnA", typeof(string)));
            table.Columns.Add(new DataColumn("ColumnB", typeof(string)));

            DataRow r = table.NewRow();
            r[0] = "Hello";
            r[1] = "World";

            table.Rows.Add(r);

            r = table.NewRow();
            r[0] = "Hello2";
            r[1] = "World2";

            table.Rows.Add(r);

            return table;
        }


        /// <summary>
        /// Returns a fixed 1x2 table
        /// <code>
        ///     ColumnA ColumnB
        ///     Hello   World
        /// </code>
        /// </summary>
        public DataTable GetHelloWorldTable() {
            DataTable table = new DataTable();

            table.Columns.Add(new DataColumn("ColumnA", typeof(string)));
            table.Columns.Add(new DataColumn("ColumnB", typeof(string)));

            DataRow r = table.NewRow();
            r[0] = "Hello";
            r[1] = "World";

            table.Rows.Add(r);
            return table;
        }

        [TestMethod]
        public void TestWritenFileExists() {
            //Arrange
            DataTable table = GetSimpleTable();
            MockFileSystem fileSystem = new MockFileSystem();
            CSVWriter writer = new CSVWriter(fileSystem);

            //Act
            writer.WriteCSV("test.csv", table);

            //Assert
            bool exist = fileSystem.File.Exists("test.csv");
            Assert.IsTrue(exist);
        }

        [TestMethod]
        public void TestWrittenNumberOfLines() {
            //Arrange
            DataTable table = GetSimpleTable();

            //Include the header since this is a line
            int totalLines = table.Rows.Count + 1;
            MockFileSystem fileSystem = new MockFileSystem();
            CSVWriter writer = new CSVWriter(fileSystem);

            //Act
            writer.WriteCSV("test.csv", table);

            //Assert
            string[] readLines = fileSystem.File.ReadAllLines("test.csv");
            Assert.AreEqual(totalLines, readLines.Length);
        }

        [TestMethod]
        public void TestWrittenContent() {
            //Arrange
            DataTable table = GetSimpleTable();
            MockFileSystem fileSystem = new MockFileSystem();
            CSVWriter writer = new CSVWriter(fileSystem);

            string line = "ColumnA,ColumnB";
            string line2 = "Hello,World";
            string line3 = "Hello2,World2";
            string[] expectedLines = { line, line2, line3 };

            //Act
            writer.WriteCSV("test.csv", table);

            //Assert
            string[] readLines = fileSystem.File.ReadAllLines("test.csv");
            CollectionAssert.AreEqual(expectedLines, readLines);
        }

        [TestMethod]
        public void TestWrittenContentWithQuotes() {
            //Arrange
            DataTable table = GetSimpleTable();
            MockFileSystem fileSystem = new MockFileSystem();
            CSVWriteOptions options = new CSVWriteOptions();
            options.QuoteFieldsWithoutDelimeter = true;
            CSVWriter writer = new CSVWriter(fileSystem);

            string line = "\"ColumnA\",\"ColumnB\"";
            string line2 = "\"Hello\",\"World\"";
            string line3 = "\"Hello2\",\"World2\"";
            string[] expectedLines = { line, line2, line3 };

            //Act
            writer.WriteCSV("test.csv", table,options);

            //Assert
            string[] readLines = fileSystem.File.ReadAllLines("test.csv");
            CollectionAssert.AreEqual(expectedLines, readLines);
        }

        [TestMethod]
        public void TestNumberOfBytes() {
            //Arrange
            DataTable table = GetSimpleTable();
            MockFileSystem fileSystem = new MockFileSystem();
            CSVWriter writer = new CSVWriter(fileSystem);

            //Default options will also write a CR and LF. This is two additional Bytes.
            string line = "ColumnA,ColumnB"; //17
            string line2 = "Hello,World"; //13
            string line3 = "Hello2,World2"; //15
            string[] expectedLines = { line, line2, line3 };

            //Act
            writer.WriteCSV("test.csv", table);

            //Assert
            byte[] readBytes = fileSystem.File.ReadAllBytes("test.csv");
            Assert.AreEqual(45, readBytes.Length);
        }

        [TestMethod]
        public void WriteCSV_WriteQuotedFieldThatContainsQuotes_ShouldEscapeTheQuote() {
            //Arrange

            DataTable table = new DataTable();

            table.Columns.Add(new DataColumn("ColumnA", typeof(string)));
            table.Columns.Add(new DataColumn("ColumnB", typeof(string)));

            DataRow r = table.NewRow();
            r[0] = "Hello";
            r[1] = "W\"orld";

            table.Rows.Add(r);

            r = table.NewRow();
            r[0] = "Hello2";
            r[1] = "World2";

            table.Rows.Add(r);
            MockFileSystem fileSystem = new MockFileSystem();
            CSVWriter writer = new CSVWriter(fileSystem);

            //Default options will also write a CR and LF. This is two additional Bytes.
            string line = "\"ColumnA\",\"ColumnB\""; //17
            string line2 = "\"Hello\",\"W\"\"orld\""; //13
            string line3 = "\"Hello2\",\"World2\""; //15
            string[] expectedLines = { line, line2, line3 };

            CSVWriteOptions opts =  new CSVWriteOptions();
            opts.QuoteFieldsWithoutDelimeter = true;

            //Act
            writer.WriteCSV("test.csv", table, opts);


            string[] readLines = fileSystem.File.ReadAllLines("test.csv");
            //for(int i=0; i<readLines.Length; i++){
            //    Console.WriteLine("Comparing {0} with {1}",readLines[i],expectedLines[i]);
            //}
            CollectionAssert.AreEqual(expectedLines, readLines);
        }

        [TestMethod]
        public void WriteCSV_WriteFieldThatContainsBothDelimeterAndQuotes_ShouldEscapeTheQuote() {
            //Arrange

            DataTable table = new DataTable();

            table.Columns.Add(new DataColumn("ColumnA", typeof(string)));
            table.Columns.Add(new DataColumn("ColumnB", typeof(string)));

            DataRow r = table.NewRow();
            r[0] = "Hello";
            r[1] = "W\"o,rld";

            table.Rows.Add(r);

            r = table.NewRow();
            r[0] = "Hello2";
            r[1] = "World2";

            table.Rows.Add(r);
            MockFileSystem fileSystem = new MockFileSystem();
            CSVWriter writer = new CSVWriter(fileSystem);

            //Default options will also write a CR and LF. This is two additional Bytes.
            string line = "ColumnA,ColumnB"; //17
            string line2 = "Hello,\"W\"\"o,rld\""; //13
            string line3 = "Hello2,World2"; //15
            string[] expectedLines = { line, line2, line3 };


            //Act
            writer.WriteCSV("test.csv", table);


            string[] readLines = fileSystem.File.ReadAllLines("test.csv");
            //for(int i=0; i<readLines.Length; i++){
            //    Console.WriteLine("Comparing {0} with {1}",readLines[i],expectedLines[i]);
            //}
            CollectionAssert.AreEqual(expectedLines, readLines);
        }


    }
}
