using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

namespace BritishPrimitives.EntityFramework;

/// <summary>
/// Converts a <see cref="CompanyRegistrationNumber"/> value to and from a <see cref="string"/> representation for data storage.
/// </summary>
public sealed class CompanyRegistrationNumberStringConverter(ConverterMappingHints? mappingHints = null) :
    ValueConverter<CompanyRegistrationNumber, string>(
        ToProvider,
        FromProvider,
        mappingHints)
{
    private static Expression<Func<CompanyRegistrationNumber, string>> ToProvider { get; } = m => m.ToString(null, null);

    private static Expression<Func<string, CompanyRegistrationNumber>> FromProvider { get; } = m => CompanyRegistrationNumber.Parse(m, null);
}