//
// Copyright (c) Aris Karagiannidis and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//


using libCSV;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;

namespace TestlibCSV {
    [TestClass]
    public class libCSVTests {

        public static IEnumerable<object[]> CSVLinesProvider() {
            CSVParseOptions options = new CSVParseOptions();


            string line = "Hello,World";
            string[] fields = { "Hello", "World" };
            yield return new object[] { line, fields, options };

            line = "\"Hello\",\"World\"";
            fields = new string[] { "Hello", "World" };
            yield return new object[] { line, fields, options };

            line = "\"He,llo\",\"World\"";
            fields = new string[] { "He,llo", "World" };
            yield return new object[] { line, fields, options };

            line = "\"He,llo\",World";
            fields = new string[] { "He,llo", "World" };
            yield return new object[] { line, fields, options };

            line = "Hello,\"World\"";
            fields = new string[] { "Hello", "World" };
            yield return new object[] { line, fields, options };

            line = "Hello";
            fields = new string[] { "Hello" };
            yield return new object[] { line, fields, options };

            line = "\"Hello\"";
            fields = new string[] { "Hello" };
            yield return new object[] { line, fields, options };

            line = "Hello,";
            fields = new string[] { "Hello" , ""};
            yield return new object[] { line, fields, options };

            line = "Hello,\n";
            fields = new string[] { "Hello" , ""};
            yield return new object[] { line, fields, options };

            line = "\"Hello\",";
            fields = new string[] { "Hello" , ""};
            yield return new object[] { line, fields, options };

            line = "\"Hello\",\n";
            fields = new string[] { "Hello" , ""};
            yield return new object[] { line, fields, options };
        }

        [DataTestMethod]
        [DynamicData(nameof(CSVLinesProvider), DynamicDataSourceType.Method)]
        public void TestLineParsing(string input, string[] expectedFields, CSVParseOptions options) {
            Console.WriteLine($"Input {input}\n");
            string[] fields = CSVParser.ParseLine(input, options);
            foreach (string s in fields) {
                Console.WriteLine($"Parsed {s}");

            }
            Assert.AreEqual(expectedFields.Length, fields.Length);
            CollectionAssert.AreEqual(expectedFields, fields);
        }

        [TestMethod]
        public void TestReadCSVFile() {
            DataTable schema = new DataTable();
            schema.Columns.Add(new DataColumn("FieldA", typeof(string)));
            schema.Columns.Add(new DataColumn("FieldB", typeof(string)));
            schema.Columns.Add(new DataColumn("FieldC", typeof(string)));
            schema.Columns.Add(new DataColumn("FieldD", typeof(string)));
            schema.Columns.Add(new DataColumn("FieldE", typeof(string)));

            DataTable table = CSVParser.ReadDefinedCSV(schema, "TestCSVFiles/Test1.csv");
            string f1 = (string)table.Rows[0][0];
            string f2 = (string)table.Rows[1][2];
            Assert.AreEqual("This", f1);
            Assert.AreEqual("Generated", f2);
        }
    }
}