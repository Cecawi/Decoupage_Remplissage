using UnityEngine;
using System.Collections.Generic;

public enum ModeApplication
{
    SaisiePolygone,
    SaisieFenetre,
    Edition,
    Inactif
}

public class FenetrageManager : MonoBehaviour
{
    public Camera cameraScene;

    public LineRenderer lrPolygone;
    public LineRenderer lrFenetre;
    public LineRenderer lrDecoupe;

    public List<Point> pointsPolygone;
    public List<Point> pointsFenetre;
    public List<Point> pointsDecoupes;

    public bool polygoneFerme = false;
    public bool fenetreFermee = false;

    public ModeApplication modeActuel = ModeApplication.SaisiePolygone;
    private Point pointSelectionne = null;
    private bool isDragging = false;
    private float rayonSelection = 0.5f;

    private void Awake()
    {
        if(cameraScene == null) cameraScene = Camera.main;

        pointsPolygone = new List<Point>();
        pointsFenetre = new List<Point>();
        pointsDecoupes = new List<Point>();

        InitialiserLineRenderers();
    }

    private void InitialiserLineRenderers()
    {
        if(lrPolygone == null) lrPolygone = CreateLR("LR_Polygone");
        if(lrFenetre == null) lrFenetre = CreateLR("LR_Fenetre");
        if(lrDecoupe == null) lrDecoupe = CreateLR("LR_Decoupe");

        ConfigurerLineRenderer(lrDecoupe, Color.red, 0.1f);
        ConfigurerLineRenderer(lrPolygone, Color.green, 0.05f);
        ConfigurerLineRenderer(lrFenetre, Color.blue, 0.05f);
    }

    private LineRenderer CreateLR(string name)
    {
        GameObject go = new GameObject(name);
        return go.AddComponent<LineRenderer>();
    }

    private void ConfigurerLineRenderer(LineRenderer lr, Color couleur, float width)
    {
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.widthMultiplier = width;
        lr.loop = false;
        lr.positionCount = 0;
        lr.startColor = couleur;
        lr.endColor = couleur;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) ResetProjet();

        Vector3 mousePos = GetMousePosition();

        switch (modeActuel)
        {
            case ModeApplication.SaisiePolygone:
                GererSaisie(pointsPolygone, ref polygoneFerme, mousePos);
                break;
            case ModeApplication.SaisieFenetre:
                GererSaisie(pointsFenetre, ref fenetreFermee, mousePos);
                break;
            case ModeApplication.Edition:
                GererEdition(mousePos);
                break;
        }

        //mise à jour continue si on drag
        if (modeActuel == ModeApplication.Edition && isDragging)
        {
            RecalculerDecoupage();
        }

        MettreAJourLineRenderers();
    }

    private Vector3 GetMousePosition()
    {
        Vector3 pos = Input.mousePosition;
        pos.z = Mathf.Abs(cameraScene.transform.position.z);
        return cameraScene.ScreenToWorldPoint(pos);
    }

    private void GererSaisie(List<Point> liste, ref bool estFerme, Vector3 mousePos)
    {
        if (estFerme) return;

        if (Input.GetMouseButtonDown(0))
        {
            liste.Add(new Point(mousePos));
        }

        if (Input.GetKeyDown(KeyCode.Return) && liste.Count >= 3)
        {
            estFerme = true;
            RecalculerDecoupage();
        }
    }

    private void GererEdition(Vector3 mousePos)
    {
        if (Input.GetMouseButtonDown(0))
        {
            //cherche le point le plus proche dans Poly et Fenetre
            Point pPoly = TrouverPointProche(pointsPolygone, mousePos);
            Point pFen = TrouverPointProche(pointsFenetre, mousePos);

            //priorité : si deux points sont proches, on prend le plus proche absolu
            if (pPoly != null && pFen != null)
            {
                float d1 = Vector3.Distance(pPoly.VersVector3(), mousePos);
                float d2 = Vector3.Distance(pFen.VersVector3(), mousePos);
                pointSelectionne = (d1 < d2) ? pPoly : pFen;
            }
            else
            {
                pointSelectionne = (pPoly != null) ? pPoly : pFen;
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
        if (polygoneFerme && fenetreFermee && pointsPolygone.Count >= 3 && pointsFenetre.Count >= 3)
        {
            DecoupageSutherlandHodgman decoupage = new DecoupageSutherlandHodgman(pointsFenetre, pointsPolygone);
            pointsDecoupes = decoupage.Decouper();
        }
        else
        {
            pointsDecoupes.Clear();
        }
    }

    private void MettreAJourLineRenderers()
    {
        MettreAJourLineRenderer(lrPolygone, pointsPolygone, polygoneFerme);
        MettreAJourLineRenderer(lrFenetre, pointsFenetre, fenetreFermee);
        MettreAJourLineRenderer(lrDecoupe, pointsDecoupes, true);
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
        modeActuel = nouveauMode;
    }

    public void ResetProjet()
    {
        pointsPolygone.Clear();
        pointsFenetre.Clear();
        pointsDecoupes.Clear();
        polygoneFerme = false;
        fenetreFermee = false;
        modeActuel = ModeApplication.SaisiePolygone;
    }
}