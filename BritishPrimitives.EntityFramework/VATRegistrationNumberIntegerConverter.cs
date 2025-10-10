using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

namespace BritishPrimitives.EntityFramework;

/// <summary>
/// Converts a <see cref="VatRegistrationNumber"/> value to and from a <see cref="ulong"/> (integer) representation for data storage.
/// </summary>
public sealed class VATRegistrationNumberIntegerConverter(ConverterMappingHints? mappingHints = null) :
    ValueConverter<VatRegistrationNumber, ulong>(
        ToProvider,
        FromProvider,
        mappingHints)
{
    private static Expression<Func<VatRegistrationNumber, ulong>> ToProvider { get; } = m => (ulong)m;

    private static Expression<Func<ulong, VatRegistrationNumber>> FromProvider { get; } = m => (VatRegistrationNumber)m;
}
