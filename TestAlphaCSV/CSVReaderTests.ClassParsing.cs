using AlphaCSV;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAlphaCSV {
    public partial class CSVReaderTests {

        public record TestRecord {
            public required string Name { get; set; }
            public required string Surname { get; set; }
        }

        public record DerivedRecord : TestRecord {
            public required string FatherName { get; set; }
        }

        public sealed record CustomerPartCrossReference {
            public required string PartNumber { get; init; }
            public required CustomerPartReference CustomerReference { get; init; }
        }

        public sealed record CustomerPartReference {
            public required string CustomerID { get; init; }
            public required string CustomerPartNumber { get; init; }
            public string? CustomerPartDescription { get; init; }
        }



        [TestMethod]
        public void TestSimpleClassParsing() {
            string input = "Name,Surname\nJohn,Doe\n";

            //Arrange
            MockFileSystem fs = new MockFileSystem();
            MockFileData mockInputFile = new MockFileData(input);
            string path = @"C:\test.csv";
            fs.AddFile(path, mockInputFile);

            TestRecord rc = new TestRecord {
                Name = "John",
                Surname = "Doe"
            };
            List<TestRecord> expected = new List<TestRecord> { rc };


            //Act
            CSVParser parser = new CSVParser(fs); //Inject dependency here
            //Since we have the expected result we just clone the schema instead of building it by hand.
            List<TestRecord> actual = parser.ParseType<TestRecord>(path);


            //Assert
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSimpleClassParsingReverseColumnOrder() {
            string input = "Surname,Name\nDoe,John\n";

            //Arrange
            MockFileSystem fs = new MockFileSystem();
            MockFileData mockInputFile = new MockFileData(input);
            string path = @"C:\test.csv";
            fs.AddFile(path, mockInputFile);

            TestRecord rc = new TestRecord {
                Name = "John",
                Surname = "Doe"
            };
            List<TestRecord> expected = new List<TestRecord> { rc };


            //Act
            CSVParser parser = new CSVParser(fs); //Inject dependency here
            //Since we have the expected result we just clone the schema instead of building it by hand.
            List<TestRecord> actual = parser.ParseType<TestRecord>(path);


            //Assert
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestDerivedClass() {
            string input = "Name,Surname,FatherName\nJohn,Doe,Donald\n";

            //Arrange
            MockFileSystem fs = new MockFileSystem();
            MockFileData mockInputFile = new MockFileData(input);
            string path = @"C:\test.csv";
            fs.AddFile(path, mockInputFile);

            DerivedRecord rc = new DerivedRecord {
                Name = "John",
                Surname = "Doe",
                FatherName = "Donald"
            };
            List<DerivedRecord> expected = new List<DerivedRecord> { rc };


            //Act
            CSVParser parser = new CSVParser(fs); //Inject dependency here
            //Since we have the expected result we just clone the schema instead of building it by hand.
            List<DerivedRecord> actual = parser.ParseType<DerivedRecord>(path);


            //Assert
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestNestedClassParsing() {
            string input = "PartNumber,CustomerReference.CustomerID,CustomerReference.CustomerPartNumber,CustomerReference.CustomerPartDescription\nPART-001,CUST-42,CUSTOM-ABC,Replacement filter\n";

            MockFileSystem fs = new MockFileSystem();
            MockFileData mockInputFile = new MockFileData(input);
            string path = @"C:\test.csv";
            fs.AddFile(path, mockInputFile);

            CustomerPartCrossReference expected = new CustomerPartCrossReference {
                PartNumber = "PART-001",
                CustomerReference = new CustomerPartReference {
                    CustomerID = "CUST-42",
                    CustomerPartNumber = "CUSTOM-ABC",
                    CustomerPartDescription = "Replacement filter"
                }
            };

            CSVParser parser = new CSVParser(fs);
            CSVParseOptions options = new CSVParseOptions {
                EnforceColumnCount = true
            };
            List<CustomerPartCrossReference> actual = parser.ParseType<CustomerPartCrossReference>(path, options);

            CollectionAssert.AreEqual(new List<CustomerPartCrossReference> { expected }, actual);
        }
    }
}
