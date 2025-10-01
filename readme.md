![British Primitives](/Wordmark.svg?raw=true)
![NuGet Version](https://img.shields.io/nuget/v/BritishPrimitives)
## Introduction

`BritishPrimitives` is a .NET library that provides a set of primitive types for representing common UK-specific data formats. These types are designed to be lightweight and efficient, with a focus on performance and ease of use. The library also includes support for serialization and [Entity Framework Core](#Entity-Framework), making it easy to use in a variety of applications.

The following table lists the types included in the library, along with their sizes in memory:


| Type | Size (bytes) |
| --- | --- |
| `CompanyRegistrationNumber` | 5 |
| `NationalInsuranceNumber` | 5 |
| `PostalCode` | 8 |
| `VATRegistrationNumber` | 5 |

## Installation

You can install the library through the NuGet Package Manager:

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

All of the structs in this library can be marshaled directly into bytes. They also provide explicit conversions to and from `ulong` for easy binary serialization:

```csharp
var postcode = PostalCode.Parse("SW1A 0AA");
ulong value = (ulong)postcode;
```

### JSON

The `BritishPrimitives.Json` library provides converters for serializing and deserializing the primitive types to and from JSON. You can serialize them as either strings or integers.

#### Installation

You can install the library through the NuGet Package Manager:

```bash
Install-Package BritishPrimitives.Json
```

#### Usage

You can use the converters in two main ways: either by applying the `[JsonConverter]` attribute directly to a property or by adding a converter factory to `JsonSerializerOptions`.

##### Using Converters Directly with attributes

For individual properties, you can apply the JSON converter attribute with either `PrimitiveStringConverter<T>` or `PrimitiveIntegerConverter<T>`.

This converter will serialize the primitive type as a JSON string.

```csharp
using BritishPrimitives.Json;
using System.Text.Json.Serialization;

public class MyModel
{
    [JsonConverter(typeof(PrimitiveStringConverter<PostalCode>))]
    public PostalCode Postcode { get; set; }
}
```

##### Using Converter Factories

For a more general approach, you can use `PrimitiveStringConverterFactory` or `PrimitiveIntegerConverterFactory` to handle all primitive types in your model.

**`PrimitiveStringConverterFactory`**

This factory will create string converters for all types that implement `IPrimitive<T>`.

```csharp
var options = new JsonSerializerOptions
{
    Converters = { new PrimitiveIntegerConverterFactory() }
};

var model = new MyModel { Crn = CompanyRegistrationNumber.Parse("12345678") };
var json = JsonSerializer.Serialize(model, options);
```

### Entity Framework
![NuGet Version](https://img.shields.io/nuget/v/BritishPrimitives.EntityFramework)

The `BritishPrimitives.EntityFramework` library provides value converters for Entity Framework Core. You can use these converters to store the primitive types in your database as either strings or integers.

#### Installation

You can install the library through the NuGet Package Manager:

```bash
Install-Package BritishPrimitives.EntityFramework
```

#### Usage

##### The Model

```csharp
using BritishPrimitives.EntityFramework;

public class Address
{
    public int Id { get; set; }
    public PostalCode Postcode { get; set; }
}
```

##### The DbContext

```csharp
using BritishPrimitives.EntityFramework;

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