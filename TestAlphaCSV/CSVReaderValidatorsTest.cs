using AlphaCSV;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAlphaCSV {
    [TestClass]
    public class CSVReaderValidatorsTest {

        private DataTable expectedData {
            get {
                DataTable table = new DataTable();
                table.Columns.Add("ColumnString",typeof(string));
                table.Columns.Add("ColumnInt",typeof(int));
                table.Columns.Add("ColumnDate",typeof(DateTime));

                DataRow r = table.NewRow();
                r[0] = "Hello";
                r[1] = 1;
                r[2] = new DateTime(2022,2,15);

                table.Rows.Add(r);
                return table;
            }
        }

        private string fileData {
            get {
                string data = "ColumnString,ColumnInt,ColumnDate\nHello,1,15-Feb-2022";
                return data;
            }
        }

        private string BadfileData {
            get {
                string data = "ColumnString,ColumnInt,ColumnDate\nHello,2,15-Feb-2022";
                return data;
            }
        }

        bool StringValidator(string input) {
            return true;
        }

        bool IntValidator(string input) {
            return input.Contains('1');
        }

        bool DateValidator(string input) {
            return input.StartsWith("15");
        }



        [TestMethod]
        public void ParseWithValidators() {
            MockFileSystem mockfs = new MockFileSystem();
            Func<string, bool> Validatora = StringValidator;
            Func<string, bool> Validatorb = IntValidator;
            Func<string, bool> Validatorc = DateValidator;
            List<Func<string, bool>> validators = new List<Func<string, bool>>() {
                Validatora,
                Validatorb,
                Validatorc
            };
            mockfs.AddFile("test.csv", new MockFileData(fileData));
            CSVParser parser = new CSVParser(mockfs);
            CSVParseOptions options = new CSVParseOptions();
            options.ValidateFields = true;
            DataTable result = parser.ParseDefinedCSV(expectedData.Clone(),"test.csv",options,validators);
            AssertDataTable.AreEqual(expectedData,result);
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ParseWithValidatorsExpectError() {
            MockFileSystem mockfs = new MockFileSystem();
            Func<string, bool> Validatora = StringValidator;
            Func<string, bool> Validatorb = IntValidator;
            Func<string, bool> Validatorc = DateValidator;
            List<Func<string, bool>> validators = new List<Func<string, bool>>() {
                Validatora,
                Validatorb,
                Validatorc
            };
            mockfs.AddFile("test.csv", new MockFileData(BadfileData));
            CSVParser parser = new CSVParser(mockfs);
            CSVParseOptions options = new CSVParseOptions();
            options.ValidateFields = true;
            parser.ParseDefinedCSV(expectedData.Clone(),"test.csv",options,validators);
        }
    }
}
