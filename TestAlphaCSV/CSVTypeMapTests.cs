using AlphaCSV;
using Xunit;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;

namespace TestAlphaCSV;

public sealed class CSVTypeMapTests {
#pragma warning disable CA1707

    private readonly ITestOutputHelper _output;

    public CSVTypeMapTests(ITestOutputHelper output) {
        _output = output;
    }

    [Fact]
    public void MappedType_NestedFieldsAndReorderedColumns_RoundTrips() {
        CSVTypeMap<Customer> map = new CSVTypeMap<Customer>(() => new Customer())
            .AddField("Name", customer => customer.Name, (customer, value) => customer.Name = value)
            .AddField("Address.City", customer => customer.Address.City, (customer, value) => customer.Address.City = value);
        MockFileSystem fileSystem = new();
        fileSystem.AddFile("/customer.csv", new MockFileData("Address.City,Name\nAthens,Ada\n"));

        List<Customer> parsed = new CSVParser(fileSystem).ParseType("/customer.csv", map);

        Assert.Single(parsed);
        Assert.Equal("Ada", parsed[0].Name);
        Assert.Equal("Athens", parsed[0].Address.City);
        Assert.Equal("Name,Address.City\nAda,Athens\n", CSVWriter.WriteType(parsed, map));
    }

    [Fact]
    public void AddField_DuplicateName_Throws() {
        CSVTypeMap<Customer> map = new(() => new Customer());
        _ = map.AddField("Name", customer => customer.Name, (customer, value) => customer.Name = value);

        ArgumentException exception = Assert.Throws<ArgumentException>(() => map.AddField("Name", customer => customer.Name, (customer, value) => customer.Name = value));

        Assert.Contains("already mapped", exception.Message);
    }

    [Fact]
    public void Constructor_NullFactory_Throws() {
        Assert.Throws<ArgumentNullException>(() => new CSVTypeMap<Customer>(factory: null!));
    }

    [Fact]
    public void AddField_EmptyName_Throws() {
        CSVTypeMap<Customer> map = new(() => new Customer());

        Assert.Throws<ArgumentException>(() => map.AddField(" ", customer => customer.Name, (customer, value) => customer.Name = value));
    }

    [Fact]
    public void AddField_NullGetter_Throws() {
        CSVTypeMap<Customer> map = new(() => new Customer());

        Assert.Throws<ArgumentNullException>(() => map.AddField<string>("Name", getter: null!, (customer, value) => customer.Name = value));
    }

    [Fact]
    public void AddField_NullSetter_Throws() {
        CSVTypeMap<Customer> map = new(() => new Customer());

        Assert.Throws<ArgumentNullException>(() => map.AddField("Name", customer => customer.Name, setter: null!));
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
