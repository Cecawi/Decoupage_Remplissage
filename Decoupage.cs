//////////LCA!!!!!

using System.Collections.Generic;

public class Point
{
    public float x, y, z;
    
    public Point(float x, float y, float z = 0)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

public class Decoupage
{
    //PW : liste des N3 sommets de la fenêtre (window)
    private List<Point> PW;
    
    //PL : liste des N1 sommets du polygone à découper
    private List<Point> PL;
    
    //PS : liste de points du polygone de sortie (PL découpé)
    private List<Point> PS;
    
    //variables
    private Point S,F,I;//point précédent, fenetre?/polynome?, d'intersection
    private int N1;//nombre de points du polygone d'entrée (sommets)
    private int N2;//nombre de points du polygone de sortie
    
    public Decoupage(List<Point> fenetre, List<Point> polygone)
    {
        this.PW = fenetre;
        this.PL = new List<Point>(polygone);
        this.PS = new List<Point>();
    }
}

bool Coupe(Point S, Point Pj, Point Fi, Point FiPlusUn)
{
    return intersection [S Pj] et (Fi FiPlusUn)?
}

Intersection()
{


}

bool Visible(Point S, Point Fi, Point FiPlusUn)
{

}

//pour chaque point de la window PW
for(int i = 1 ; i < PW.Length() - 1 ; i++)
{
    N2 = 0;
    //PS = vide
    //pour chaque point du polygone PL
    for(int j = A ; j < N1 : j++)
    {
        if(j = 1)
        {
            F.//push(P(j)) on met le 1er/dernier sommet dans le liste
        }
        else
        {
            if(Coupe(S, P(j), F(j), F(j+1)))
            {
                I = Intersection(S, P(j), F(i), F(i+1));
                PS.push(I);
                N2++;
            }
        }
        S = P(j);
        if(Visible(S, F(i), F(i+1)))
        {
            PS.push(S);
            N2++;
        }
    }
    if(N2 > 0)
    {
        //traitement du dernier côté de PL
        if(Coupe(S, F, F(i), F(i+1)))
        {
            I = Intersection(S, F, F(i), F(i+1));
            PS.push(I);
            N2++;
        }
        //découpage pour chacun des polygones
        PL = PS;
        N1 = N2;
    }
}