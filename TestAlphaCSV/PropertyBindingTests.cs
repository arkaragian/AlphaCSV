using AlphaCSV.Models;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace TestAlphaCSV;

public sealed class PropertyBindingTests {
    private readonly ITestOutputHelper _output;

    public PropertyBindingTests(ITestOutputHelper output) {
        _output = output;
    }

#pragma warning disable CA1707

    [Fact]
    public void SetValue_ReadOnlyLeaf_LeavesExistingValueUnchanged() {
        PropertyInfo property = typeof(ReadOnlyLeaf).GetProperty(nameof(ReadOnlyLeaf.Value))!;
        PropertyBinding binding = new("Value", typeof(string), new List<PropertyInfo> { property });
        ReadOnlyLeaf instance = new();

        binding.SetValue(instance, "replacement");

        Assert.Equal("original", instance.Value);
    }

    [Fact]
    public void SetValue_ReadOnlyNullParent_DoesNotAssignConstructedParent() {
        PropertyInfo parent = typeof(ReadOnlyParent).GetProperty(nameof(ReadOnlyParent.Child))!;
        PropertyInfo leaf = typeof(ReadOnlyLeaf).GetProperty(nameof(ReadOnlyLeaf.Value))!;
        PropertyBinding binding = new("Child.Value", typeof(string), new List<PropertyInfo> { parent, leaf });
        ReadOnlyParent instance = new();

        binding.SetValue(instance, "replacement");

        Assert.Null(instance.Child);
    }

    private sealed class ReadOnlyLeaf {
        public string Value { get; } = "original";
    }

    private sealed class ReadOnlyParent {
        public ReadOnlyLeaf? Child { get; }
    }

#pragma warning restore CA1707
}
