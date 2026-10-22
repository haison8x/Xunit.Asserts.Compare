namespace Xunit.Asserts.Compare.Tests;

public class StringOptionTests
{
    [Fact]
    public void Strings_are_case_sensitive_by_default()
    {
        Assert.Throws<EqualException>(() => DeepAssert.Equal(new PersonDto { Name = "Jacob" }, new PersonDto { Name = "JACOB" }));
    }

    [Fact]
    public void IgnoreCase_accepts_case_differences()
    {
        DeepAssert.Equal(new PersonDto { Name = "Jacob" }, new PersonDto { Name = "JACOB" }, o => o.IgnoreCase());
    }

    [Fact]
    public void Null_and_empty_strings_differ_by_default()
    {
        Assert.Throws<EqualException>(() => DeepAssert.Equal(new PersonDto { Name = null }, new PersonDto { Name = "" }));
    }

    [Fact]
    public void TreatNullAndEmptyStringAsEqual_accepts_null_versus_empty()
    {
        DeepAssert.Equal(new PersonDto { Name = null }, new PersonDto { Name = "" }, o => o.TreatNullAndEmptyStringAsEqual());
    }

    [Fact]
    public void Whitespace_is_significant_by_default()
    {
        Assert.Throws<EqualException>(() => DeepAssert.Equal(new PersonDto { Name = "Jacob" }, new PersonDto { Name = "  Jacob " }));
    }

    [Fact]
    public void IgnoreLeadingTrailingWhitespace_trims_before_comparing()
    {
        DeepAssert.Equal(new PersonDto { Name = "Jacob" }, new PersonDto { Name = "  Jacob " }, o => o.IgnoreLeadingTrailingWhitespace());
    }

    [Fact]
    public void IgnoreLeadingTrailingWhitespace_keeps_inner_whitespace_significant()
    {
        Assert.Throws<EqualException>(
            () => DeepAssert.Equal(new PersonDto { Name = "Ja cob" }, new PersonDto { Name = "Jacob" }, o => o.IgnoreLeadingTrailingWhitespace()));
    }
}
