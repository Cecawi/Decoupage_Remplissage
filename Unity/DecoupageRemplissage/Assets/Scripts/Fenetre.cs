using UnityEngine;
using System.Collections.Generic;

public class Fenetre
{
    public List<Point> sommets;
    public LineRenderer lrContour;
    public bool estFermee;
    public Color couleur;

    public Fenetre()
    {
        sommets = new List<Point>();
        estFermee = false;
        couleur = Color.blue;
    }

    public void InitialiserLineRenderer()
    {
        if (lrContour == null)
        {
            GameObject go = new GameObject("LR_Fenetre_" + GetHashCode());
            lrContour = go.AddComponent<LineRenderer>();
            ConfigurerLR(lrContour, couleur, 0.05f);
        }
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

    public void MettreAJourRenderer()
    {
        MettreAJourLR(lrContour, sommets, estFermee);
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
        if (lrContour != null) Object.Destroy(lrContour.gameObject);
    }
}
