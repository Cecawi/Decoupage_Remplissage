using UnityEngine;
using System.Collections.Generic;

public class Fenetre
{
    public List<Point> sommets;
    public bool estFermee;
    public Color couleur;

    private LineRenderer lrContour;
    private List<LineRenderer> lrTriangles;
    private Transform transformParent;

    public List<List<Point>> trianglesCache;

    public Fenetre()
    {
        sommets = new List<Point>();
        estFermee = false;
        couleur = Color.blue;
        lrTriangles = new List<LineRenderer>();
        trianglesCache = new List<List<Point>>();
    }

    public void InitialiserLineRenderer()
    {
        GameObject goParent = new GameObject("Fenetre_" + GetHashCode());
        transformParent = goParent.transform;

        GameObject goContour = new GameObject("LR_Fenetre_Contour");
        goContour.transform.SetParent(transformParent);
        lrContour = goContour.AddComponent<LineRenderer>();
        ConfigurerLR(lrContour, couleur, 0.05f);
    }

    private void ConfigurerLR(LineRenderer lr, Color col, float width)
    {
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.widthMultiplier = width;
        lr.loop = false;
        lr.positionCount = 0;
        lr.startColor = col;
        lr.endColor = col;
        lr.sortingOrder = 1;
    }

    public void MettreAJourRenderer(bool afficherTriangulation)
    {
        MettreAJourLR(lrContour, sommets, estFermee);

        if (afficherTriangulation && trianglesCache != null && trianglesCache.Count > 0)
        {
            // Gérer le pool de LineRenderers pour les triangles
            int besoin = trianglesCache.Count;
            
            while (lrTriangles.Count < besoin)
            {
                GameObject go = new GameObject("LR_Triangle_" + lrTriangles.Count);
                go.transform.SetParent(transformParent);
                LineRenderer lr = go.AddComponent<LineRenderer>();
                
                // Bleu plus clair/fin pour la triangulation
                ConfigurerLR(lr, new Color(0.2f, 0.2f, 1f, 0.3f), 0.02f);
                lr.sortingOrder = 1;
                lrTriangles.Add(lr);
            }

            for (int i = 0; i < besoin; i++)
            {
                MettreAJourLR(lrTriangles[i], trianglesCache[i], true);
                lrTriangles[i].enabled = true;
            }

            for (int i = besoin; i < lrTriangles.Count; i++)
            {
                lrTriangles[i].positionCount = 0;
                lrTriangles[i].enabled = false;
            }
        }
        else
        {
            foreach (var lr in lrTriangles) 
            {
                lr.positionCount = 0;
                lr.enabled = false;
            }
        }
    }

    public void CheckConcavite()
    {
        if (estFermee && sommets.Count >= 3)
        {
            trianglesCache = Triangulation.Trianguler(sommets);
        }
        else
        {
            trianglesCache.Clear();
        }
    }

    public bool EstConcave()
    {
        // Simple heuristic: if triangulation returns > (N-2) triangles? No, triangulation always N-2.
        // Actually we just need to know if we have a valid cache, 
        // Triangulation works for Convex too.
        // We can check if any vertex reflex?
        // For simpler UI logic: "ExisteFenetreConcave" will just assume any window with >3 points *could* be concave for the sake of the button,
        // OR we specifically detect concavity.
        // Let's use specific Convex check on vertices.
        return !EstConvexe(sommets);
    }

    private bool EstConvexe(List<Point> poly)
    {
        if (poly.Count < 3) return false;
        if (poly.Count == 3) return true;

        bool? sign = null;
        for(int i=0; i<poly.Count; i++)
        {
            Point p1 = poly[i];
            Point p2 = poly[(i+1)%poly.Count];
            Point p3 = poly[(i+2)%poly.Count];
            
            float cross = (p2.x - p1.x)*(p3.y - p2.y) - (p2.y - p1.y)*(p3.x - p2.x);
            if(Mathf.Abs(cross) < 0.0001f) continue;

            bool currentSign = cross > 0;
            if(sign == null) sign = currentSign;
            else if(sign != currentSign) return false;
        }
        return true;
    }

    private void MettreAJourLR(LineRenderer lr, List<Point> points, bool fermer)
    {
        if (lr == null || points == null || points.Count == 0)
        {
            if (lr != null) lr.positionCount = 0;
            return;
        }

        lr.positionCount = points.Count;
        lr.loop = fermer && points.Count >= 3;

        for (int i = 0; i < points.Count; i++)
        {
            lr.SetPosition(i, points[i].VersVector3());
        }
    }


    public void SetHighlight(bool highlight)
    {
        if (lrContour != null)
        {
            float width = highlight ? 0.15f : 0.05f;
            Color col = highlight ? Color.yellow : couleur;
            lrContour.widthMultiplier = width;
            lrContour.startColor = col;
            lrContour.endColor = col;
        }
    }

    public void Detruire()
    {
        if (transformParent != null) Object.Destroy(transformParent.gameObject);
    }
}
