using System;
using UnityEngine;

[Serializable]
public class EQSLayer : ScriptableObject
{
#if UNITY_EDITOR
    [HideInInspector]
    public float execTime = 0f;
#endif

    public virtual EQS ExecuteLayer(EQS eqs = null)
    {
        return eqs;
    }
}
