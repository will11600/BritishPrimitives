namespace BritishPrimitives;

internal readonly struct Checksum(uint sevenDigitNumber)
{
    private const int Modulus = 97;
    private const int HighestWeight = 8;
    private const int LowestWeight = 2;
    private const int DecimalBase = 10;
    private const int Hmrc9755Offset = 55;

    private readonly int _remainder = CalculateWeightedRemainder(sevenDigitNumber);

    public int Standard => CalculateStandardChecksum(_remainder);

    public int Mod97 => Calculate9755Checksum(_remainder);

    private static int CalculateWeightedRemainder(uint sevenDigitNumber)
    {
        int total = 0;
        uint temp = sevenDigitNumber;

        for (int weight = HighestWeight; weight >= LowestWeight; weight--)
        {
            total += (int)(temp % DecimalBase) * weight;
            temp /= DecimalBase;
        }

        return total % Modulus;
    }

    private static int CalculateStandardChecksum(int remainder)
    {
        return Modulus - remainder;
    }

    private static int Calculate9755Checksum(int remainder)
    {
        var check = remainder > Hmrc9755Offset ? Modulus - remainder + Hmrc9755Offset : 42 - remainder;
        return check > 0 ? check : check + Modulus;
    }
}