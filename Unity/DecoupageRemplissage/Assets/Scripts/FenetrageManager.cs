using UnityEngine;
using System.Collections.Generic;

public class FenetrageManager : MonoBehaviour
{
    public Camera cameraScene;

    public LineRenderer lrPolygone;
    public LineRenderer lrFenetre;
    public LineRenderer lrDecoupe;

    public List<Point> pointsPolygone;
    public List<Point> pointsFenetre;
    public List<Point> pointsDecoupes;

    public bool saisiePolygone = true;
    public bool polygoneFerme = false;
    public bool fenetreFermee = false;

    private void Awake()
    {
        if(cameraScene == null)
        {
            cameraScene = Camera.main;
        }

        pointsPolygone = new List<Point>();
        pointsFenetre = new List<Point>();
        pointsDecoupes = new List<Point>();

        InitialiserLineRenderers();
    }

    private void InitialiserLineRenderers()
    {
        if(lrPolygone == null)
        {
            GameObject goPolygone = new GameObject("LR_Polygone");
            lrPolygone = goPolygone.AddComponent<LineRenderer>();
        }

        if(lrFenetre == null)
        {
            GameObject goFenetre = new GameObject("LR_Fenetre");
            lrFenetre = goFenetre.AddComponent<LineRenderer>();
        }

        if(lrDecoupe == null)
        {
            GameObject goDecoupe = new GameObject("LR_Decoupe");
            lrDecoupe = goDecoupe.AddComponent<LineRenderer>();
        }

        ConfigurerLineRenderer(lrDecoupe, Color.red);
        ConfigurerLineRenderer(lrPolygone, Color.green);
        ConfigurerLineRenderer(lrFenetre, Color.blue);
    }

    private void ConfigurerLineRenderer(LineRenderer lr, Color couleur)
    {
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.widthMultiplier = 0.05f;
        lr.loop = false;
        lr.positionCount = 0;
        lr.startColor = couleur;
        lr.endColor = couleur;
    }

    private void Update()
    {
        GererChangementMode();
        GererSaisiePoints();
        GererValidationEtDecoupage();
        MettreAJourLineRenderers();
    }

    private void GererChangementMode()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            saisiePolygone = true;
        }

        if(Input.GetKeyDown(KeyCode.F))
        {
            saisiePolygone = false;
        }
    }

    private void GererSaisiePoints()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector3 pos = Input.mousePosition;
            pos.z = Mathf.Abs(cameraScene.transform.position.z);
            Vector3 monde = cameraScene.ScreenToWorldPoint(pos);

            Point p = new Point(monde);

            if(saisiePolygone && !polygoneFerme)
            {
                pointsPolygone.Add(p);
            }
            else if(!saisiePolygone && !fenetreFermee)
            {
                pointsFenetre.Add(p);
            }
        }
    }

    private void GererValidationEtDecoupage()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            if(saisiePolygone && pointsPolygone.Count >= 3)
            {
                polygoneFerme = true;
            }
            else if(!saisiePolygone && pointsFenetre.Count >= 3)
            {
                fenetreFermee = true;
            }
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(polygoneFerme && fenetreFermee && pointsPolygone.Count >= 3 && pointsFenetre.Count >= 3)
            {
                DecoupageSutherlandHodgman decoupage = new DecoupageSutherlandHodgman(pointsFenetre, pointsPolygone);
                pointsDecoupes = decoupage.Decouper();
            }
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
        if(liste == null || liste.Count == 0)
        {
            lr.positionCount = 0;
            lr.loop = false;
            return;
        }

        lr.positionCount = liste.Count;
        lr.loop = fermer && liste.Count >= 3;

        for(int i = 0 ; i < liste.Count ; i++)
        {
            lr.SetPosition(i, liste[i].VersVector3());
        }
    }
}