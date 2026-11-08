using AlphaCSV;
using Xunit;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Abstractions.TestingHelpers;

namespace TestAlphaCSV {
    public class CSVReaderValidatorsTest {
#pragma warning disable CA1707

        private readonly ITestOutputHelper _output;

        public CSVReaderValidatorsTest(ITestOutputHelper output) {
            _output = output;
        }

        private DataTable expectedData {
            get {
                DataTable table = new();
                table.Columns.Add("ColumnString", typeof(string));
                table.Columns.Add("ColumnInt", typeof(int));
                table.Columns.Add("ColumnDate", typeof(DateTime));

                DataRow r = table.NewRow();
                r[0] = "Hello";
                r[1] = 1;
                r[2] = new DateTime(2022, 2, 15);

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

        bool StringValidatorAlwaysReturnTrue(string input) {
            return true;
        }

        bool IntValidatorCheckIfStringContainsOne(string input) {
            return input.Contains('1');
        }

        bool DateValidatorCheckIfStringStartsWithFifteen(string input) {
            return input.StartsWith("15");
        }



        [Fact]
        public void ParseDefinedCSV_ValidFieldsWithValidators_ReturnsExpectedTable() {
            MockFileSystem mockfs = new();
            Func<string, bool> Validatora = StringValidatorAlwaysReturnTrue;
            Func<string, bool> Validatorb = IntValidatorCheckIfStringContainsOne;
            Func<string, bool> Validatorc = DateValidatorCheckIfStringStartsWithFifteen;
            List<Func<string, bool>> validators = [
                Validatora,
                Validatorb,
                Validatorc
            ];
            mockfs.AddFile("test.csv", new MockFileData(fileData));
            CSVParser parser = new(mockfs);
            CSVParseOptions options = new() {
                ValidateFields = true
            };
            DataTable result = parser.ParseDefinedCSV(expectedData.Clone(), "test.csv", options, validators);
            AssertDataTable.AreEqual(expectedData, result);
        }


        [Fact]
        public void ParseDefinedCSV_FieldFailsValidation_ThrowsInvalidOperationException() {
            MockFileSystem mockfs = new();
            Func<string, bool> Validatora = StringValidatorAlwaysReturnTrue;
            Func<string, bool> Validatorb = IntValidatorCheckIfStringContainsOne;
            Func<string, bool> Validatorc = DateValidatorCheckIfStringStartsWithFifteen;
            List<Func<string, bool>> validators = [
                Validatora,
                Validatorb,
                Validatorc
            ];
            mockfs.AddFile("test.csv", new MockFileData(BadfileData));
            CSVParser parser = new(mockfs);
            CSVParseOptions options = new() {
                ValidateFields = true
            };

            _ = Assert.Throws<InvalidOperationException>(() => {
                parser.ParseDefinedCSV(expectedData.Clone(), "test.csv", options, validators);
            });
        }
#pragma warning restore CA1707
    }
}