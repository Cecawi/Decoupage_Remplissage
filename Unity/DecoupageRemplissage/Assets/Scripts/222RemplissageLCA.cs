using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum RegleRemplissage { ParImpair, Enroulement }

public class RemplissageLCA2 : MonoBehaviour
{
    public FenetrageManager manager;
    public Material remplissageMaterial;
    public RegleRemplissage regle = RegleRemplissage.ParImpair;
    [Range(0.01f, 0.5f)] public float resolution = 0.1f;

    private List<MeshFilter> meshFilters = new List<MeshFilter>();

    class Edge
    {
        public float yMax;
        public float xCurr;
        public float invM;
        public int direction; // +1 si monte, -1 si descend (pour l'enroulement)

        public Edge(Vector2 p1, Vector2 p2)
        {
            // Déterminer la direction avant de trier par Y
            direction = (p2.y > p1.y) ? 1 : -1;

            if (p1.y > p2.y) { var tmp = p1; p1 = p2; p2 = tmp; }
            yMax = p2.y;
            xCurr = p1.x;
            invM = (p2.x - p1.x) / (p2.y - p1.y);
        }
    }

    private void Update()
    {
        if (manager == null || manager.listePolygones == null) return;

        int nbPolys = manager.listePolygones.Count;
        while (meshFilters.Count < nbPolys)
        {
            GameObject go = new GameObject("LCA_Render_" + meshFilters.Count);
            go.transform.SetParent(transform);
            MeshFilter mf = go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>().material = remplissageMaterial;
            meshFilters.Add(mf);
        }

        for (int i = 0; i < nbPolys; i++)
        {
            var poly = manager.listePolygones[i];
            if (poly.resultatsDecoupes != null && poly.resultatsDecoupes.Count > 0)
                ExecuterLCA(meshFilters[i], poly.resultatsDecoupes);
            else
                meshFilters[i].mesh = null;
        }
    }

    void ExecuterLCA(MeshFilter mf, List<List<Point>> morceaux)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        foreach (var morceau in morceaux)
        {
            if (morceau.Count < 3) continue;
            List<Vector2> pts = morceau.Select(p => new Vector2(p.x, p.y)).ToList();

            float yMin = pts.Min(p => p.y);
            float yMax = pts.Max(p => p.y);

            // 1. SI (Structure Intermédiaire)
            Dictionary<float, List<Edge>> SI = new Dictionary<float, List<Edge>>();
            for (int i = 0; i < pts.Count; i++)
            {
                Vector2 p1 = pts[i];
                Vector2 p2 = pts[(i + 1) % pts.Count];
                if (Mathf.Abs(p1.y - p2.y) > 0.0001f)
                {
                    Edge e = new Edge(p1, p2);
                    float yStart = Mathf.Ceil(Mathf.Min(p1.y, p2.y) / resolution) * resolution;
                    if (!SI.ContainsKey(yStart)) SI[yStart] = new List<Edge>();
                    SI[yStart].Add(e);
                }
            }

            // 2. LCA (Liste des Côtés Actifs)
            List<Edge> LCA = new List<Edge>();
            for (float y = yMin; y <= yMax; y += resolution)
            {
                float key = Mathf.Round(y / resolution) * resolution;
                if (SI.ContainsKey(key)) LCA.AddRange(SI[key]);
                LCA.RemoveAll(e => e.yMax < y);
                LCA.Sort((a, b) => a.xCurr.CompareTo(b.xCurr));

                if (regle == RegleRemplissage.ParImpair)
                {
                    // Règle Par-Impair (Intersection count)
                    for (int i = 0; i < LCA.Count; i += 2)
                    {
                        if (i + 1 < LCA.Count) CreerBande(LCA[i].xCurr, LCA[i + 1].xCurr, y, vertices, triangles);
                    }
                }
                else
                {
                    // Règle Enroulement (Winding Number)
                    int winding = 0;
                    for (int i = 0; i < LCA.Count - 1; i++)
                    {
                        winding += LCA[i].direction;
                        if (winding != 0) // On remplit si l'enroulement n'est pas nul
                        {
                            CreerBande(LCA[i].xCurr, LCA[i + 1].xCurr, y, vertices, triangles);
                        }
                    }
                }
                foreach (var e in LCA) e.xCurr += e.invM * resolution;
            }
        }

        Mesh m = new Mesh();
        m.vertices = vertices.ToArray();
        m.triangles = triangles.ToArray();
        mf.mesh = m;
    }

    void CreerBande(float x1, float x2, float y, List<Vector3> v, List<int> t)
    {
        int idx = v.Count;
        v.Add(new Vector3(x1, y, 0));
        v.Add(new Vector3(x2, y, 0));
        v.Add(new Vector3(x2, y + resolution, 0));
        v.Add(new Vector3(x1, y + resolution, 0));
        t.Add(idx); t.Add(idx + 2); t.Add(idx + 1);
        t.Add(idx); t.Add(idx + 3); t.Add(idx + 2);
    }
}