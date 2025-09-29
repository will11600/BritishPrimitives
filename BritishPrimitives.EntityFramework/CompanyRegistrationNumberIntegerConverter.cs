using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

namespace BritishPrimitives.EntityFramework;

/// <summary>
/// Converts a <see cref="CompanyRegistrationNumber"/> value to and from a <see cref="ulong"/> (integer) representation for data storage.
/// </summary>
public sealed class CompanyRegistrationNumberIntegerConverter(ConverterMappingHints? mappingHints = null) :
    ValueConverter<CompanyRegistrationNumber, ulong>(
        ToProvider,
        FromProvider,
        mappingHints)
{
    private static Expression<Func<CompanyRegistrationNumber, ulong>> ToProvider { get; } = m => (ulong)m;

    private static Expression<Func<ulong, CompanyRegistrationNumber>> FromProvider { get; } = m => (CompanyRegistrationNumber)m;
}
