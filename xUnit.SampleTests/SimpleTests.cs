namespace xUnit.SampleTests;

using Xunit;
using Xunit.Asserts.Compare;

public class Student
{
    public int Id { get; set; }

    public string Name { get; set; }
}

public class SimpleTests
{
    [Fact]
    public void CompareTwoEqualStudent()
    {
        // Arrange
        var expected = new Student { Id = 1, Name = "Jacob" };
        var actual = new Student { Id = 1, Name = "Jacob" };

        // Assert
        DeepAssert.Equal(expected, actual);
    }

    [Fact]
    public void CompareTwoNotEqualStudent()
    {
        // Arrange
        var expected = new Student { Id = 1, Name = "James" };
        var actual = new Student { Id = 1, Name = "Jacob" };

        // Assert
        DeepAssert.NotEqual(expected, actual);
    }
}