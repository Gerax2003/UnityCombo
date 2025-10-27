
using System.Collections.Generic;
using UnityEngine;

public enum FilterType: int
{
    FILTER,
    SCORING
}

public class Filter : EQSLayer
{
    public FilterType filterType = FilterType.SCORING;

    public bool inverseCondition = false;

    public Transform targetTransform = null;

    public Filter(FilterType filterType = FilterType.SCORING, bool inverseCondition = false)
    {
        this.filterType = filterType;
        this.inverseCondition = inverseCondition;
    }

    // Default behavior simply clamps all values to range 0-1, may be useful later
    public override EQS ExecuteLayer(EQS eqs) 
    {
        // copy of the dict keys because you can't change a dictionnary values in a loop referencing it (xDDDDDD)
        List<Vector3> keys = new List<Vector3>(eqs.data.Keys);
        foreach (Vector3 key in keys)
            eqs.data[key] = Mathf.Clamp(eqs.data[key], 0f, 1f);

        return eqs;
    }
}
