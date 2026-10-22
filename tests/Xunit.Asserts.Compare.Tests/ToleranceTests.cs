namespace Xunit.Asserts.Compare.Tests;

public class ToleranceTests
{
    private static readonly DateTime BaseTime = new(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);

    private static Measurement Create() => new()
    {
        Value = 1.0,
        Amount = 10.00m,
        Timestamp = BaseTime,
        RecordedAt = new DateTimeOffset(BaseTime),
    };

    [Fact]
    public void Double_values_must_match_exactly_by_default()
    {
        var actual = Create();
        actual.Value = 1.0001;

        Assert.Throws<EqualException>(() => DeepAssert.Equal(Create(), actual));
    }

    [Fact]
    public void WithDoubleTolerance_accepts_differences_within_tolerance()
    {
        var actual = Create();
        actual.Value = 1.0001;

        DeepAssert.Equal(Create(), actual, o => o.WithDoubleTolerance(0.001));
    }

    [Fact]
    public void WithDoubleTolerance_rejects_differences_beyond_tolerance()
    {
        var actual = Create();
        actual.Value = 1.01;

        Assert.Throws<EqualException>(() => DeepAssert.Equal(Create(), actual, o => o.WithDoubleTolerance(0.001)));
    }

    [Fact]
    public void WithDecimalTolerance_accepts_differences_within_tolerance()
    {
        var actual = Create();
        actual.Amount = 10.004m;

        DeepAssert.Equal(Create(), actual, o => o.WithDecimalTolerance(0.01m));
    }

    [Fact]
    public void WithDecimalTolerance_rejects_differences_beyond_tolerance()
    {
        var actual = Create();
        actual.Amount = 10.5m;

        Assert.Throws<EqualException>(() => DeepAssert.Equal(Create(), actual, o => o.WithDecimalTolerance(0.01m)));
    }

    [Fact]
    public void WithDateTimeTolerance_accepts_differences_within_tolerance()
    {
        var actual = Create();
        actual.Timestamp = BaseTime.AddMilliseconds(400);
        actual.RecordedAt = actual.RecordedAt.AddMilliseconds(-400);

        DeepAssert.Equal(Create(), actual, o => o.WithDateTimeTolerance(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void WithDateTimeTolerance_rejects_differences_beyond_tolerance()
    {
        var actual = Create();
        actual.Timestamp = BaseTime.AddSeconds(5);

        Assert.Throws<EqualException>(() => DeepAssert.Equal(Create(), actual, o => o.WithDateTimeTolerance(TimeSpan.FromSeconds(1))));
    }

    [Fact]
    public void Tolerances_must_not_be_negative()
    {
        var options = new DeepAssertOptions<Measurement>();

        Assert.Throws<ArgumentOutOfRangeException>(() => options.WithDoubleTolerance(-0.1));
        Assert.Throws<ArgumentOutOfRangeException>(() => options.WithDecimalTolerance(-0.1m));
        Assert.Throws<ArgumentOutOfRangeException>(() => options.WithDateTimeTolerance(TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public void Tolerances_must_be_finite()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DeepAssertOptions<Measurement>().WithDoubleTolerance(double.NaN));
    }
}
