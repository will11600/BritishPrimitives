using System.Runtime.CompilerServices;

namespace BritishPrimitives.Text;

internal static class Character
{
    public const char UppercaseA = 'A';
    public const char UppercaseZ = 'Z';
    public const char LowercaseA = 'a';
    public const char LowercaseZ = 'z';
    public const char UppercaseD = 'D';
    public const char LowercaseD = 'd';
    public const char Zero = '0';
    public const char Nine = '9';
    public const char Whitespace = ' ';

    private static readonly CharacterRange _uppercaseAToZ = new(UppercaseA, UppercaseZ);
    public static ref readonly CharacterRange UppercaseAToZ => ref _uppercaseAToZ;

    private static readonly CharacterRange _lowercaseAToZ = new(LowercaseA, LowercaseZ);
    public static ref readonly CharacterRange LowercaseAToZ => ref _lowercaseAToZ;

    private static readonly CharacterRange _zeroToNine = new(Zero, Nine);
    public static ref readonly CharacterRange ZeroToNine => ref _zeroToNine;
}