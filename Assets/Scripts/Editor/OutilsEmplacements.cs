using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OutilsEditeurEntrepot
{
    public static class OutilsEmplacements
    {
        [MenuItem("Entrepot/2 - Générer les emplacements")]
        public static void GenererTousLesEmplacements()
        {
            GameObject racine = GameObject.Find(GenerateurEntrepot.NomRacine);
            if (racine == null)
            {
                Debug.LogError("Entrepôt introuvable. Lance d'abord la génération de l'entrepôt.");
                return;
            }

            Transform rayonnages = racine.transform.Find("Rayonnages");
            if (rayonnages == null)
            {
                Debug.LogError("Entrepot/Rayonnages introuvable.");
                return;
            }

            Transform parentEmplacements = racine.transform.Find("Emplacements");
            if (parentEmplacements == null)
            {
                GameObject parent = new GameObject("Emplacements");
                parent.transform.SetParent(racine.transform);
                parentEmplacements = parent.transform;
            }

            if (parentEmplacements.childCount > 0)
            {
                if (!EditorUtility.DisplayDialog(
                        "Régénérer les emplacements",
                        "Les emplacements existants seront remplacés. Continuer ?",
                        "Remplacer",
                        "Annuler"))
                    return;

                for (int i = parentEmplacements.childCount - 1; i >= 0; i--)
                    Undo.DestroyObjectImmediate(parentEmplacements.GetChild(i).gameObject);
            }

            int compteur = 0;

            for (int allee = 1; allee <= ReglesEntrepot.NombreAllees; allee++)
            {
                Transform rayonnage = rayonnages.Find($"A{allee}");
                if (rayonnage == null)
                {
                    Debug.LogWarning($"A{allee} introuvable : emplacements ignorés pour cette allée.");
                    continue;
                }

                for (int indexSection = 0; indexSection < ReglesEntrepot.NombreSections; indexSection++)
                {
                    string section = ((char)('A' + indexSection)).ToString();

                    for (int niveau = 1; niveau <= ReglesEntrepot.NombreNiveaux; niveau++)
                    {
                        string nom = $"A{allee}-{section}-{niveau}";
                        GameObject objet = new GameObject(nom);
                        Undo.RegisterCreatedObjectUndo(objet, "Créer emplacement");
                        objet.transform.SetParent(parentEmplacements);

                        Emplacement emplacement = objet.AddComponent<Emplacement>();
                        emplacement.Initialiser(allee, section, niveau);
                        emplacement.PlacerEmplacement();
                        EditorUtility.SetDirty(emplacement);

                        compteur++;
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"{compteur} emplacements générés sous Entrepot/Emplacements.");
        }
    }
}
