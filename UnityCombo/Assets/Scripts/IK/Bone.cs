using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bone : MonoBehaviour
{
    [HideInInspector]
    public Bone childBone = null;
    [HideInInspector]
    public Bone parentBone = null;

    [HideInInspector]
    public float length = 0.0001f; // not 0 to avoid potential issues

    public Vector3 jointAxis = Vector3.zero;
    public float jointMinLimits = -360f;
    public float jointMaxLimits = 360f;

    // Start is called before the first frame update
    void Awake()
    {
        if (jointAxis != Vector3.zero) 
            jointAxis.Normalize();

        parentBone = transform.parent.GetComponent<Bone>();

        Bone[] children = GetComponentsInChildren<Bone>();
    
        foreach (Bone b in children) 
        {
            if (b != this && b.transform.IsChildOf(transform))
            {
                childBone = b;
                length = (childBone.transform.position - transform.position).magnitude;
                return;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
