using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RemplissageLCA : MonoBehaviour
{
    public FenetrageManager manager;
    public Material remplissageMaterial;

    private List<MeshFilter> meshFilters = new List<MeshFilter>();
    private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();

    //[cite_start]// Maillon pour la Liste des Côtés Actifs (LCA) [cite: 316, 319]
    class Edge
    {
        public float yMax;    // Ordonnée maximale [cite: 321]
        public float xCurr;   // Abscisse courante [cite: 322]
        public float invM;    // 1/m [cite: 323]

        public Edge(Vector2 p1, Vector2 p2)
        {
            if (p1.y > p2.y) { var tmp = p1; p1 = p2; p2 = tmp; }
            yMax = p2.y;
            xCurr = p1.x;
            invM = (p2.x - p1.x) / (p2.y - p1.y);
        }
    }

    private void Awake()
    {
        if (manager == null) manager = FindFirstObjectByType<FenetrageManager>();
        if (remplissageMaterial == null)
        {
            remplissageMaterial = new Material(Shader.Find("Sprites/Default"));
            remplissageMaterial.color = new Color(1f, 0f, 0f, 0.5f);
        }
    }

    private void Update()
    {
        if (manager == null || manager.listePolygones == null) return;

        int nbPolygones = manager.listePolygones.Count;
        AjusterPoolObjets(nbPolygones);

        for (int i = 0; i < nbPolygones; i++)
        {
            Polygone poly = manager.listePolygones[i];
          //  [cite_start]// On traite tous les morceaux issus du découpage 
            if (poly.resultatsDecoupes != null && poly.resultatsDecoupes.Count > 0)
            {
                GenererRemplissagePourPolygone(meshFilters[i], poly.resultatsDecoupes);
            }
            else
            {
                meshFilters[i].mesh = null;
            }
        }
    }

    private void AjusterPoolObjets(int besoin)
    {
        while (meshFilters.Count < besoin)
        {
            GameObject go = new GameObject("Remplissage_LCA_" + meshFilters.Count);
            go.transform.SetParent(transform);
            meshFilters.Add(go.AddComponent<MeshFilter>());
            var mr = go.AddComponent<MeshRenderer>();
            mr.material = remplissageMaterial;
            meshRenderers.Add(mr);
        }
        while (meshFilters.Count > besoin)
        {
            int idx = meshFilters.Count - 1;
            Destroy(meshFilters[idx].gameObject);
            meshFilters.RemoveAt(idx);
            meshRenderers.RemoveAt(idx);
        }
    }

    void GenererRemplissagePourPolygone(MeshFilter mf, List<List<Point>> morceaux)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        foreach (var morceau in morceaux)
        {
            if (morceau.Count < 3) continue;
            List<Vector2> points = morceau.Select(p => new Vector2(p.x, p.y)).ToList();
            CalculerLCA(points, vertices, triangles);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mf.mesh = mesh;
    }

    // [cite_start]// Algorithme LCA pur selon le cours [cite: 418, 427]

    void CalculerLCA(List<Vector2> poly, List<Vector3> v, List<int> t)
    {
        float yMin = poly.Min(p => p.y);
        float yMax = poly.Max(p => p.y);

        // RÉSOLUTON : Plus cette valeur est petite, plus c'est précis (0.1 = 10 lignes par unité)
        float step = 0.05f;

        Dictionary<float, List<Edge>> SI = new Dictionary<float, List<Edge>>();
        for (int i = 0; i < poly.Count; i++)
        {
            Vector2 p1 = poly[i];
            Vector2 p2 = poly[(i + 1) % poly.Count];
            if (Mathf.Abs(p1.y - p2.y) > 0.001f)
            {
                Edge e = new Edge(p1, p2);
                // On cale le départ sur le multiple de 'step' le plus proche
                float yStart = Mathf.Ceil(Mathf.Min(p1.y, p2.y) / step) * step;
                if (!SI.ContainsKey(yStart)) SI[yStart] = new List<Edge>();
                SI[yStart].Add(e);
            }
        }

        List<Edge> LCA = new List<Edge>();

        for (float y = yMin; y <= yMax; y += step)
        {
            // On arrondit la clé pour éviter les erreurs de float
            float key = Mathf.Round(y / step) * step;
            if (SI.ContainsKey(key)) LCA.AddRange(SI[key]);

            LCA.RemoveAll(e => e.yMax < y);

            if (LCA.Count >= 2)
            {
                LCA.Sort((a, b) => a.xCurr.CompareTo(b.xCurr));

                for (int i = 0; i < LCA.Count; i += 2)
                {
                    if (i + 1 < LCA.Count)
                    {
                        int idx = v.Count;
                        // Les 4 sommets du maillage pour cette petite bande
                        v.Add(new Vector3(LCA[i].xCurr, y, 0));
                        v.Add(new Vector3(LCA[i + 1].xCurr, y, 0));
                        v.Add(new Vector3(LCA[i + 1].xCurr, y + step, 0));
                        v.Add(new Vector3(LCA[i].xCurr, y + step, 0));

                        t.Add(idx); t.Add(idx + 2); t.Add(idx + 1);
                        t.Add(idx); t.Add(idx + 3); t.Add(idx + 2);
                    }
                }
            }
            // Mise à jour : on ajoute (1/m * step) pour rester proportionnel à la résolution
            foreach (var e in LCA) e.xCurr += e.invM * step;
        }
    }
}