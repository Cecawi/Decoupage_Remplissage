using UnityEngine;
using System.Collections.Generic;

public class Remplissage : MonoBehaviour
{
    public FenetrageManager manager;
    public Material remplissageMaterial;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        if (manager == null)
        {
            manager = FindObjectOfType<FenetrageManager>();
        }

        meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        if (remplissageMaterial == null)
        {
            remplissageMaterial = new Material(Shader.Find("Sprites/Default"));
            remplissageMaterial.color = new Color(1f, 0f, 0f, 0.5f);
        }

        meshRenderer.material = remplissageMaterial;
    }

    private void Update()
    {
        if (manager != null && manager.pointsDecoupes != null && manager.pointsDecoupes.Count >= 3)
        {
            RemplirPolygone(manager.pointsDecoupes);
        }
        else
        {
            meshFilter.mesh = null;
        }
    }

    private void RemplirPolygone(List<Point> points)
    {
        Mesh mesh = new Mesh();
        int n = points.Count;

        Vector3[] vertices = new Vector3[n];
        for (int i = 0; i < n; i++)
        {
            vertices[i] = points[i].VersVector3();
        }

        int[] triangles = GenererTrianglesFan(n);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    private int[] GenererTrianglesFan(int n)
    {
        if (n < 3) return new int[0];

        int nbTriangles = n - 2;
        int[] triangles = new int[nbTriangles * 3];

        for (int i = 0; i < nbTriangles; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        return triangles;
    }
}