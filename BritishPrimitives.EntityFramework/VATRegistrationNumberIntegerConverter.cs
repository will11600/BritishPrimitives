using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

namespace BritishPrimitives.EntityFramework;

/// <summary>
/// Converts a <see cref="VATRegistrationNumber"/> value to and from a <see cref="ulong"/> (integer) representation for data storage.
/// </summary>
public sealed class VATRegistrationNumberIntegerConverter(ConverterMappingHints? mappingHints = null) :
    ValueConverter<VATRegistrationNumber, ulong>(
        ToProvider,
        FromProvider,
        mappingHints)
{
    private static Expression<Func<VATRegistrationNumber, ulong>> ToProvider { get; } = m => (ulong)m;

    private static Expression<Func<ulong, VATRegistrationNumber>> FromProvider { get; } = m => (VATRegistrationNumber)m;
}
