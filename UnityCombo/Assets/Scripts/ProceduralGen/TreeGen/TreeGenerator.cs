
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.WSA;

[RequireComponent(typeof(TreeVolume), typeof(MeshFilter))]
public class TreeGenerator : MonoBehaviour
{
    TreeVolume treeVolume = null;

    List<Branch> tree = new List<Branch>();

    // Generator Variables
    [SerializeField]
    bool showGizmos = true;

    [SerializeField]
    int maxIter = 1000;

    [SerializeField]
    float branchLen = 0.2f;

    [SerializeField]
    float attractionRadius = 0.5f;

    [SerializeField]
    float destroyRadius = 0.3f;

    [SerializeField]
    float stepTime = 0.1f;

    [SerializeField]
    Vector3 startOffset = Vector3.zero;

    // Mesh variables
    [SerializeField]
    int cylinderSections = 6;


    [SerializeField]
    float endBranchRadius = 0.1f;

    [SerializeField]
    float reverseGrowthFactor = 1.5f;

    [SerializeField]
    Material mat = null;

    [HideInInspector]
    public Mesh treeMesh; // public for save in custom editor

    MeshFilter filter;

    Vector3 startPos = Vector3.zero;

    public void Setup()
    {
        startPos = startOffset + transform.position;

        treeVolume = GetComponent<TreeVolume>();

        if (treeVolume.points.Count != treeVolume.pointsNum)
            treeVolume.InitPoints();

        tree.Clear();

        tree.Add(new Branch(startPos, startPos + Vector3.up * branchLen, Vector3.up));
    }

    void KillAttractors()
    {
        List<Vector3> toKill = new List<Vector3>();

        foreach (Vector3 point in treeVolume.points)
        {
            foreach (Branch b in tree)
            {
                float d = Vector3.Distance(b.start, point);
                if (d < destroyRadius)
                {
                    toKill.Add(point);
                    break;
                }
            }
        }

        foreach(Vector3 point in toKill)
            treeVolume.points.Remove(point);
    }

    HashSet<Branch> SetBranchesToGrow()
    {
        HashSet<Branch> toGrow = new HashSet<Branch>();

        foreach (Branch b in tree)
            b.attractors.Clear();

        foreach (Vector3 point in treeVolume.points)
        {
            float minDist = 100000f;
            Branch closest = null;

            foreach (Branch b in tree) 
            {
                float d = Vector3.Distance(b.end, point);

                if (d < attractionRadius)
                {
                    if (d < minDist && d < attractionRadius)
                    {
                        closest = b;
                        minDist = d;
                    }
                }
            }

            if (closest != null)
            {
                closest.attractors.Add(point);
                if (!toGrow.Contains(closest))
                    toGrow.Add(closest);
            }
        }

        // If we find no branch to grow, we simply grow the extremities
        if (toGrow.Count <= 0)
            foreach (Branch b in tree)
                if (b.children.Count <= 0)
                    toGrow.Add(b);

        return toGrow;
    }

    public IEnumerator GenerateAsync()
    {
        for (int i = 0; i < maxIter && treeVolume.points.Count > 0; i++)
        {
            KillAttractors();

            HashSet<Branch> toGrow = SetBranchesToGrow();

            foreach (Branch b in toGrow)
            {
                Vector3 dir = Vector3.zero;

                // Extermities with no attractors found
                if (b.attractors.Count <= 0)
                {
                    dir = b.direction + Random.onUnitSphere * 0.05f;
                    dir.Normalize();

                    tree.Add(new Branch(b.end, b.end + dir * branchLen, dir, b));
                    continue;
                }

                // Branch with attractors
                foreach (Vector3 attractor in b.attractors)
                {
                    Vector3 endToAttr = (attractor - b.end);
                    dir += endToAttr.normalized;
                }

                dir /= b.attractors.Count;
                // This is here to add small natural randomness and avoid case when attractors cancel out
                dir += Random.onUnitSphere * 0.05f; 
                dir.Normalize();
                //Debug.Log("dir = " + dir + ", attractors: " + b.attractors.Count + ", mag: " + dir.magnitude);

                tree.Add(new Branch(b.end, b.end + dir * branchLen, dir, b));
            }

            Debug.Log("iter " + i + " out of " + maxIter);

            yield return new EditorWaitForSeconds(stepTime);
        }

        Debug.Log("mesh generation start");

        CreateMesh();

        Debug.Log("done");

        yield break;
    }

    void ComputeBranchSizes()
    {
        float inverseFactor = 1f / reverseGrowthFactor; // avoids computing on every branch

        // Start from the end, calculate current branch size based on child branches size
        for (int i = tree.Count - 1; i >= 0; i--) 
        {
            if (tree[i].children.Count <= 0)
            {
                tree[i].size = endBranchRadius;
                continue;
            }

            float sum = 0f;
            foreach (Branch b in tree[i].children)
                sum += Mathf.Pow(b.size, reverseGrowthFactor);

            tree[i].size = Mathf.Pow(sum, inverseFactor);
        }
    }

