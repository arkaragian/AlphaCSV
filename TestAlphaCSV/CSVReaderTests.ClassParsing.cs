using AlphaCSV;
using Xunit;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAlphaCSV {
    public partial class CSVReaderTests {
#pragma warning disable CA1707

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



        [Fact]
        public void ParseType_SimpleClass_ReturnsExpectedRecord() {
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
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ParseType_ReversedColumnOrder_ReturnsExpectedRecord() {
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
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ParseType_DerivedClass_ReturnsExpectedRecord() {
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
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ParseType_NestedClass_ReturnsExpectedRecord() {
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

            Assert.Equal(new List<CustomerPartCrossReference> { expected }, actual);
        }
#pragma warning restore CA1707
    }
}
