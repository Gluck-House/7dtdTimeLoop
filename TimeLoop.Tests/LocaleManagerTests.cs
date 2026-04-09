using System.Reflection;
using TimeLoop.Managers;

namespace TimeLoop.Tests;

public class LocaleManagerTests {
    [Fact]
    public void ParseLocaleDictionary_ParsesBomPrefixedJsonAndIgnoresNonStringValues() {
        const string json = "\uFEFF{\"prefix\":\"[TL] \",\"hello\":\"world\",\"count\":1,\"nested\":{\"x\":1}}";

        var parsed = InvokeParseLocaleDictionary(json);

        Assert.Equal(2, parsed.Count);
        Assert.Equal("[TL] ", parsed["prefix"]);
        Assert.Equal("world", parsed["hello"]);
    }

    [Fact]
    public void ParseLocaleDictionary_ThrowsWhenRootIsNotAnObject() {
        var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseLocaleDictionary("[]"));

        Assert.IsType<FormatException>(exception.InnerException);
        Assert.Equal("Locale file root must be a JSON object.", exception.InnerException?.Message);
    }

    [Fact]
    public void ParseLocaleDictionary_ThrowsWhenNoStringEntriesExist() {
        var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseLocaleDictionary("{\"count\":1}"));

        Assert.IsType<FormatException>(exception.InnerException);
        Assert.Equal("Locale file does not contain any string entries.", exception.InnerException?.Message);
    }

    private static Dictionary<string, string> InvokeParseLocaleDictionary(string json) {
        var method = typeof(LocaleManager).GetMethod("ParseLocaleDictionary",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        return (Dictionary<string, string>)method.Invoke(null, new object[] { json })!;
    }
}
