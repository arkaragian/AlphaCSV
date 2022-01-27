//
// Copyright (c) Aris Karagiannidis and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//


using libCSV;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Abstractions.TestingHelpers;
using System.Text;

namespace TestlibCSV {
    [TestClass]
    public class SimpleCSVFileTests {

        enum CSVFieldStyle {
            Simple,
            LF,
            CRLF,

        }


        public static IEnumerable<object[]> SimpleCSV() {
            CSVParseOptions options = new CSVParseOptions();

            //The line comprises of three modes.
            //Simple Field delimitation
            //Simple filed delimitation with newline
            //Simple filed delimitation with CR and Newline

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>{
                { @"c:\a.csv", new MockFileData("Testing is meh.") },
                { @"c:\b.csv", new MockFileData("some js") },
             });

            string header;
            string line;
            string[] HeaderFields;
            string[] LineFields;

            header = "colA";
            line = "Hello";
            HeaderFields = new string[] { "colA" };
            LineFields = new string[] { "Hello" };
            yield return new object[] { header, HeaderFields, line, LineFields, options };
        }


        public static List<MockFileData> BuildSimpleCSVs() {
            List<string> SingleColumnHeaders = new List<string> {
                "colA\n",
                "colA\r\n",
            };

            List<string> SingleColumnLineData = new List<string> {
                "Hello",
                "Hello\n",
                "Hello\r\n",
            };


            List<string> TwoColumnHeaders = new List<string> {
                "colA,colB\n",
                "colA,colB\r\n",
            };

            List<string> TwoColumnLineData = new List<string> {
                "Hello,",
                "Hello,\n",
                "Hello,\r\n",

                "Hello,World",
                "Hello,World\n",
                "Hello,World\r\n"
            };

            List<MockFileData> mockData = new List<MockFileData>();

            foreach (string header in SingleColumnHeaders) {
                StringBuilder sb = new StringBuilder();
                sb.Append(header);
                foreach (string line in SingleColumnLineData) {
                    sb.Append(line);
                }
                mockData.Add(new MockFileData(sb.ToString()));
            }

            foreach (string header in TwoColumnHeaders) {
                StringBuilder sb = new StringBuilder();
                sb.Append(header);
                foreach (string line in TwoColumnLineData) {
                    sb.Append(line);
                }
                mockData.Add(new MockFileData(sb.ToString()));
            }

            return mockData;
        }

        public static List<DataTable> GetSimpleDataTable() {
            DataTable Table = new DataTable {
                Columns = { "colA" },
                Rows = { { "Hello" } }
            };

            DataTable Table2 = new DataTable {
                Columns = { "colA", "colB" },
                Rows = { { "Hello", "World" } }
            };
            return new List<DataTable> { Table, Table2 };
        }

        //[DataTestMethod]
        //[DynamicData(nameof(SimpleCSV), DynamicDataSourceType.Method)]
        [TestMethod]
        public void TestLineParsing(List<DataTable> expectedTables, CSVParseOptions options) {
            MockFileSystem fs = new MockFileSystem();
            List<MockFileData> data = BuildSimpleCSVs();
            List<DataTable> tables = GetSimpleDataTable();
            //{ @"c:\a.csv", new MockFileData("Testing is meh.") },
            for (int k = 0; k < data.Count; k++) {
                string filename = $"c:\\{k}.csv";
                fs.AddFile(filename, data[k]);
            }

            CSVParser parser = new CSVParser(fs);

            int i = 0;
            foreach (string file in fs.AllFiles) {
                DataTable table = parser.ParseSimpleCSV(file, options);
                Assert.AreEqual(expectedTables[i], table);
            }
        }
    }
}