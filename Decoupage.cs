/*

*/

public class Decoupage
{
    //PF

    //VI
    int i, j, N2
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