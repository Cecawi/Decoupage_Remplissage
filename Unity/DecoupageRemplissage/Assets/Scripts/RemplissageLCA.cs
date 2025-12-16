using UnityEngine;
using System.Collections.Generic;

public class RemplissageLCA : MonoBehaviour
{
    public FenetrageManager manager;
    public Material remplissageMaterial;

    private List<MeshFilter> meshFilters;
    private List<MeshRenderer> meshRenderers;

    private void Awake()
    {
        if (manager == null)
            manager = FindFirstObjectByType<FenetrageManager>();

        meshFilters = new List<MeshFilter>();
        meshRenderers = new List<MeshRenderer>();

        if (remplissageMaterial == null)
        {
            remplissageMaterial = new Material(Shader.Find("Sprites/Default"));
            remplissageMaterial.color = new Color(1f, 0f, 0f, 0.5f);
        }
    }

    private void Update()
    {
        if (manager != null && manager.listePolygones != null)
        {
            int nbPolygones = manager.listePolygones.Count;

            while (meshFilters.Count < nbPolygones)
            {
                GameObject go = new GameObject("Remplissage_" + meshFilters.Count);
                go.transform.SetParent(transform);
                
                MeshFilter mf = go.AddComponent<MeshFilter>();
                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                mr.material = remplissageMaterial;

                meshFilters.Add(mf);
                meshRenderers.Add(mr);
            }

            while (meshFilters.Count > nbPolygones)
            {
                int lastIdx = meshFilters.Count - 1;
                if (meshFilters[lastIdx] != null)
                {
                    Destroy(meshFilters[lastIdx].gameObject);
                }
                meshFilters.RemoveAt(lastIdx);
                meshRenderers.RemoveAt(lastIdx);
            }

            for (int i = 0; i < nbPolygones; i++)
            {
                Polygone poly = manager.listePolygones[i];
                if (poly.sommetsDecoupes != null && poly.sommetsDecoupes.Count >= 3)
                {
                    RemplirPolygone(meshFilters[i], poly.sommetsDecoupes);
                }
                else
                {
                    meshFilters[i].mesh = null;
                }
            }
        }
    }

    private void RemplirPolygone(MeshFilter mf, List<Point> points)
    {
        //convertir en Vector2
        List<Vector2> poly = new List<Vector2>();
        foreach (Point p in points)
            poly.Add(new Vector2(p.x, p.y));

        //triangulation LCA
        List<int> triangles = TriangulerLCA(poly);

        //création mesh
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[points.Count];
        for (int i = 0; i < points.Count; i++)
            vertices[i] = points[i].VersVector3();

        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        mf.mesh = mesh;
    }

    private List<int> TriangulerLCA(List<Vector2> polygon)
    {
        List<int> triangles = new List<int>();
        
        if (polygon.Count < 3) return triangles;

        //algorithme LCA (winding number) simplifié
        //pour l'instant on utilise ear clipping basique
        List<Vector2> temp = new List<Vector2>(polygon);
        
        while (temp.Count > 3)
        {
            bool earFound = false;
            for (int i = 0; i < temp.Count; i++)
            {
                int prev = (i - 1 + temp.Count) % temp.Count;
                int next = (i + 1) % temp.Count;

                if (EstConvexe(temp[prev], temp[i], temp[next]))
                {
                    bool isEar = true;
                    for (int j = 0; j < temp.Count; j++)
                    {
                        if (j == prev || j == i || j == next) continue;
                        if (PointDansTriangle(temp[j], temp[prev], temp[i], temp[next]))
                        {
                            isEar = false;
                            break;
                        }
                    }

                    if (isEar)
                    {
                        int idx_prev = polygon.IndexOf(temp[prev]);
                        int idx_i = polygon.IndexOf(temp[i]);
                        int idx_next = polygon.IndexOf(temp[next]);

                        triangles.Add(idx_prev);
                        triangles.Add(idx_i);
                        triangles.Add(idx_next);

                        temp.RemoveAt(i);
                        earFound = true;
                        break;
                    }
                }
            }
            if (!earFound) break;
        }

        if (temp.Count == 3)
        {
            triangles.Add(polygon.IndexOf(temp[0]));
            triangles.Add(polygon.IndexOf(temp[1]));
            triangles.Add(polygon.IndexOf(temp[2]));
        }

        return triangles;
    }

    private bool EstConvexe(Vector2 a, Vector2 b, Vector2 c)
    {
        return ProduitVectoriel(a, b, c) > 0;
    }

    private float ProduitVectoriel(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    }

    private bool PointDansTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = ProduitVectoriel(a, b, p);
        float d2 = ProduitVectoriel(b, c, p);
        float d3 = ProduitVectoriel(c, a, p);

        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(hasNeg && hasPos);
    }
}
