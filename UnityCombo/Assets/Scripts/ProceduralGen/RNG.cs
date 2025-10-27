
using UnityEngine;

public enum RandomTypes
{
    LCG = 0,
    NONE,
}

public static class RNG 
{
    private static int iterations = 5;

    private static int seed = 0;

    private static RandomTypes randomType = RandomTypes.LCG;

    public static void SetSeed(int inSeed)
    {
        seed = inSeed;
    }

    public static void SetRandomType(RandomTypes type)
    {
        randomType = type;
    }

    public static int Rand(int rangeMin = 0, int rangeMax = 65536)
    {
        int val = 0;

        switch (randomType)
        {
            case RandomTypes.LCG:
                val = LCG();
                break;
            default:
                val = LCG();
                break;
        }

        // bring val in range
        return (val % (rangeMax - rangeMin)) + rangeMin;
    }

    public static float Rand(float rangeMin = 0, float rangeMax = 65536)
    {
        int val = 0;

        switch (randomType)
        {
            case RandomTypes.LCG:
                val = LCG();
                break;
            default:
                val = LCG();
                break;
        }

        // divide by 2 because I think intMax is poorly converted to float
        float hVal = val / 2f;

        // get val in range 0-1
        float n = hVal / (int.MaxValue / 2f);

        // bring val in range min-max
        return (n * (rangeMax - rangeMin)) + rangeMin;
    }


    #region LCG
    // Values taken from MSVC standard, according to 
    // https://en.wikipedia.org/wiki/Linear_congruential_generator#Parameters_in_common_use
    private static int lcgA = 214013;
    private static int lcgM = int.MaxValue;
    private static int lcgC = 2531011;

    public static int LCG()
    { 
        for (int i = 0; i < iterations; i++)
        {
            seed = Mathf.Abs((lcgA * seed + lcgC) % lcgM); // Abs here because Rand needs a positive number to function properly
        }

        return seed;
    }
#endregion
}
