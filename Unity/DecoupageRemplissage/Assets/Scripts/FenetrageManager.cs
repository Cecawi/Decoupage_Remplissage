using UnityEngine;
using System.Collections.Generic;

public enum ModeApplication
{
    SaisiePolygone,
    SaisieFenetre,
    Edition,
    SelectionSupprimer,
    Inactif
}

public class FenetrageManager : MonoBehaviour
{
    public Camera cameraScene;
    private UIManager uiManager;

    public List<Polygone> listePolygones;
    private Polygone polygoneCourant;
    
    public List<Point> pointsFenetre;
    public LineRenderer lrFenetre;
    public bool fenetreFermee = false;

    public ModeApplication modeActuel = ModeApplication.SaisiePolygone;
    private Point pointSelectionne = null;
    private Polygone polygoneSelectionne = null;
    private bool isDragging = false;
    private float rayonSelection = 0.5f;

    private void Awake()
    {
        if(cameraScene == null) cameraScene = Camera.main;
        uiManager = FindFirstObjectByType<UIManager>();

        listePolygones = new List<Polygone>();
        pointsFenetre = new List<Point>();

        InitialiserLineRendererFenetre();
    }

    private void InitialiserLineRendererFenetre()
    {
        if(lrFenetre == null)
        {
            GameObject go = new GameObject("LR_Fenetre");
            lrFenetre = go.AddComponent<LineRenderer>();
            ConfigurerLineRenderer(lrFenetre, Color.blue, 0.05f);
        }
    }

    private void ConfigurerLineRenderer(LineRenderer lr, Color couleur, float width)
    {
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.widthMultiplier = width;
        lr.loop = false;
        lr.positionCount = 0;
        lr.startColor = couleur;
        lr.endColor = couleur;
        
        //ordre de rendu: vert=0, bleu=1, rouge=2
        if (couleur == Color.blue)
        {
            lr.sortingOrder = 1;
        }
        else
        {
            lr.sortingOrder = 0;
        }
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
        if (fenetreFermee) return;

        if (Input.GetMouseButtonDown(0) && !IsMouseOverUI())
        {
            pointsFenetre.Add(new Point(mousePos));
        }

        if (Input.GetKeyDown(KeyCode.Return) && pointsFenetre.Count >= 3)
        {
            fenetreFermee = true;
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

            Point pFen = TrouverPointProche(pointsFenetre, mousePos);
            if (pFen != null)
            {
                float d = Vector3.Distance(pFen.VersVector3(), mousePos);
                if (d < minDist)
                {
                    pointSelectionne = pFen;
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
        if (!fenetreFermee || pointsFenetre.Count < 3) return;

        foreach (Polygone poly in listePolygones)
        {
            if (poly.estFerme && poly.sommets.Count >= 3)
            {
                DecoupageSutherlandHodgman decoupage = new DecoupageSutherlandHodgman(pointsFenetre, poly.sommets);
                poly.sommetsDecoupes = decoupage.Decouper();
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

        MettreAJourLineRenderer(lrFenetre, pointsFenetre, fenetreFermee);
    }

    private void MettreAJourLineRenderer(LineRenderer lr, List<Point> liste, bool fermer)
    {
        if (liste == null || liste.Count == 0)
        {
            lr.positionCount = 0;
            return;
        }

        lr.positionCount = liste.Count;
        lr.loop = fermer && liste.Count >= 3;

        for (int i = 0; i < liste.Count; i++)
        {
            lr.SetPosition(i, liste[i].VersVector3());
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

    public void ResetProjet()
    {
        foreach (Polygone poly in listePolygones)
        {
            poly.Detruire();
        }
        listePolygones.Clear();
        polygoneCourant = null;
        polygoneSelectionne = null;
        
        pointsFenetre.Clear();
        fenetreFermee = false;
        modeActuel = ModeApplication.SaisiePolygone;
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
        if (polygoneCourant != null && !polygoneCourant.estFerme && polygoneCourant.sommets.Count >= 3)
        {
            polygoneCourant.estFerme = true;
        }
        
        pointsFenetre.Clear();
        fenetreFermee = false;
        foreach (Polygone poly in listePolygones)
        {
            poly.sommetsDecoupes.Clear();
        }
        modeActuel = ModeApplication.SaisieFenetre;
    }

    private bool IsMouseOverUI()
    {
        return uiManager != null && uiManager.IsMouseOverUI();
    }
}