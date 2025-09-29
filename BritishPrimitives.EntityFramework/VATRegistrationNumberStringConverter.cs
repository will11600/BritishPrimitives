using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

namespace BritishPrimitives.EntityFramework;

/// <summary>
/// Converts a <see cref="VATRegistrationNumber"/> value to and from a <see cref="string"/> representation for data storage.
/// </summary>
public sealed class VATRegistrationNumberStringConverter(ConverterMappingHints? mappingHints = null) :
    ValueConverter<VATRegistrationNumber, string>(
        ToProvider,
        FromProvider,
        mappingHints)
{
    private static Expression<Func<VATRegistrationNumber, string>> ToProvider { get; } = m => m.ToString(null, null);

    private static Expression<Func<string, VATRegistrationNumber>> FromProvider { get; } = m => VATRegistrationNumber.Parse(m, null);
}
