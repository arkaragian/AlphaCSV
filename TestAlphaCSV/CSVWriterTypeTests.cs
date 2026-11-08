//
// Copyright (c) Aris Karagiannidis and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using AlphaCSV;
using Xunit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions.TestingHelpers;

namespace TestAlphaCSV {
    public sealed class CSVWriterTypeTests {
#pragma warning disable CA1707

        private readonly ITestOutputHelper _output;

        public CSVWriterTypeTests(ITestOutputHelper output) {
            _output = output;
        }

        [Fact]
        public void WriteType_ObjectsHavePublicReadableProperties_WritesHeaderAndRows() {
            // Arrange
            WritableModel[] input = {
                new WritableModel(name: "Alpha", count: 1),
                new WritableModel(name: "Beta, \"quoted\"", count: 2)
            };

            // Act
            string result = CSVWriter.WriteType(input, options: null);

            // Assert
            Assert.Equal("Name,Count\nAlpha,1\n\"Beta, \"\"quoted\"\"\",2\n", result);
        }

        [Fact]
        public void WriteType_SequenceIsEmpty_WritesHeaderOnly() {
            // Arrange
            WritableModel[] input = Array.Empty<WritableModel>();

            // Act
            string result = CSVWriter.WriteType(input, options: null);

            // Assert
            Assert.Equal("Name,Count\n", result);
        }

        [Fact]
        public void WriteType_OutputIsParsedAgain_PreservesObjects() {
            // Arrange
            List<RoundTripModel> expected = new() {
                new RoundTripModel() {
                    Name = "Alpha,Beta",
                    Quantity = 12,
                    Enabled = true,
                    Price = 1234.5m,
                    Created = new DateTime(year: 2024, month: 2, day: 3, hour: 4, minute: 5, second: 6),
                    State = RoundTripState.Active
                },
                new RoundTripModel() {
                    Name = "Gamma",
                    Quantity = -3,
                    Enabled = false,
                    Price = 0.25m,
                    Created = new DateTime(year: 2025, month: 6, day: 7, hour: 8, minute: 9, second: 10),
                    State = RoundTripState.Archived
                }
            };
            CSVWriteOptions writeOptions = CreateWriteOptions();
            CSVParseOptions parseOptions = CreateParseOptions();
            MockFileSystem fileSystem = new();

            // Act
            string csv = CSVWriter.WriteType(expected, writeOptions);
            fileSystem.AddFile(ROUND_TRIP_PATH, new MockFileData(csv));
            CSVParser reader = new(fileSystem);
            List<RoundTripModel> actual = reader.ParseType<RoundTripModel>(ROUND_TRIP_PATH, parseOptions);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WriteTypeLine_OutputIsParsedAgain_PreservesObject() {
            // Arrange
            RoundTripModel expected = new() {
                Name = "Alpha,Beta",
                Quantity = 12,
                Enabled = true,
                Price = 1234.5m,
                Created = new DateTime(year: 2024, month: 2, day: 3, hour: 4, minute: 5, second: 6),
                State = RoundTripState.Active
            };
            CSVWriteOptions writeOptions = CreateWriteOptions();
            CSVParseOptions parseOptions = CreateParseOptions();
            MockFileSystem fileSystem = new();

            // Act
            string line = CSVWriter.WriteTypeLine(expected, writeOptions);
            string csv = $"Name,Quantity,Enabled,Price,Created,State\n{line}\n";
            fileSystem.AddFile(ROUND_TRIP_PATH, new MockFileData(csv));
            CSVParser reader = new(fileSystem);
            List<RoundTripModel> actual = reader.ParseType<RoundTripModel>(ROUND_TRIP_PATH, parseOptions);

            // Assert
            Assert.Single(actual);
            Assert.Equal(expected, actual[0]);
        }

        [Fact]
        public void WriteTypeLine_ValuesNeedFormatting_UsesConfiguredAndInvariantFormats() {
            // Arrange
            CSVWriteOptions options = new CSVWriteOptions() {
                CommonOptions = new CSVOptions() {
                    DateTimeFormat = "yyyy/MM/dd HH:mm"
                }
            };
            FormattedModel input = new FormattedModel() {
                Missing = null,
                Enabled = false,
                Created = new DateTime(year: 2024, month: 2, day: 3, hour: 4, minute: 5, second: 0),
                Amount = 1234.5m
            };
            CultureInfo originalCulture = CultureInfo.CurrentCulture;

            try {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("el-GR");

                // Act
                string result = CSVWriter.WriteTypeLine(input, options);

                // Assert
                Assert.Equal(",false,2024/02/03 04:05,1234.5", result);
            } finally {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Theory]
        [InlineData("Alpha,Beta", "\"Alpha,Beta\"")]
        [InlineData("Alpha \"Beta\"", "\"Alpha \"\"Beta\"\"\"")]
        [InlineData("Alpha\rBeta", "\"Alpha\rBeta\"")]
        [InlineData("Alpha\nBeta", "\"Alpha\nBeta\"")]
        public void WriteTypeLine_FieldContainsCsvSpecialCharacter_QuotesAndEscapesField(string value, string expected) {
            // Arrange
            EscapedModel input = new EscapedModel() {
                Value = value
            };

            // Act
            string result = CSVWriter.WriteTypeLine(input, options: null);

            // Assert
            Assert.Equal(expected, result);
        }

        private sealed class WritableModel {
            public WritableModel(string name, int count) {
                Name = name;
                Count = count;
            }

            public string Name { get; }

            public int Count { get; }

            public string PropertyWithPrivateGetter { private get; set; } = "Not written";

            public string WriteOnlyProperty { set { } }
        }

        private sealed class FormattedModel {
            public string? Missing { get; set; }

            public bool Enabled { get; set; }

            public DateTime Created { get; set; }

            public decimal Amount { get; set; }
        }

        private sealed class EscapedModel {
            public string Value { get; set; } = string.Empty;
        }

        public sealed record RoundTripModel {
            public string Name { get; set; } = string.Empty;

            public int Quantity { get; set; }

            public bool Enabled { get; set; }

            public decimal Price { get; set; }

            public DateTime Created { get; set; }

            public RoundTripState State { get; set; }
        }

        public enum RoundTripState {
            Active,
            Archived
        }

        private const string ROUND_TRIP_PATH = @"C:\round-trip.csv";

        private static CSVWriteOptions CreateWriteOptions() {
            return new CSVWriteOptions() {
                CommonOptions = new CSVOptions() {
                    DateTimeFormat = "yyyy-MM-dd HH:mm:ss"
                }
            };
        }

        private static CSVParseOptions CreateParseOptions() {
            return new CSVParseOptions() {
                CommonOptions = new CSVOptions() {
                    DateTimeFormat = "yyyy-MM-dd HH:mm:ss"
                }
            };
        }

#pragma warning restore CA1707
    }
}
