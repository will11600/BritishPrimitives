using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

namespace BritishPrimitives.EntityFramework;

/// <summary>
/// Converts a <see cref="VatRegistrationNumber"/> value to and from a <see cref="string"/> representation for data storage.
/// </summary>
public sealed class VatRegistrationNumberStringConverter(ConverterMappingHints? mappingHints = null) :
    ValueConverter<VatRegistrationNumber, string>(
        ToProvider,
        FromProvider,
        mappingHints)
{
    private static Expression<Func<VatRegistrationNumber, string>> ToProvider { get; } = m => m.ToString(null, null);

    private static Expression<Func<string, VatRegistrationNumber>> FromProvider { get; } = m => VatRegistrationNumber.Parse(m, null);
}
