﻿using AlphaCSV;
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
    }
}