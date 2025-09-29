![British Primitives](/Wordmark.svg?raw=true)
![NuGet Version](https://img.shields.io/nuget/v/BritishPrimitives)
## Introduction

`BritishPrimitives` is a .NET library that provides a set of primitive types for representing common UK-specific data formats. These types are designed to be lightweight and efficient, with a focus on performance and ease of use. The library also includes support for serialization and Entity Framework Core, making it easy to use in a variety of applications.

The following table lists the types included in the library, along with their sizes in memory:

| Type | Size (bytes) | Size (bits) |
| --- | --- | --- |
| `CompanyRegistrationNumber` | 6 | 48 |
| `NationalInsuranceNumber` | 5 | 40 |
| `PostalCode` | 8 | 64 |
| `VATRegistrationNumber` | 5 | 40 |

## Installation

You can install the library through NuGet Package Manager:

```bash
Install-Package BritishPrimitives
```

## Validation & Initialization

All of the primitive types in this library can be initialized in a similar way. For example, you can create a `PostalCode` from a string, and the library will validate the format for you:

```csharp
// Valid postcode
var postcode = PostalCode.Parse("SW1A 0AA");

// Invalid postcode - throws FormatException
var invalidPostcode = PostalCode.Parse("not a postcode");
```

You can also use the `TryParse` method to avoid exceptions:

```csharp
if (PostalCode.TryParse("SW1A 0AA", out var postcode))
{
    // ...
}
```

## Serialization

### Binary

All of the structs in this library can be marshaled directly into bytes. They also provide explicit conversions to and from `ulong` for easy binary serialization:

```csharp
var postcode = PostalCode.Parse("SW1A 0AA");
ulong value = (ulong)postcode;
```

### JSON

The `BritishPrimitives.Json` library provides converters for `System.Text.Json`. You can serialize the primitive types as either strings or integers. Here's how to use the `BritishPrimitiveStringConverter` and `BritishPrimitiveIntegerConverter`:

**Using attributes:**

```csharp
public class MyModel
{
    [JsonConverter(typeof(BritishPrimitiveStringConverter<PostalCode>))]
    public PostalCode Postcode { get; set; }
}
```

**Using a `BritishPrimitiveConverterFactory`:**

```csharp
var options = new JsonSerializerOptions
{
    Converters = { new BritishPrimitiveConverterFactory<BritishPrimitiveStringConverter<PostalCode>, PostalCode>() }
};

var model = new MyModel { Postcode = PostalCode.Parse("SW1A 0AA") };
var json = JsonSerializer.Serialize(model, options);
```

### Entity Framework

The `BritishPrimitives.EntityFramework` library provides value converters for Entity Framework Core. You can use these converters to store the primitive types in your database as either strings or integers. Here's an example of how to configure a `PostalCode` property in your `DbContext`:

**The Model:**

```csharp
public class Address
{
    public int Id { get; set; }
    public PostalCode Postcode { get; set; }
}
```

**The DbContext:**

```csharp
public class MyDbContext : DbContext
{
    public DbSet<Address> Addresses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("your_connection_string");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>()
            .Property(e => e.Postcode)
            .HasConversion<PostalCodeStringConverter>();
    }
}
```