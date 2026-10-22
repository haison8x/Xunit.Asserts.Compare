namespace Xunit.Asserts.Compare.Tests.UseCases;

public class Customer
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Address? ShippingAddress { get; set; }

    public List<OrderLine> Lines { get; set; } = [];
}

public class Address
{
    public string? Street { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }
}

public class OrderLine
{
    public string? Sku { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}

public class CustomerDto
{
    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? City { get; set; }
}

public class Invoice
{
    public string? Number { get; set; }

    public double TaxRate { get; set; }

    public decimal Total { get; set; }

    public DateTime IssuedAt { get; set; }

    public AuditTrail? Audit { get; set; }
}

public class AuditTrail
{
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>A tiny stand-in for a repository or service under test.</summary>
public static class CustomerService
{
    public static Customer Register(string name, string email, string city) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Email = email,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        ShippingAddress = new Address { Street = "1 Main St", City = city, Country = "VN" },
    };

    public static CustomerDto ToDto(Customer customer) => new()
    {
        Name = customer.Name,
        Email = customer.Email,
        City = customer.ShippingAddress?.City,
    };

    // Simulates a database query whose row order is not guaranteed.
    public static List<OrderLine> LoadLines() =>
    [
        new() { Sku = "PEN", Quantity = 10, UnitPrice = 1.20m },
        new() { Sku = "BOOK", Quantity = 1, UnitPrice = 12.50m },
    ];
}
