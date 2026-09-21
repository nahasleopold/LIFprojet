using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class OutilsEmplacements
{
    private const float MargeAuDessusPlateau = 0.05f;
    private const float DistanceRobotRayonnage = 1f;

    [MenuItem("Entrepot/2 - Generer les emplacements")]
    public static void GenererLesEmplacements()
    {
        GameObject racineEntrepot = GameObject.Find("Entrepot");

        if (racineEntrepot == null)
        {
            Debug.LogError("GameObject 'Entrepot' introuvable dans la scene.");
            return;
        }

        Transform rayonnages = racineEntrepot.transform.Find("Rayonnages");

        if (rayonnages == null)
        {
            Debug.LogError("Entrepot/Rayonnages introuvable.");
            return;
        }

        Transform emplacementsExistants = racineEntrepot.transform.Find("Emplacements");

        if (emplacementsExistants != null)
        {
            bool remplacer = EditorUtility.DisplayDialog(
                "Regenerer les emplacements",
                "Des emplacements existent deja. Voulez-vous les remplacer ?",
                "Remplacer",
                "Annuler");

            if (!remplacer)
            {
                return;
            }

            Undo.DestroyObjectImmediate(emplacementsExistants.gameObject);
        }

        GameObject racineEmplacements = new GameObject("Emplacements");
        Undo.RegisterCreatedObjectUndo(racineEmplacements, "Generer les emplacements");
        racineEmplacements.transform.SetParent(racineEntrepot.transform);

        int nombreCrees = 0;

        for (int allee = 1; allee <= ReglesEntrepot.NombreAllees; allee++)
        {
            Transform rayonnage = rayonnages.Find($"A{allee}");

            if (rayonnage == null)
            {
                Debug.LogWarning($"Rayonnage A{allee} introuvable. Il est ignore.");
                continue;
            }

            if (!EssayerCalculerBornes(rayonnage, out Bounds bornes))
            {
                Debug.LogWarning($"Impossible de calculer les dimensions de A{allee}.");
                continue;
            }

            for (int indexSection = 0;
                 indexSection < ReglesEntrepot.NombreSections;
                 indexSection++)
            {
                SectionEntrepot section = (SectionEntrepot)indexSection;

                float longueurSection = bornes.size.z / ReglesEntrepot.NombreSections;
                float z = bornes.min.z
                    + longueurSection * (indexSection + 0.5f);

                for (int niveau = 1;
                     niveau <= ReglesEntrepot.NombreNiveaux;
                     niveau++)
                {
                    Transform plateau = rayonnage.Find($"Plateau_{niveau - 1}");

                    if (plateau == null)
                    {
                        Debug.LogWarning(
                            $"Plateau_{niveau - 1} introuvable dans A{allee}.");
                        continue;
                    }

                    float y = CalculerHauteurStockage(plateau);
                    float xStockage = bornes.center.x;

                    Vector3 positionStockage = new Vector3(
                        xStockage,
                        y,
                        z);

                    float xRobot = ReglesEntrepot.EstAccessibleParGauche(allee)
                        ? bornes.min.x - DistanceRobotRayonnage
                        : bornes.max.x + DistanceRobotRayonnage;

                    Vector3 positionAccesRobot = new Vector3(
                        xRobot,
                        0f,
                        z);

                    GameObject objet = new GameObject(
                        $"A{allee}-{section}-{niveau}");

                    Undo.RegisterCreatedObjectUndo(objet, "Generer un emplacement");

                    objet.transform.SetParent(racineEmplacements.transform);
                    objet.transform.position = positionStockage;

                    Emplacement emplacement = objet.AddComponent<Emplacement>();
                    emplacement.Initialiser(
                        allee,
                        section,
                        niveau,
                        positionAccesRobot);

                    nombreCrees++;
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(
            EditorSceneManager.GetActiveScene());

        Selection.activeGameObject = racineEmplacements;

        Debug.Log($"{nombreCrees} emplacements generes dans Entrepot/Emplacements.");
    }

    private static bool EssayerCalculerBornes(
        Transform rayonnage,
        out Bounds bornes)
    {
        Renderer[] rendus = rayonnage.GetComponentsInChildren<Renderer>();

        if (rendus.Length == 0)
        {
            bornes = default;
            return false;
        }

        bornes = rendus[0].bounds;

        for (int i = 1; i < rendus.Length; i++)
        {
            bornes.Encapsulate(rendus[i].bounds);
        }

        return true;
    }

    private static float CalculerHauteurStockage(Transform plateau)
    {
        Renderer renduPlateau = plateau.GetComponent<Renderer>();

        if (renduPlateau != null)
        {
            return renduPlateau.bounds.max.y + MargeAuDessusPlateau;
        }

        return plateau.position.y + MargeAuDessusPlateau;
    }
}
