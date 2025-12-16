using UnityEngine;
using System.Collections.Generic;

public class RemplissageLCA : MonoBehaviour
{
    public FenetrageManager manager;  // contient pointsDecoupes ou liste de polygones
    public Material remplissageMaterial;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        if (manager == null)
            manager = FindFirstObjectByType<FenetrageManager>();

        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

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
            RemplirPolygones(manager.pointsDecoupes);
        }
        else
        {
            meshFilter.mesh = null;
        }
    }

    // -----------------------------
    // REMPLISSAGE LCA avec winding number
    // -----------------------------
    void RemplirPolygones(List<Point> points)
    {
        // Convertir en Vector2
        List<Vector2> poly = new List<Vector2>();
        foreach (var p in points)
            poly.Add(p.VersVector3());

        // Triangulation Ear Clipping pour gérer tous les types de polygone
        List<int> triangles = Triangulate(poly);

        // Construire le mesh
        Vector3[] vertices = new Vector3[poly.Count];
        for (int i = 0; i < poly.Count; i++)
            vertices[i] = new Vector3(poly[i].x, poly[i].y, 0);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    // -----------------------------
    // TRIANGULATION EAR CLIPPING
    // -----------------------------
    List<int> Triangulate(List<Vector2> poly)
    {
        List<int> indices = new List<int>();
        List<int> verts = new List<int>();
        for (int i = 0; i < poly.Count; i++)
            verts.Add(i);

        int counter = 0;
        while (verts.Count > 3 && counter < 5000)
        {
            counter++;
            bool earFound = false;
            for (int i = 0; i < verts.Count; i++)
            {
                int i0 = verts[(i + verts.Count - 1) % verts.Count];
                int i1 = verts[i];
                int i2 = verts[(i + 1) % verts.Count];

                Vector2 a = poly[i0];
                Vector2 b = poly[i1];
                Vector2 c = poly[i2];

                if (IsConvex(a, b, c))
                {
                    bool contains = false;
                    for (int j = 0; j < verts.Count; j++)
                    {
                        int vi = verts[j];
                        if (vi == i0 || vi == i1 || vi == i2) continue;
                        if (PointInTriangle(poly[vi], a, b, c))
                        {
                            contains = true;
                            break;
                        }
                    }

                    if (!contains)
                    {
                        indices.Add(i0);
                        indices.Add(i1);
                        indices.Add(i2);
                        verts.RemoveAt(i);
                        earFound = true;
                        break;
                    }
                }
            }
            if (!earFound) break;
        }

        if (verts.Count == 3)
        {
            indices.Add(verts[0]);
            indices.Add(verts[1]);
            indices.Add(verts[2]);
        }

        return indices;
    }

    bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
    {
        return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) < 0;
    }

    bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float dX = p.x - c.x;
        float dY = p.y - c.y;
        float dX21 = c.x - b.x;
        float dY12 = b.y - c.y;
        float D = dY12 * (a.x - c.x) + dX21 * (a.y - c.y);
        float s = dY12 * dX + dX21 * dY;
        float t = (c.y - a.y) * dX + (a.x - c.x) * dY;
        if (D < 0) return s <= 0 && t <= 0 && s + t >= D;
        return s >= 0 && t >= 0 && s + t <= D;
    }
}
