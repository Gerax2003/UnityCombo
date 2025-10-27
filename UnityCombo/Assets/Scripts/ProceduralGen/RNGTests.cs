
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RNGTests : MonoBehaviour
{
    [SerializeField]
    int rangeMin = 0;
    [SerializeField]
    int rangeMax = 100;
    [SerializeField]
    int iterations = 100000;
    [SerializeField]
    int fRangeMin = 0;
    [SerializeField]
    int fRangeMax = 100;

    // Start is called before the first frame update
    void Start()
    {
        #region INT TEST
        {
            RNG.SetSeed(2983);
            int[] results = new int[iterations];

            for (int i = 0; i < iterations; i++)
                results[i] = RNG.Rand(rangeMin, rangeMax);

            RNG.SetSeed(2983);

            int[] results2 = new int[iterations];

            for (int i = 0; i < iterations; i++)
                results2[i] = RNG.Rand(rangeMin, rangeMax);

            bool equalResults = results.SequenceEqual(results2);

            Debug.Log("Algorithm is deterministic: " + equalResults);

            Dictionary<int, int> dict = new Dictionary<int, int>();

            for (int i = rangeMin; i < rangeMax; ++i)
                dict.Add(i, 0);

            for (int i = 0; i < results.Length; ++i)
            {
                dict[results[i]]++;
            }

            KeyValuePair<int, int> pairMin = new KeyValuePair<int, int>(0, results.Length);
            KeyValuePair<int, int> pairMax = new KeyValuePair<int, int>(0, 0);

            for (int i = rangeMin; i < rangeMax; ++i)
            {
                if (dict[i] < pairMin.Value)
                    pairMin = new KeyValuePair<int, int>(i, dict[i]);
                else if (dict[i] > pairMax.Value)
                    pairMax = new KeyValuePair<int, int>(i, dict[i]);
            }

            double avg = dict.Values.Average();

            Debug.Log(pairMin + ", " + pairMax + "\n" + avg + ", [" + dict.Keys.Min() + ", " + dict.Keys.Max() + "]");
        }
        #endregion

        #region FLOAT TEST
        {
            RNG.SetSeed(7399);

            float[] resultsF = new float[iterations];

            for (int i = 0; i < iterations; i++)
                resultsF[i] = RNG.Rand(fRangeMin, fRangeMax);

            RNG.SetSeed(7399);

            float[] resultsF2 = new float[iterations];

            for (int i = 0; i < iterations; i++)
                resultsF2[i] = RNG.Rand(fRangeMin, fRangeMax);


            bool equalResults = resultsF.SequenceEqual(resultsF2);

            Debug.Log("Algorithm is deterministic: " + equalResults);

            Dictionary<float, int> dict = new Dictionary<float, int>();

            for (int i = rangeMin; i < rangeMax; ++i)
                dict.Add(i, 0);

            for (int i = 0; i < resultsF.Length; ++i)
            {
                dict[resultsF[i]]++;
            }

            KeyValuePair<float, int> pairMin = new KeyValuePair<float, int>(0, resultsF.Length);
            KeyValuePair<float, int> pairMax = new KeyValuePair<float, int>(0, 0);

            for (int i = rangeMin; i < rangeMax; ++i)
            {
                if (dict[i] < pairMin.Value)
                    pairMin = new KeyValuePair<float, int>(i, dict[i]);
                else if (dict[i] > pairMax.Value)
                    pairMax = new KeyValuePair<float, int>(i, dict[i]);
            }

            double avg = dict.Values.Average();

            Debug.Log(pairMin + ", " + pairMax + "\n" + avg + ", [" + dict.Keys.Min() + ", " + dict.Keys.Max() + "]");
        }
        #endregion
    }

}
