//
// Copyright (c) Aris Karagiannidis and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using AlphaCSV;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

namespace TestAlphaCSV {
    [TestClass]
    public sealed class CSVWriterTypeTests {
#pragma warning disable CA1707

        [TestMethod]
        public void WriteType_ObjectsHavePublicReadableProperties_WritesHeaderAndRows() {
            // Arrange
            WritableModel[] input = {
                new WritableModel(name: "Alpha", count: 1),
                new WritableModel(name: "Beta, \"quoted\"", count: 2)
            };

            // Act
            string result = CSVWriter.WriteType(input, options: null);

            // Assert
            Assert.AreEqual("Name,Count\nAlpha,1\n\"Beta, \"\"quoted\"\"\",2\n", result);
        }

        [TestMethod]
        public void WriteType_SequenceIsEmpty_WritesHeaderOnly() {
            // Arrange
            WritableModel[] input = Array.Empty<WritableModel>();

            // Act
            string result = CSVWriter.WriteType(input, options: null);

            // Assert
            Assert.AreEqual("Name,Count\n", result);
        }

        [TestMethod]
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
                Assert.AreEqual(",false,2024/02/03 04:05,1234.5", result);
            } finally {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [TestMethod]
        [DataRow("Alpha,Beta", "\"Alpha,Beta\"")]
        [DataRow("Alpha \"Beta\"", "\"Alpha \"\"Beta\"\"\"")]
        [DataRow("Alpha\rBeta", "\"Alpha\rBeta\"")]
        [DataRow("Alpha\nBeta", "\"Alpha\nBeta\"")]
        public void WriteTypeLine_FieldContainsCsvSpecialCharacter_QuotesAndEscapesField(string value, string expected) {
            // Arrange
            EscapedModel input = new EscapedModel() {
                Value = value
            };

            // Act
            string result = CSVWriter.WriteTypeLine(input, options: null);

            // Assert
            Assert.AreEqual(expected, result);
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

#pragma warning restore CA1707
    }
}
