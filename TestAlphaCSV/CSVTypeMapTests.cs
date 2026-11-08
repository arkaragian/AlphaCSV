using AlphaCSV;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;

namespace TestAlphaCSV;

[TestClass]
public sealed class CSVTypeMapTests {
#pragma warning disable CA1707

    [TestMethod]
    public void MappedType_NestedFieldsAndReorderedColumns_RoundTrips() {
        CSVTypeMap<Customer> map = new CSVTypeMap<Customer>(() => new Customer())
            .AddField("Name", customer => customer.Name, (customer, value) => customer.Name = value)
            .AddField("Address.City", customer => customer.Address.City, (customer, value) => customer.Address.City = value);
        MockFileSystem fileSystem = new();
        fileSystem.AddFile("/customer.csv", new MockFileData("Address.City,Name\nAthens,Ada\n"));

        List<Customer> parsed = new CSVParser(fileSystem).ParseType("/customer.csv", map);

        Assert.HasCount(1, parsed);
        Assert.AreEqual("Ada", parsed[0].Name);
        Assert.AreEqual("Athens", parsed[0].Address.City);
        Assert.AreEqual("Name,Address.City\nAda,Athens\n", CSVWriter.WriteType(parsed, map));
    }

    private sealed class Customer {
        public string Name { get; set; } = "";
        public Address Address { get; } = new();
    }

    private sealed class Address {
        public string City { get; set; } = "";
    }

#pragma warning restore CA1707
}
