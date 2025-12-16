using UnityEngine;
using System.Collections.Generic;

public class Polygone
{
    public List<Point> sommets;
    public List<List<Point>> resultatsDecoupes; // Changé en liste de listes
    public bool estFerme;
    public Color couleur;

    private LineRenderer lrContour;
    private List<LineRenderer> lrDecoupes; // Liste de LineRenderers pour les résultats
    private Transform transformParent;

    public Polygone()
    {
        sommets = new List<Point>();
        resultatsDecoupes = new List<List<Point>>();
        estFerme = false;
        couleur = Color.green;
        lrDecoupes = new List<LineRenderer>();
    }

    public void InitialiserLineRenderers()
    {
        GameObject goParent = new GameObject("Polygone_" + GetHashCode());
        transformParent = goParent.transform;

        GameObject goContour = new GameObject("LR_Contour");
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
        
        // Gérer le pool de LineRenderers pour les résultats
        int besoin = resultatsDecoupes.Count;
        
        // Créer des nouveaux si nécessaire
        while (lrDecoupes.Count < besoin)
        {
            GameObject go = new GameObject("LR_Decoupe_" + lrDecoupes.Count);
            go.transform.SetParent(transformParent);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            ConfigurerLR(lr, Color.red, 0.1f);
            lrDecoupes.Add(lr);
        }

        // Mettre à jour ceux utilisés
        for (int i = 0; i < besoin; i++)
        {
            MettreAJourLR(lrDecoupes[i], resultatsDecoupes[i], true);
            lrDecoupes[i].enabled = true;
        }

        // Cacher ceux inutilisés
        for (int i = besoin; i < lrDecoupes.Count; i++)
        {
            lrDecoupes[i].positionCount = 0;
            lrDecoupes[i].enabled = false;
        }
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

    // Mise à jour de SetHighlight pour gérer la liste
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
            foreach(var lr in lrDecoupes)
            {
                lr.widthMultiplier = 0.2f;
                lr.startColor = Color.yellow;
                lr.endColor = Color.yellow;
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
            foreach(var lr in lrDecoupes)
            {
                lr.widthMultiplier = 0.1f;
                lr.startColor = Color.red;
                lr.endColor = Color.red;
            }
        }
    }

    public void Detruire()
    {
        if (transformParent != null) Object.Destroy(transformParent.gameObject);
    }
}