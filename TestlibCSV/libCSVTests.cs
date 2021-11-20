using libCSV;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

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
        }

        [DataTestMethod]
        [DynamicData(nameof(CSVLinesProvider), DynamicDataSourceType.Method)]
        public void TestLineParsing(string input, string[] expectedFields, CSVParseOptions options) {
            string[] fields = CSVParser.ParseLine(input, options);
            foreach (string s in fields) {
                Console.WriteLine($"Parsed {s}");

            }
            Assert.AreEqual(expectedFields.Length, fields.Length);
            CollectionAssert.AreEqual(expectedFields, fields);
        }
    }
}