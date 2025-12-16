using UnityEngine;
using System.Collections.Generic;

public class Polygone
{
    public List<Point> sommets;
    public List<Point> sommetsDecoupes;
    public LineRenderer lrContour;
    public LineRenderer lrDecoupe;
    public bool estFerme;
    public Color couleur;

    public Polygone()
    {
        sommets = new List<Point>();
        sommetsDecoupes = new List<Point>();
        estFerme = false;
        couleur = Color.green;
    }

    public void InitialiserLineRenderers()
    {
        if (lrContour == null)
        {
            GameObject go = new GameObject("LR_Polygone_" + GetHashCode());
            lrContour = go.AddComponent<LineRenderer>();
            ConfigurerLR(lrContour, couleur, 0.05f);
        }

        if (lrDecoupe == null)
        {
            GameObject go = new GameObject("LR_Decoupe_" + GetHashCode());
            lrDecoupe = go.AddComponent<LineRenderer>();
            ConfigurerLR(lrDecoupe, Color.red, 0.1f);
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
        
        //ordre de rendu: rouge au-dessus
        if (col == Color.red)
        {
            lr.sortingOrder = 2;
        }
        else
        {
            lr.sortingOrder = 0;
        }
    }

    public void MettreAJourRenderers()
    {
        MettreAJourLR(lrContour, sommets, estFerme);
        MettreAJourLR(lrDecoupe, sommetsDecoupes, true);
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
        if (highlight)
        {
            if (lrContour != null)
            {
                lrContour.widthMultiplier = 0.15f;
                lrContour.startColor = Color.yellow;
                lrContour.endColor = Color.yellow;
            }
            if (lrDecoupe != null)
            {
                lrDecoupe.widthMultiplier = 0.2f;
                lrDecoupe.startColor = Color.yellow;
                lrDecoupe.endColor = Color.yellow;
            }
        }
        else
        {
            if (lrContour != null)
            {
                lrContour.widthMultiplier = 0.05f;
                lrContour.startColor = couleur;
                lrContour.endColor = couleur;
            }
            if (lrDecoupe != null)
            {
                lrDecoupe.widthMultiplier = 0.1f;
                lrDecoupe.startColor = Color.red;
                lrDecoupe.endColor = Color.red;
            }
        }
    }

    public void Detruire()
    {
        if (lrContour != null) Object.Destroy(lrContour.gameObject);
        if (lrDecoupe != null) Object.Destroy(lrDecoupe.gameObject);
    }
}