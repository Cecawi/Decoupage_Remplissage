using UnityEngine;
using System.Collections.Generic;

public static class Triangulation
{
    // Algorithme "Ear Clipping" basique
    public static List<List<Point>> Trianguler(List<Point> polygone)
    {
        List<List<Point>> triangles = new List<List<Point>>();
        
        if (polygone == null || polygone.Count < 3) return triangles;

        // Copie de la liste pour ne pas modifier l'originale
        List<Point> sommets = new List<Point>(polygone);

        // S'assurer que le polygone est dans le sens anti-horaire (CCW)
        if (!EstAntiHoraire(sommets))
        {
            sommets.Reverse();
        }

        int count = sommets.Count;
        int maxIterations = count * count; // Sécurité pour éviter boucle infinie
        int iterations = 0;

        while (sommets.Count > 3 && iterations < maxIterations)
        {
            bool earFound = false;
            iterations++;

            for (int i = 0; i < sommets.Count; i++)
            {
                int prevIndex = (i - 1 + sommets.Count) % sommets.Count;
                int nextIndex = (i + 1) % sommets.Count;

                Point prev = sommets[prevIndex];
                Point curr = sommets[i];
                Point next = sommets[nextIndex];

                if (EstOreille(prev, curr, next, sommets))
                {
                    // C'est une oreille, on la coupe (crée un triangle)
                    List<Point> triangle = new List<Point> { prev, curr, next };
                    triangles.Add(triangle);

                    // On retire le sommet courant
                    sommets.RemoveAt(i);
                    earFound = true;
                    break;
                }
            }

            if (!earFound)
            {
                // Cas dégénéré ou polygone complexe non supporté (auto-intersection)
                break; 
            }
        }

        // Ajouter le dernier triangle restant
        if (sommets.Count == 3)
        {
            triangles.Add(new List<Point>(sommets));
        }

        return triangles;
    }

    private static bool EstOreille(Point p1, Point p2, Point p3, List<Point> polygone)
    {
        // 1. Vérifier si l'angle est convexe
        if (!EstConvexe(p1, p2, p3)) return false;

        // 2. Vérifier qu'aucun autre point n'est dans le triangle
        Vector3 a = p1.VersVector3();
        Vector3 b = p2.VersVector3();
        Vector3 c = p3.VersVector3();

        foreach (Point p in polygone)
        {
            if (p == p1 || p == p2 || p == p3) continue;

            Vector3 pt = p.VersVector3();
            if (PointDansTriangle(pt, a, b, c))
            {
                return false;
            }
        }

        return true;
    }

    private static bool EstConvexe(Point p1, Point p2, Point p3)
    {
        Vector3 output = Vector3.Cross(p2.VersVector3() - p1.VersVector3(), p3.VersVector3() - p2.VersVector3());
        return output.z > 0; // Pour CCW, convexe si Z > 0
    }

    private static bool EstAntiHoraire(List<Point> polygone)
    {
        float sum = 0;
        for (int i = 0; i < polygone.Count; i++)
        {
            Point p1 = polygone[i];
            Point p2 = polygone[(i + 1) % polygone.Count];
            sum += (p2.x - p1.x) * (p2.y + p1.y);
        }
        // Si somme < 0 c'est CCW, sinon CW (formule Area = 0.5 * sum)
        // Mais attention repère Unity (Y up) vs écran.
        // Test simple : Cross Product Z global.
        // Utilisons la formule standard : si > 0 => CW, < 0 => CCW
        return sum < 0; 
    }

    private static bool PointDansTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        // Technique barycentrique ou produits vectoriels
        bool b1 = Signe(p, a, b) < 0.0f;
        bool b2 = Signe(p, b, c) < 0.0f;
        bool b3 = Signe(p, c, a) < 0.0f;

        return ((b1 == b2) && (b2 == b3));
    }

    private static float Signe(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }
}
