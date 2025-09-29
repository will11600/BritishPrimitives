using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

namespace BritishPrimitives.EntityFramework;

/// <summary>
/// Converts a <see cref="PostalCode"/> value to and from a <see cref="string"/> representation for data storage.
/// </summary>
public sealed class PostalCodeStringConverter(ConverterMappingHints? mappingHints = null) :
    ValueConverter<PostalCode, string>(
        ToProvider,
        FromProvider,
        mappingHints)
{
    private static Expression<Func<PostalCode, string>> ToProvider { get; } = m => m.ToString(null, null);

    private static Expression<Func<string, PostalCode>> FromProvider { get; } = m => PostalCode.Parse(m, null);
}
