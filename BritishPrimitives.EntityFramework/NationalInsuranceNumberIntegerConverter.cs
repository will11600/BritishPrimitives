using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

namespace BritishPrimitives.EntityFramework;

/// <summary>
/// Converts a <see cref="NationalInsuranceNumber"/> value to and from a <see cref="ulong"/> (integer) representation for data storage.
/// </summary>
public sealed class NationalInsuranceNumberIntegerConverter(ConverterMappingHints? mappingHints = null) :
    ValueConverter<NationalInsuranceNumber, ulong>(
        ToProvider,
        FromProvider,
        mappingHints)
{

    private static Expression<Func<NationalInsuranceNumber, ulong>> ToProvider { get; } = m => (ulong)m;

    private static Expression<Func<ulong, NationalInsuranceNumber>> FromProvider { get; } = m => (NationalInsuranceNumber)m;
}
