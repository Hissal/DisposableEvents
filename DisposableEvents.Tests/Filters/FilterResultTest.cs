using DisposableEvents;
using FluentAssertions;
using Xunit;

namespace DisposableEvents.Tests.Filters;

[TestSubject(typeof(FilterResult))]
public class FilterResultTest {

    [Fact]
    public void Ctor_True_SetsPassedTrue_And_BlockedFalse() {
        var result = new FilterResult(true);
        
        result.Passed.Should().BeTrue();
        result.Blocked.Should().BeFalse();
    }

    [Fact]
    public void Ctor_False_SetsPassedFalse_And_BlockedTrue() {
        var result = new FilterResult(false);
        
        result.Passed.Should().BeFalse();
        result.Blocked.Should().BeTrue();
    }

    [Fact]
    public void Factory_Pass_SetsPassedTrue_And_BlockedFalse() {
        var result = FilterResult.Pass;
        
        result.Passed.Should().BeTrue();
        result.Blocked.Should().BeFalse();
    }

    [Fact]
    public void Factory_Block_SetsPassedFalse_And_BlockedTrue() {
        var result = FilterResult.Block;
        
        result.Passed.Should().BeFalse();
        result.Blocked.Should().BeTrue();
    }

    [Fact]
    public void Implicit_FromBool_True_YieldsPassedTrue() {
        FilterResult result = true;
        
        result.Passed.Should().BeTrue();
        result.Blocked.Should().BeFalse();
    }

    [Fact]
    public void Implicit_FromBool_False_YieldsPassedFalse() {
        FilterResult result = false;
        
        result.Passed.Should().BeFalse();
        result.Blocked.Should().BeTrue();
    }

    [Fact]
    public void Implicit_ToBool_FromPassedTrue_IsTrue() {
        var input = new FilterResult(true);
        bool asBool = input;
        asBool.Should().BeTrue();
    }

    [Fact]
    public void Implicit_ToBool_FromPassedFalse_IsFalse() {
        var input = new FilterResult(false);
        bool asBool = input;
        asBool.Should().BeFalse();
    }
}