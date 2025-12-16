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

    public List<Polygone> listePolygones;
    private Polygone polygoneCourant;
    
    public List<Fenetre> listeFenetres;
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

    public void RecalculerDecoupage()
    {
        //si aucune fenetre valide, pas de découpage
        bool aucuneFenetreValide = true;
        foreach (Fenetre fen in listeFenetres)
        {
            if (fen.estFermee && fen.sommets.Count >= 3)
            {
                aucuneFenetreValide = false;
                break;
            }
        }

        if (aucuneFenetreValide)
        {
            foreach (Polygone poly in listePolygones) poly.sommetsDecoupes.Clear();
            return;
        }

        foreach (Polygone poly in listePolygones)
        {
            if (poly.estFerme && poly.sommets.Count >= 3)
            {
                //commencer avec les sommets du polygone
                List<Point> currentPoints = new List<Point>(poly.sommets);

                //appliquer chaque fenetre séquentiellement
                foreach (Fenetre fen in listeFenetres)
                {
                    if (fen.estFermee && fen.sommets.Count >= 3)
                    {
                        if (currentPoints.Count < 3) break; //plus rien à découper

                        DecoupageSutherlandHodgman decoupage = new DecoupageSutherlandHodgman(fen.sommets, currentPoints);
                        currentPoints = decoupage.Decouper();
                    }
                }
                poly.sommetsDecoupes = currentPoints;
            }
            else
            {
                poly.sommetsDecoupes.Clear();
            }
        }
    }

    private void MettreAJourRenderers()
    {
        foreach (Polygone poly in listePolygones)
        {
            poly.MettreAJourRenderers();
        }

        foreach (Fenetre fen in listeFenetres)
        {
            fen.MettreAJourRenderer();
        }
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
}