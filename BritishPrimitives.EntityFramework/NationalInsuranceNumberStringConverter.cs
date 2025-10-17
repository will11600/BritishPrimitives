using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

namespace BritishPrimitives.EntityFramework;

/// <summary>
/// Converts a <see cref="NationalInsuranceNumber"/> value to and from a <see cref="string"/> representation for data storage.
/// </summary>
public sealed class NationalInsuranceNumberStringConverter(ConverterMappingHints? mappingHints = null) :
    ValueConverter<NationalInsuranceNumber, string>(
        ToProvider,
        FromProvider,
        mappingHints)
{
    private static Expression<Func<NationalInsuranceNumber, string>> ToProvider { get; } = m => m.ToString(null, null);

    private static Expression<Func<string, NationalInsuranceNumber>> FromProvider { get; } = m => NationalInsuranceNumber.Parse(m, null);
}
