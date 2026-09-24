using UnityEngine;
using UnityEditor;

namespace Warehouse.EditorTools
{
    public static class EmplacementGenerator
    {
        private const int SectionCount = 7; // A à G, propre à Emplacement (n'existe pas ailleurs)

        [MenuItem("Entrepot/Générer les emplacements")]
        public static void GenerateAll()
        {
            ///test si l'entrepot est deja generé sinon erreur
            GameObject root = GameObject.Find(WarehouseEnvironmentGenerator.RootName);
            if (root == null)
            {
                Debug.LogError("Entrepôt introuvable. Génère d'abord l'environnement.");
                return;
            }

            ///eviter la génération de doublons
            Transform existing = root.transform.Find("Emplacements");
            if (existing != null)
            {
            if (!EditorUtility.DisplayDialog(
                    "Régénérer les emplacements",
                    "Des emplacements existent déjà dans cette scène. Les remplacer ?",
                    "Remplacer", "Annuler"))
                return;

                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject emplacementsRoot = new GameObject("Emplacements");
            emplacementsRoot.transform.SetParent(root.transform);

            for (int allee = 1; allee <= WarehouseLayout.AisleCount; allee++)
            {
                Transform aisleTransform = root.transform.Find($"A{allee}");
                if (aisleTransform == null) continue;

                for (int s = 0; s < SectionCount; s++)
                {
                    Section section = (Section)s;

                    for (int niveau = 1; niveau <= WarehouseEnvironmentGenerator.ShelfLevels; niveau++)
                    {
                        Vector3 pos = CalculatePosition(aisleTransform, s, niveau);

                        GameObject go = new GameObject($"Emplacement_A{allee}-{section}-{niveau}");
                        go.transform.SetParent(emplacementsRoot.transform);
                        go.transform.position = pos;

                        Emplacement emp = go.AddComponent<Emplacement>();
                        emp.Initialize(allee, section, niveau);
                    }
                }
            }

            Debug.Log("Emplacements générés.");
        }

        private static Vector3 CalculatePosition(Transform aisleTransform, int sectionIndex, int niveau)
        {
            float depth = WarehouseEnvironmentGenerator.AisleDepth;
            float height = WarehouseEnvironmentGenerator.AisleHeight;
            int levelCount = WarehouseEnvironmentGenerator.ShelfLevels;

            float zLocal = -depth / 2f + depth * (sectionIndex + 0.5f) / SectionCount;
            float yLocal = 0.1f + (height - 0.2f) * (niveau - 1) / (float)(levelCount - 1);

            return aisleTransform.position + new Vector3(0f, yLocal, zLocal);
        }
    }
}