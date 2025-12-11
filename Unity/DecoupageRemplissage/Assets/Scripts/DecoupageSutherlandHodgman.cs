using UnityEngine;
using System.Collections.Generic;

public class DecoupageSutherlandHodgman
{
    public List<Point> PW;//liste des sommets de la fenêtre
    public List<Point> PL;//liste des sommets du polygone d'entrée
    public List<Point> PS;//liste temporaire pour le polygone découpé (sortie)

    public Point S;//point courant / précédent
    public Point F;//premier sommet du polygone courant
    public Point I;//point d'intersection

    public int N1;//nombre de sommets du polygone d'entrée
    public int N2;//nombre de sommets du polygone de sortie

    private float orientationFenetre;

    public DecoupageSutherlandHodgman(List<Point> fenetre, List<Point> polygone)
    {
        PW = fenetre;
        PL = new List<Point>(polygone);
        PS = new List<Point>();

        N1 = PL.Count;
        N2 = 0;

        CalculerOrientationFenetre();
    }

    private void CalculerOrientationFenetre()
    {
        if(PW == null || PW.Count < 3)
        {
            orientationFenetre = 0.0f;
            return;
        }

        Point A = PW[0];
        Point B = PW[1];
        Point C = PW[2];

        float abx = B.x - A.x;
        float aby = B.y - A.y;
        float acx = C.x - A.x;
        float acy = C.y - A.y;

        orientationFenetre = abx * acy - aby * acx;
    }
}