    public void CreateMesh()
    {
        filter = GetComponent<MeshFilter>();
        treeMesh = new Mesh();
        treeMesh.name = "Tree Mesh";

        ComputeBranchSizes();

        Vector3[] vertices = new Vector3[(tree.Count + 1) * cylinderSections];
        int[] triangles = new int[tree.Count * cylinderSections * 6];

        // vertices construction 
        for (int i = 0; i < tree.Count; i++)
        {
            Branch b = tree[i];

            // start index of this branch's vertices
            b.verticesId = cylinderSections * i;

            // quaternion to rotate the vertices along the branch direction
            Quaternion quat = Quaternion.FromToRotation(Vector3.up, b.direction);

            // construction of the vertices 
            for (int s = 0; s < cylinderSections; s++)
            {
                // radial angle of the vertex
                float alpha = ((float)s / cylinderSections) * Mathf.PI * 2f;

                // pos relative to vertical branch
                Vector3 pos = new Vector3(Mathf.Cos(alpha) * b.size, 0, Mathf.Sin(alpha) * b.size);
                pos = quat * pos; // rotation from branch to get position relative to actual branch
                pos += b.end; // translation to the end point of the branch, relative to local object
                // vertices must be aligned on global origin, we take it out of the local space
                vertices[b.verticesId + s] = pos - transform.position; 

                // if this is the tree root, vertices of the base are added at the end of the array 
                if (b.parent == null)
                {
                    vertices[tree.Count * cylinderSections + s] = b.start + new Vector3(Mathf.Cos(alpha) * b.size, 0, Mathf.Sin(alpha) * b.size) - transform.position;
                }
            }
        }

        // triangles construction
        for (int i = 0; i < tree.Count; i++)
        {
            int triId = i * cylinderSections * 6; // we have 6 index for 6 vertices spread on 2 triangles
            int topIdStart = tree[i].verticesId;
            int botIdStart = 0; 
            
            if (tree[i].parent != null)
                botIdStart = tree[i].parent.verticesId;
            else
                botIdStart = tree.Count * cylinderSections;

            // construction of the faces triangles
            for (int s = 0; s < cylinderSections; s++)
            {
                // the triangles 
                triangles[triId + s * 6] = botIdStart + s;
                triangles[triId + s * 6 + 1] = topIdStart + s;
                triangles[triId + s * 6 + 3] = botIdStart + s;

                if (s == cylinderSections - 1)
                {
                    // if last subdivision
                    triangles[triId + s * 6 + 2] = topIdStart;
                    triangles[triId + s * 6 + 4] = topIdStart;
                    triangles[triId + s * 6 + 5] = botIdStart;
                }
                else
                {
                    triangles[triId + s * 6 + 2] = topIdStart + s + 1;
                    triangles[triId + s * 6 + 4] = topIdStart + s + 1;
                    triangles[triId + s * 6 + 5] = botIdStart + s + 1;
                }
            }

        }
        
        // Set values in tree mesh
        treeMesh.vertices = vertices;
        treeMesh.triangles = triangles;
        treeMesh.RecalculateNormals();
        filter.mesh = treeMesh;

        if (mat != null)
        {
            Renderer renderer = GetComponent<Renderer>();

            renderer.sharedMaterial = mat;
        }
    }

    public void OnDrawGizmosSelected()
    {
        if (!showGizmos)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(startOffset + transform.position, new Vector3(0.15f, 0.15f, 0.15f));

        foreach (Branch branch in tree)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(branch.start, branchLen * 0.2f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(branch.start, branch.end);
        }
    }
}


[CustomEditor(typeof(TreeGenerator))]
class TreeGeneratorEditor : Editor
{
    EditorCoroutine genCoroutine;

    string assetName = "";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Generate tree"))
        {
            TreeGenerator gen = (TreeGenerator)target;

            gen.Setup();
            genCoroutine = EditorCoroutineUtility.StartCoroutine(gen.GenerateAsync(), gen);
            //gen.GenerateTree();
        }
        if (GUILayout.Button("Stop gen"))
        {
            TreeGenerator gen = (TreeGenerator)target;

            EditorCoroutineUtility.StopCoroutine(genCoroutine);
        }
        if (GUILayout.Button("Regen mesh"))
        {
            TreeGenerator gen = (TreeGenerator)target;

            gen.CreateMesh();
        }
        assetName = GUILayout.TextField(assetName);
        if (GUILayout.Button("Save mesh asset"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Meshes"))
                AssetDatabase.CreateFolder("Assets", "Meshes");

            if (!AssetDatabase.IsValidFolder("Assets/Meshes/TreeGen"))
                AssetDatabase.CreateFolder("Assets/Meshes", "TreeGen");

            TreeGenerator gen = (TreeGenerator)target;
            if (assetName == "")
                AssetDatabase.CreateAsset(gen.treeMesh, "Assets/Meshes/TreeGen/Tree" + Random.Range(0, 9999) + ".asset");
            else
                AssetDatabase.CreateAsset(gen.treeMesh, "Assets/Meshes/TreeGen/" + assetName + ".asset");

            AssetDatabase.SaveAssets();
        }
    }
}
