namespace Xunit.Asserts.Compare.Tests.UseCases;

public class Student
{
    public int Id { get; set; }

    public string? Name { get; set; }
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

    [Fact]
    public void CompareStudentIgnoringId()
    {
        // Arrange
        var expected = new Student { Id = 1, Name = "Jacob" };
        var actual = new Student { Id = 2, Name = "Jacob" };

        // Assert
        DeepAssert.Equal(expected, actual, "Id");
        DeepAssert.Equal(expected, actual, o => o.Ignore(s => s.Id));
    }
}
