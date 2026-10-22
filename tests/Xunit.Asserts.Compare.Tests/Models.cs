namespace Xunit.Asserts.Compare.Tests;

public class Address
{
    public string? Street { get; set; }

    public string? City { get; set; }
}

public class Order
{
    public int Id { get; set; }

    public string? Product { get; set; }

    public decimal Price { get; set; }

    public double Weight { get; set; }
}

public class Person
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int Age { get; set; }

    public DateTime CreatedAt { get; set; }

    public Address? Address { get; set; }

    public List<Order> Orders { get; set; } = [];

    public Dictionary<string, int> Scores { get; set; } = [];
}

public class Employee : Person
{
    public string? Department { get; set; }
}

public class PersonDto
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int Age { get; set; }
}

public class AuditInfo
{
    public string? ModifiedBy { get; set; }
}

public class Document
{
    public string? Title { get; set; }

    public AuditInfo? Audit { get; set; }
}

public class Measurement
{
    public double Value { get; set; }

    public decimal Amount { get; set; }

    public DateTime Timestamp { get; set; }

    public DateTimeOffset RecordedAt { get; set; }
}

public class SecretHolder
{
    private readonly string _secret;

    public SecretHolder(string secret)
    {
        _secret = secret;
    }

    public string Public { get; set; } = "same";

    public override string ToString() => $"SecretHolder({_secret})";
}

public static class TestData
{
    public static Person CreatePerson() => new()
    {
        Id = 1,
        Name = "Jacob",
        Age = 30,
        CreatedAt = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc),
        Address = new Address { Street = "1 Main St", City = "Hanoi" },
        Orders =
        [
            new Order { Id = 10, Product = "Book", Price = 12.50m, Weight = 0.4 },
            new Order { Id = 11, Product = "Pen", Price = 1.20m, Weight = 0.01 },
        ],
        Scores = new Dictionary<string, int> { ["math"] = 9, ["art"] = 7 },
    };
}
