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

    private float ProduitVectoriel(Point Fi, Point FiPlusUn, Point P)
    {
        float dx1 = FiPlusUn.x - Fi.x;
        float dy1 = FiPlusUn.y - Fi.y;
        float dx2 = P.x - Fi.x;
        float dy2 = P.y - Fi.y;

        return dx1 * dy2 - dy1 * dx2;
    }

    private bool Visible(Point P, Point Fi, Point FiPlusUn)
    {
        float pv = ProduitVectoriel(Fi, FiPlusUn, P);

        if(orientationFenetre > 0.0f)
        {
            //intérieur du côté gauche
            return pv >= 0.0f;
        }
        else if(orientationFenetre < 0.0f)
        {
            //intérieur du côté droit
            return pv <= 0.0f;
        }
        else
        {
            //fenêtre dégénérée, on considère tout visible
            return true;
        }
    }

    //teste si le segment [S,Pj] coupe le bord [Fi,FiPlusUn] en tenant compte des points sur la frontière
    private bool Coupe(Point S, Point Pj, Point Fi, Point FiPlusUn)
    {
        float d1 = ProduitVectoriel(Fi, FiPlusUn, S);
        float d2 = ProduitVectoriel(Fi, FiPlusUn, Pj);

        //au moins un point sur la frontière
        if(Mathf.Approximately(d1, 0.0f) || Mathf.Approximately(d2, 0.0f))
        {
            return true;
        }

        //signes opposés -> coupe
        return (d1 * d2 < 0.0f);
    }

    private Point Intersection(Point S, Point Pj, Point Fi, Point FiPlusUn)
    {
        float dx1 = Pj.x - S.x;
        float dy1 = Pj.y - S.y;

        float dx2 = FiPlusUn.x - Fi.x;
        float dy2 = FiPlusUn.y - Fi.y;

        float denom = dx1 * dy2 - dy1 * dx2;

        if(Mathf.Approximately(denom, 0.0f))
        {
            //segments parallèles ou presque, on renvoie S pour éviter NaN
            return new Point(S.x, S.y, S.z);
        }

        float dx3 = Fi.x - S.x;
        float dy3 = Fi.y - S.y;

        float t = (dx3 * dy2 - dy3 * dx2) / denom;

        float x = S.x + t * dx1;
        float y = S.y + t * dy1;
        float z = S.z + t * (Pj.z - S.z);

        return new Point(x, y, z);
    }

    //renvoie le polygone PL découpé par la fenêtre PW
    public List<Point> Decouper()
    {
        N1 = PL.Count;

        if(PW == null || PW.Count < 2 || N1 == 0)
        {
            return PL;
        }

        for(int i = 0 ; i < PW.Count ; i++)
        {
            N2 = 0;
            PS = new List<Point>();

            Point Fi = PW[i];
            Point FiPlusUn = PW[(i + 1) % PW.Count];

            if(N1 == 0)
            {
                break;
            }

            //S = dernier sommet du polygone courant
            S = PL[N1 - 1];

            //pour chaque sommet du polygone PL
            for(int j = 0 ; j < N1 ; j++)
            {
                Point Pj = PL[j];

                bool Svisible = Visible(S, Fi, FiPlusUn);
                bool Pjvisible = Visible(Pj, Fi, FiPlusUn);

                if(Svisible && Pjvisible)
                {
                    //de l'intérieur vers l'intérieur
                    PS.Add(Pj);
                    N2++;
                }
                else if(Svisible && !Pjvisible)
                {
                    //de l'intérieur vers l'extérieur
                    if(Coupe(S, Pj, Fi, FiPlusUn))
                    {
                        I = Intersection(S, Pj, Fi, FiPlusUn);
                        PS.Add(I);
                        N2++;
                    }
                }
                else if(!Svisible && Pjvisible)
                {
                    //de l'extérieur vers l'intérieur
                    if(Coupe(S, Pj, Fi, FiPlusUn))
                    {
                        I = Intersection(S, Pj, Fi, FiPlusUn);
                        PS.Add(I);
                        N2++;
                    }
                    PS.Add(Pj);
                    N2++;
                }
                else
                {
                    //extérieur vers extérieur -> rien à ajouter
                }

                S = Pj;
            }

            //mise à jour du polygone
            PL = new List<Point>(PS);
            N1 = N2;

            if(N1 == 0)
            {
                //polygone complètement à l'extérieur
                break;
            }
        }

        return PL;
    }
}