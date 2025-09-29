using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

namespace BritishPrimitives.EntityFramework;

/// <summary>
/// Converts a <see cref="PostalCode"/> value to and from a <see cref="ulong"/> (integer) representation for data storage.
/// </summary>
public sealed class PostalCodeIntegerConverter(ConverterMappingHints? mappingHints = null) :
    ValueConverter<PostalCode, ulong>(
        ToProvider,
        FromProvider,
        mappingHints)
{
    private static Expression<Func<PostalCode, ulong>> ToProvider { get; } = m => (ulong)m;

    private static Expression<Func<ulong, PostalCode>> FromProvider { get; } = m => (PostalCode)m;
}
