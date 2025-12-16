using UnityEngine;

public class UIManager : MonoBehaviour
{
    private FenetrageManager manager;
    private Rect windowRect = new Rect(10, 10, 200, 300);
    private float uiScale = 2.5f;

    private void Start()
    {
        manager = FindFirstObjectByType<FenetrageManager>();
    }

    private void OnGUI()
    {
        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(uiScale, uiScale, 1.0f));

        GUI.Box(windowRect, "Menu");

        GUILayout.BeginArea(new Rect(windowRect.x + 10, windowRect.y + 30, windowRect.width - 20, windowRect.height - 40));

        GUILayout.Label("Actions : ");

        if (GUILayout.Button("Nouveau Polygone"))
        {
            manager.pointsPolygone.Clear();
            manager.polygoneFerme = false;
            manager.pointsDecoupes.Clear();
            manager.SetMode(ModeApplication.SaisiePolygone);
        }

        if (GUILayout.Button("Nouvelle Fenêtre"))
        {
            manager.pointsFenetre.Clear();
            manager.fenetreFermee = false;
            manager.pointsDecoupes.Clear();
            manager.SetMode(ModeApplication.SaisieFenetre);
        }

        GUILayout.Space(10);
        GUILayout.Label("Mode Actuel : " + manager.modeActuel);

        if (GUILayout.Button("Mode Édition / Déplacement"))
        {
            manager.SetMode(ModeApplication.Edition);
        }

        GUILayout.Space(10);
        GUILayout.Label("Options : ");

        if (GUILayout.Button("Réinitialiser"))
        {
            manager.ResetProjet();
        }

        GUILayout.Space(10);
        
        GUILayout.Label("Légende : ");
        GUI.color = Color.green;
        GUILayout.Label("- Polygone (Vert)");
        GUI.color = Color.blue;
        GUILayout.Label("- Fenêtre (Bleu)");
        GUI.color = Color.red;
        GUILayout.Label("- Résultat (Rouge)");
        GUI.color = Color.white;
        
        GUILayout.EndArea();

        GUI.matrix = oldMatrix;
    }
}
