using UnityEngine;
using System.Collections.Generic;

public enum ModeApplication
{
    SaisiePolygone,
    SaisieFenetre,
    Edition,
    SelectionSupprimer,
    SelectionSupprimerFenetre,
    Inactif
}

public class FenetrageManager : MonoBehaviour
{
    public Camera cameraScene;
    private UIManager uiManager;

    public List<Polygone> listePolygones = new List<Polygone>();
    private Polygone polygoneCourant;
    
    public List<Fenetre> listeFenetres = new List<Fenetre>();
    private Fenetre fenetreCourante;

    public ModeApplication modeActuel = ModeApplication.SaisiePolygone;
    private Point pointSelectionne = null;
    private Polygone polygoneSelectionne = null;
    private Fenetre fenetreSelectionnee = null;
    private bool isDragging = false;
    private float rayonSelection = 0.5f;

    private void Awake()
    {
        if(cameraScene == null) cameraScene = Camera.main;
        uiManager = FindFirstObjectByType<UIManager>();

        listePolygones = new List<Polygone>();
        listeFenetres = new List<Fenetre>();
    }


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) ResetProjet();

        Vector3 mousePos = GetMousePosition();

        switch (modeActuel)
        {
            case ModeApplication.SaisiePolygone:
                GererSaisiePolygone(mousePos);
                break;
            case ModeApplication.SaisieFenetre:
                GererSaisieFenetre(mousePos);
                break;
            case ModeApplication.Edition:
                GererEdition(mousePos);
                break;
            case ModeApplication.SelectionSupprimer:
                GererSelectionSupprimer(mousePos);
                break;
            case ModeApplication.SelectionSupprimerFenetre:
                GererSelectionSupprimerFenetre(mousePos);
                break;
        }

        if (modeActuel == ModeApplication.Edition && isDragging)
        {
            RecalculerDecoupage();
        }

        MettreAJourRenderers();
    }

    private Vector3 GetMousePosition()
    {
        Vector3 pos = Input.mousePosition;
        pos.z = Mathf.Abs(cameraScene.transform.position.z);
        return cameraScene.ScreenToWorldPoint(pos);
    }

    private void GererSaisiePolygone(Vector3 mousePos)
    {
        if (polygoneCourant == null || polygoneCourant.estFerme)
        {
            polygoneCourant = new Polygone();
            polygoneCourant.InitialiserLineRenderers();
            listePolygones.Add(polygoneCourant);
        }

        if (Input.GetMouseButtonDown(0) && !IsMouseOverUI())
        {
            polygoneCourant.sommets.Add(new Point(mousePos));
        }

        if (Input.GetKeyDown(KeyCode.Return) && polygoneCourant.sommets.Count >= 3)
        {
            polygoneCourant.estFerme = true;
            RecalculerDecoupage();
        }
    }

    private void GererSaisieFenetre(Vector3 mousePos)
    {
        if (fenetreCourante == null || fenetreCourante.estFermee)
        {
            fenetreCourante = new Fenetre();
            fenetreCourante.InitialiserLineRenderer();
            listeFenetres.Add(fenetreCourante);
        }

        if (Input.GetMouseButtonDown(0) && !IsMouseOverUI())
        {
            fenetreCourante.sommets.Add(new Point(mousePos));
        }

        if (Input.GetKeyDown(KeyCode.Return) && fenetreCourante.sommets.Count >= 3)
        {
            fenetreCourante.estFermee = true;
            RecalculerDecoupage();
        }
    }

    private void GererEdition(Vector3 mousePos)
    {
        if (Input.GetMouseButtonDown(0) && !IsMouseOverUI())
        {
            pointSelectionne = null;
            float minDist = rayonSelection;

            foreach (Polygone poly in listePolygones)
            {
                foreach (Point p in poly.sommets)
                {
                    float d = Vector3.Distance(p.VersVector3(), mousePos);
                    if (d < minDist)
                    {
                        minDist = d;
                        pointSelectionne = p;
                    }
                }
            }

            foreach (Fenetre fen in listeFenetres)
            {
                foreach(Point p in fen.sommets)
                {
                    float d = Vector3.Distance(p.VersVector3(), mousePos);
                    if (d < minDist)
                    {
                        minDist = d;
                        pointSelectionne = p;
                    }
                }
            }

            if (pointSelectionne != null)
            {
                isDragging = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            pointSelectionne = null;
        }

        if (isDragging && pointSelectionne != null)
        {
            pointSelectionne.x = mousePos.x;
            pointSelectionne.y = mousePos.y;
            pointSelectionne.z = mousePos.z;
        }
    }

    private void GererSelectionSupprimer(Vector3 mousePos)
    {
        if (Input.GetMouseButtonDown(0) && !IsMouseOverUI())
        {
            Polygone ancienSelection = polygoneSelectionne;
            polygoneSelectionne = null;
            float minDist = float.MaxValue;

            foreach (Polygone poly in listePolygones)
            {
                foreach (Point p in poly.sommets)
                {
                    float d = Vector3.Distance(p.VersVector3(), mousePos);
                    if (d < minDist)
                    {
                        minDist = d;
                        polygoneSelectionne = poly;
                    }
                }
            }

            if (ancienSelection != null) ancienSelection.SetHighlight(false);
            if (polygoneSelectionne != null) polygoneSelectionne.SetHighlight(true);
        }
    }

    private void GererSelectionSupprimerFenetre(Vector3 mousePos)
    {
        if (Input.GetMouseButtonDown(0) && !IsMouseOverUI())
        {
            Fenetre ancienSelection = fenetreSelectionnee;
            fenetreSelectionnee = null;
            float minDist = float.MaxValue;

            foreach (Fenetre fen in listeFenetres)
            {
                foreach (Point p in fen.sommets)
                {
                    float d = Vector3.Distance(p.VersVector3(), mousePos);
                    if (d < minDist)
                    {
                        minDist = d;
                        fenetreSelectionnee = fen;
                    }
                }
            }

            if (ancienSelection != null) ancienSelection.SetHighlight(false);
            if (fenetreSelectionnee != null) fenetreSelectionnee.SetHighlight(true);
        }
    }

    private Point TrouverPointProche(List<Point> liste, Vector3 target)
    {
        if (liste == null) return null;
        Point best = null;
        float minDist = rayonSelection;

        foreach (Point p in liste)
        {
            float d = Vector3.Distance(p.VersVector3(), target);
            if (d < minDist)
            {
                minDist = d;
                best = p;
            }
        }
        return best;
    }

    public bool gestionConcavite = false;
    public bool afficherTriangulation = false;

    public void RecalculerDecoupage()
    {
        //si aucune fenetre valide, pas de découpage
        bool aucuneFenetreValide = true;
        foreach (Fenetre fen in listeFenetres)
        {
            if (fen.estFermee && fen.sommets.Count >= 3)
            {
                aucuneFenetreValide = false;
                // Calcul concavité toujours nécessaire pour savoir si on affiche le bouton UI ou pas
                fen.CheckConcavite(); 
                break;
            }
        }

        // On recalcul concavité pour toutes
        foreach(Fenetre fen in listeFenetres) if(fen.estFermee) fen.CheckConcavite();

        if (aucuneFenetreValide)
        {
            foreach (Polygone poly in listePolygones) poly.resultatsDecoupes.Clear();
            return;
        }

        foreach (Polygone poly in listePolygones)
        {
            if (poly.estFerme && poly.sommets.Count >= 3)
            {
                List<List<Point>> morceauxCourants = new List<List<Point>>();
                morceauxCourants.Add(new List<Point>(poly.sommets));

                foreach (Fenetre fen in listeFenetres)
                {
                    if (fen.estFermee && fen.sommets.Count >= 3)
                    {
                        if (morceauxCourants.Count == 0) break;

                        List<List<Point>> nouveauxMorceaux = new List<List<Point>>();

                        // LOGIQUE PRINCIPALE : CONVEXE vs CONCAVE
                        if (!gestionConcavite)
                        {
                            // Mode SIMPLE (Sutherland-Hodgman standard sur la fenêtre entière)
                            foreach(List<Point> morceau in morceauxCourants)
                            {
                                DecoupageSutherlandHodgman decoupage = new DecoupageSutherlandHodgman(fen.sommets, morceau);
                                List<Point> resultat = decoupage.Decouper();
                                if (resultat.Count >= 3) nouveauxMorceaux.Add(resultat);
                            }
                        }
                        else
                        {
                            // Mode AVANCÉ (Triangulation)
                            List<List<Point>> trianglesFenetre = fen.trianglesCache;
                            if(trianglesFenetre == null || trianglesFenetre.Count == 0) 
                            {
                                // Fallback si triangulation vide (bizarre mais safety)
                                trianglesFenetre = new List<List<Point>> { fen.sommets };
                            }

                            foreach(List<Point> morceau in morceauxCourants)
                            {
                                foreach(List<Point> triangle in trianglesFenetre)
                                {
                                    DecoupageSutherlandHodgman decoupage = new DecoupageSutherlandHodgman(triangle, morceau);
                                    List<Point> resultat = decoupage.Decouper();
                                    if (resultat.Count >= 3) nouveauxMorceaux.Add(resultat);
                                }
                            }
                        }
                        
                        morceauxCourants = nouveauxMorceaux;
                    }
                }

                // ETAPE FINALE DE FUSION : On recolle les morceaux adjacents pour supprimer les traits internes
                if (gestionConcavite && morceauxCourants.Count > 1)
                {
                    morceauxCourants = FusionnerMorceaux(morceauxCourants);
                }

                poly.resultatsDecoupes = morceauxCourants;
            }
            else
            {
                poly.resultatsDecoupes.Clear();
            }
        }
    }

    private void MettreAJourRenderers()
    {
        if (listePolygones != null)
        {
            foreach (Polygone poly in listePolygones)
            {
                poly.MettreAJourRenderers();
            }
        }

        if (listeFenetres != null)
        {
            foreach (Fenetre fen in listeFenetres)
            {
                // On affiche la triangulation SEULEMENT si mode concave actif ET option affichage active
                fen.MettreAJourRenderer(gestionConcavite && afficherTriangulation);
            }
        }
    }

    public bool ExisteFenetreConcave()
    {
        foreach(Fenetre fen in listeFenetres)
        {
            if(fen.estFermee && fen.EstConcave()) return true;
        }
        return false;
    }

    public void ToggleGestionConcavite()
    {
        gestionConcavite = !gestionConcavite;
        // Si on désactive la concavité, on cache forcément la triangulation
        if(!gestionConcavite) afficherTriangulation = false; 
        RecalculerDecoupage();
    }

    public void ToggleAfficherTriangulation()
    {
        afficherTriangulation = !afficherTriangulation;
    }


    public void SetMode(ModeApplication nouveauMode)
    {
        if (modeActuel == ModeApplication.SelectionSupprimer && polygoneSelectionne != null)
        {
            polygoneSelectionne.SetHighlight(false);
            polygoneSelectionne = null;
        }

        modeActuel = nouveauMode;
    }

    public void SupprimerPolygoneSelectionne()
    {
        if (polygoneSelectionne != null)
        {
            polygoneSelectionne.Detruire();
            listePolygones.Remove(polygoneSelectionne);
            polygoneSelectionne = null;
            RecalculerDecoupage();
        }
    }

    public bool APolygoneSelectionne()
    {
        return polygoneSelectionne != null;
    }

    public int NombrePolygones()
    {
        return listePolygones.Count;
    }

    public void NouveauPolygone()
    {
        if (polygoneCourant != null && !polygoneCourant.estFerme && polygoneCourant.sommets.Count >= 3)
        {
            polygoneCourant.estFerme = true;
            RecalculerDecoupage();
        }
        modeActuel = ModeApplication.SaisiePolygone;
    }

    public void NouvelleFenetre()
    {
        //auto-close polygon if open
        if (polygoneCourant != null && !polygoneCourant.estFerme && polygoneCourant.sommets.Count >= 3)
        {
            polygoneCourant.estFerme = true;
            RecalculerDecoupage();
        }

        //auto-close current window if open
        if (fenetreCourante != null && !fenetreCourante.estFermee && fenetreCourante.sommets.Count >= 3)
        {
            fenetreCourante.estFermee = true;
            RecalculerDecoupage();
        }
        modeActuel = ModeApplication.SaisieFenetre;
    }

    public void SupprimerFenetreSelectionnee()
    {
        if (fenetreSelectionnee != null)
        {
            fenetreSelectionnee.Detruire();
            listeFenetres.Remove(fenetreSelectionnee);
            fenetreSelectionnee = null;
            RecalculerDecoupage();
        }
    }

    public bool AFenetreSelectionnee()
    {
        return fenetreSelectionnee != null;
    }

    public int NombreFenetres()
    {
        return listeFenetres.Count;
    }

    public void ResetProjet()
    {
        foreach (Polygone p in listePolygones) p.Detruire();
        listePolygones.Clear();
        polygoneCourant = null;
        polygoneSelectionne = null;

        foreach (Fenetre f in listeFenetres) f.Detruire();
        listeFenetres.Clear();
        fenetreCourante = null;
        fenetreSelectionnee = null;

        modeActuel = ModeApplication.SaisiePolygone;
    }



    private bool IsMouseOverUI()
    {
        return uiManager != null && uiManager.IsMouseOverUI();
    }

    // =========================================================================================
    // FUSION DES POLYGONES (Suppression des arêtes internes)
    // =========================================================================================

    private List<List<Point>> FusionnerMorceaux(List<List<Point>> morceaux)
    {
        // 1. Extraire toutes les arêtes orientées
        List<AreteOriente> toutesAretes = new List<AreteOriente>();
        foreach (List<Point> poly in morceaux)
        {
            for (int i = 0; i < poly.Count; i++)
            {
                Point A = poly[i];
                Point B = poly[(i + 1) % poly.Count];
                toutesAretes.Add(new AreteOriente(A, B));
            }
        }

        // 2. Identifier et marquer les arêtes "jumelles" (A->B et B->A)
        // O(N^2) naïf, acceptable pour ce projet interactif
        List<AreteOriente> aretesFinales = new List<AreteOriente>();
        bool[] ignoreIndices = new bool[toutesAretes.Count];

        for (int i = 0; i < toutesAretes.Count; i++)
        {
            if (ignoreIndices[i]) continue;

            bool paired = false;
            for (int j = i + 1; j < toutesAretes.Count; j++)
            {
                if (ignoreIndices[j]) continue;

                if (SontJumelles(toutesAretes[i], toutesAretes[j]))
                {
                    ignoreIndices[i] = true;
                    ignoreIndices[j] = true;
                    paired = true;
                    break;
                }
            }

            if (!paired)
            {
                aretesFinales.Add(toutesAretes[i]);
            }
        }

        // Si tout s'est annulé (impossible normalement), retour vide
        if (aretesFinales.Count == 0) return new List<List<Point>>();

        // 3. Reconstruire les boucles (Polygones disjoints ou unique)
        List<List<Point>> resultatsFusionnes = new List<List<Point>>();
        bool[] areteUtilisee = new bool[aretesFinales.Count];

        for (int i = 0; i < aretesFinales.Count; i++)
        {
            if (areteUtilisee[i]) continue;

            List<Point> nouveauPoly = new List<Point>();
            AreteOriente current = aretesFinales[i];
            
            nouveauPoly.Add(current.start);
            areteUtilisee[i] = true;
            
            bool boucleFermee = false;
            int watchdog = 0;
            int maxWatchdog = aretesFinales.Count * 2; // Protection boucle infinie

            while (!boucleFermee && watchdog < maxWatchdog)
            {
                watchdog++;
                Point chercheDepart = current.end;
                
                // Chercher l'arête suivante qui commence par chercheDepart
                bool foundNext = false;
                for (int j = 0; j < aretesFinales.Count; j++)
                {
                    if (!areteUtilisee[j] && PointsEgaux(aretesFinales[j].start, chercheDepart))
                    {
                        current = aretesFinales[j];
                        areteUtilisee[j] = true;
                        
                        // Si on revient au point de départ du polygone
                        if (PointsEgaux(current.end, nouveauPoly[0]))
                        {
                            nouveauPoly.Add(current.start); // Ajoute le dernier point (fermeture implicite ou explicite selon usage)
                            boucleFermee = true;
                        }
                        else
                        {
                            nouveauPoly.Add(current.start);
                        }
                        foundNext = true;
                        break;
                    }
                }

                if (!foundNext)
                {
                    // Peut arriver si géométrie dégénérée, on abandonne ce fragment
                    break; 
                }
            }

            if (boucleFermee && nouveauPoly.Count >= 3)
            {
                resultatsFusionnes.Add(nouveauPoly);
            }
        }

        return resultatsFusionnes;
    }

    private bool SontJumelles(AreteOriente a1, AreteOriente a2)
    {
        return PointsEgaux(a1.start, a2.end) && PointsEgaux(a1.end, a2.start);
    }

    private bool PointsEgaux(Point p1, Point p2)
    {
        return Vector3.Distance(p1.VersVector3(), p2.VersVector3()) < 0.001f; // epsilon pour floats
    }

    private class AreteOriente
    {
        public Point start;
        public Point end;
        public AreteOriente(Point s, Point e) { start = s; end = e; }
    }
}