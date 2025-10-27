
using System.Collections.Generic;
using UnityEngine;

public class Branch
{
    public Vector3 start = Vector3.zero;
    public Vector3 end = Vector3.zero;
    public Vector3 direction = Vector3.up;
    public Branch parent = null;
    public List<Branch> children = new List<Branch>();
	public List<Vector3> attractors = new List<Vector3>();
    public int depth = 0;
    public int verticesId = 0;
    public float size = 0.1f;

	public Branch(Vector3 inStart, Vector3 inEnd, Vector3 inDirection, Branch inParent = null)
    {
        start = inStart;
        end = inEnd;
        direction = inDirection;
        parent = inParent;

        if (parent != null)
        {
            parent.children.Add(this);
            depth = parent.depth + 1;
        }
    }
}
