using UnityEngine;

public class UIManager : MonoBehaviour
{
    private FenetrageManager manager;
    [SerializeField] private Rect windowRect = new Rect(10, 10, 200, 300); 
    [SerializeField] private Rect legendRect = new Rect(0, 10, 180, 150);
    [SerializeField] private float uiScale = 2.5f;
    private Rect scaledWindowRect;
    private Rect scaledLegendRect;
    private Vector2 scrollPos;
    
    private void Start()
    {
        manager = FindFirstObjectByType<FenetrageManager>();
    }

    private void OnGUI()
    {
        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(uiScale, uiScale, 1.0f));

        //calcul position légende (haut droite)
        legendRect.x = (Screen.width / uiScale) - legendRect.width - 10;

        scaledWindowRect = new Rect(
            windowRect.x * uiScale,
            windowRect.y * uiScale,
            windowRect.width * uiScale,
            windowRect.height * uiScale
        );

        scaledLegendRect = new Rect(
            legendRect.x * uiScale,
            legendRect.y * uiScale,
            legendRect.width * uiScale,
            legendRect.height * uiScale
        );

        //menu principal
        GUI.Box(windowRect, "Menu");

        GUILayout.BeginArea(new Rect(windowRect.x + 10, windowRect.y + 30, windowRect.width - 20, windowRect.height - 40));
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUILayout.Label("Actions : ");

        if (GUILayout.Button("Nouveau Polygone"))
        {
            manager.NouveauPolygone();
        }

        if (GUILayout.Button("Nouvelle Fenêtre"))
        {
            manager.NouvelleFenetre();
        }

        GUILayout.Space(10);
        GUILayout.Label("Mode Actuel : " + manager.modeActuel);

        if (GUILayout.Button("Mode Édition / Déplacement"))
        {
            manager.SetMode(ModeApplication.Edition);
        }

        GUILayout.Space(10);

        if (manager.NombrePolygones() > 0)
        {
            bool enModeSelection = manager.modeActuel == ModeApplication.SelectionSupprimer;
            string texte = enModeSelection && manager.APolygoneSelectionne() 
                ? "Supprimer le Polygone" 
                : "Sélectionner Polygone à Supprimer";

            if (GUILayout.Button(texte))
            {
                if (enModeSelection && manager.APolygoneSelectionne())
                {
                    manager.SupprimerPolygoneSelectionne();
                    manager.SetMode(ModeApplication.Inactif);
                }
                else
                {
                    manager.SetMode(ModeApplication.SelectionSupprimer);
                }
            }
        }

        if (manager.NombreFenetres() > 0)
        {
            bool enModeSelectionFen = manager.modeActuel == ModeApplication.SelectionSupprimerFenetre;
            string texte = enModeSelectionFen && manager.AFenetreSelectionnee() 
                ? "Supprimer la Fenêtre" 
                : "Sélectionner Fenêtre à Supprimer";

            if (GUILayout.Button(texte))
            {
                if (enModeSelectionFen && manager.AFenetreSelectionnee())
                {
                    manager.SupprimerFenetreSelectionnee();
                    manager.SetMode(ModeApplication.Inactif);
                }
                else
                {
                    manager.SetMode(ModeApplication.SelectionSupprimerFenetre);
                }
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("Options Avancées : ");
        
        // Bouton Toggle Concavité
        string txtConcavite = manager.gestionConcavite ? "Ne plus traiter cas concaves" : "Traiter cas concaves";
        if (GUILayout.Button(txtConcavite))
        {
            manager.ToggleGestionConcavite();
        }

        // Bouton Triangulation (conditionnel)
        if (manager.gestionConcavite && manager.ExisteFenetreConcave())
        {
            string txtTri = manager.afficherTriangulation ? "Cacher triangulation" : "Afficher fenetrage avec triangulation";
            if (GUILayout.Button(txtTri))
            {
                manager.ToggleAfficherTriangulation();
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("Options : ");

        if (GUILayout.Button("Réinitialiser"))
        {
            manager.ResetProjet();
        }

        if (GUILayout.Button("Réinitialiser Caméra"))
        {
            CameraController camCtrl = FindFirstObjectByType<CameraController>();
            if (camCtrl != null) camCtrl.ResetCamera();
        }

        GUILayout.Space(10);

        GUILayout.Label("Contrôles : ");
        GUILayout.Label("- Zoom : Molette souris");
        GUILayout.Label("- Pan : Clic molette + Drag");

        GUILayout.EndScrollView();
        GUILayout.EndArea();

        //légende séparée (haut droite)
        GUI.Box(legendRect, "Légende");
        GUILayout.BeginArea(new Rect(legendRect.x + 10, legendRect.y + 30, legendRect.width - 20, legendRect.height - 40));
        
        GUI.color = Color.green;
        GUILayout.Label("- Polygone (Vert)");
        GUI.color = Color.blue;
        GUILayout.Label("- Fenêtre (Bleu)");
        GUI.color = Color.red;
        GUILayout.Label("- Résultat (Rouge)");
        GUI.color = Color.yellow;
        GUILayout.Label("- Sélectionné (Jaune)");
        GUI.color = Color.white;

        GUILayout.EndArea();

        GUI.matrix = oldMatrix;
    }

    public bool IsMouseOverUI()
    {
        Vector2 mousePos = Input.mousePosition;
        mousePos.y = Screen.height - mousePos.y;
        return scaledWindowRect.Contains(mousePos) || scaledLegendRect.Contains(mousePos);
    }
}
