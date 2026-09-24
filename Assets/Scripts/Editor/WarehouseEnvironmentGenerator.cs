using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Warehouse;

namespace Warehouse.EditorTools
{
    /// <summary>
    /// Génère la géométrie de base de l'entrepôt (sol texturé + marquage au sol, rayonnages
    /// A1-A16 sous forme d'étagères, zone de départ des robots, zone de dépôt) dans la scène
    /// actuellement ouverte — SANS placer aucune pièce dans les rayonnages.
    ///
    /// Utilisation : ouvrez ou créez la scène de l'entrepôt dans l'éditeur Unity, puis
    /// menu "Entrepot > Générer l'environnement (sans pièces)". Relancer la commande
    /// remplace l'environnement précédemment généré ; les matériaux et la texture de sol
    /// générés sont réutilisés (pas de doublons créés à chaque relance).
    ///
    /// Les dimensions ci-dessous sont des valeurs de départ raisonnables : à ajuster
    /// selon l'échelle réelle indiquée sur le plan (légende ~90m x 43m).
    /// Passer en internal const pour pouvoir les utiliser dans les autre generator
    /// </summary>
    public static class WarehouseEnvironmentGenerator
    {
        internal const string RootName = "Entrepot_Environnement";

        internal const float SingleAisleWidth = 4f;   // largeur d'une allée simple (A1, A16)
        internal const float DoubleAisleWidth = 8f;    // largeur totale d'un module à 2 allées (4m chacune)
        internal  const float AisleDepth = 43f;         // profondeur des rayonnages (cf. légende du plan)
        internal const float AisleHeight = 4f;
        internal const float CorridorWidth = 6f;

        internal const int ShelfLevels = 4;
 
        internal const float ZoneStripDepth = 6f;      // profondeur de la bande où vivent les zones départ/dépôt
        internal const float ZoneClearance = 14f;      // <-- espace vide entre les zones et la première allée
        internal const float CornerInset = 6f;         // marge entre le bord latéral de l'entrepôt et le pad de zone
        internal const float FloorMargin = 4f;         // marge de sol au-delà des zones et des allées

        private const int DefaultRobotCount = 5;
        private const float RobotSpacing = 2f;

        private const string MaterialsFolder = "Assets/Materials";
        private const string TexturesFolder = "Assets/Textures";

        [MenuItem("Entrepot/Générer l'environnement (sans pièces)")]
        public static void GenerateEnvironment()
        {
            var existingRoot = GameObject.Find(RootName);
            if (existingRoot != null)
            {
                if (!EditorUtility.DisplayDialog(
                        "Régénérer l'entrepôt",
                        "Un environnement généré existe déjà dans cette scène. Le remplacer ?",
                        "Remplacer", "Annuler"))
                    return;

                Object.DestroyImmediate(existingRoot);
            }

            // --- Matériaux et texture (créés une fois, réutilisés aux relances) ---
            var frameMat = GetOrCreateMaterial("Etagere_Structure", new Color(0.28f, 0.29f, 0.31f), 0.6f, 0.4f);
            var shelfMat = GetOrCreateMaterial("Etagere_Plateau", new Color(0.95f, 0.75f, 0.1f), 0.1f, 0.3f);
            var markMontant = GetOrCreateMaterial("Marquage_Montant", new Color(0.15f, 0.55f, 0.9f), 0f, 0.4f);
            var markDescendant = GetOrCreateMaterial("Marquage_Descendant", new Color(0.95f, 0.55f, 0.1f), 0f, 0.4f);
            var markDepart = GetOrCreateMaterial("Marquage_ZoneDepart", new Color(0.25f, 0.75f, 0.35f), 0f, 0.4f);
            var markDepot = GetOrCreateMaterial("Marquage_ZoneDepot", new Color(0.2f, 0.55f, 0.85f), 0f, 0.4f);
            var floorTexture = GetOrCreateFloorTexture();
            var floorMat = GetOrCreateMaterial("Sol_Beton", Color.white, 0f, 0.15f);
            floorMat.mainTexture = floorTexture;

            var root = new GameObject(RootName);
            var modules = WarehouseLayout.GetModules();

            float totalWidth = 0f;
            foreach (var module in modules)
                totalWidth += module.Length == 1 ? SingleAisleWidth : DoubleAisleWidth;
            totalWidth += CorridorWidth * (modules.Count - 1);

            // --- Disposition en profondeur (Z) : bande des zones -> espace vide -> allées ---
            float zoneRowZ = FloorMargin + ZoneStripDepth / 2f;                 // centre de la bande départ/dépôt
            float aislesStartZ = FloorMargin + ZoneStripDepth + ZoneClearance;  // début des rayonnages
            float aisleCenterZ = aislesStartZ + AisleDepth / 2f;
            float totalDepth = FloorMargin + ZoneStripDepth + ZoneClearance + AisleDepth + FloorMargin;
            float floorCenterZ = totalDepth / 2f;

            float cursorX = -totalWidth / 2f;

            // --- Sol texturé + NavMeshSurface (à bake-r manuellement : Window > AI > Navigation) ---
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Sol";
            floor.transform.SetParent(root.transform);
            floor.transform.position = new Vector3(0f, 0f, floorCenterZ);
            floor.transform.localScale = new Vector3(totalWidth / 10f, 1f, totalDepth / 10f);
            const float tileSize = 4f; // 1 tuile de texture tous les 4m, pour éviter un étirement
            floorMat.mainTextureScale = new Vector2(totalWidth / tileSize, totalDepth / tileSize);
            floor.GetComponent<Renderer>().sharedMaterial = floorMat;
            var navSurface = floor.AddComponent<NavMeshSurface>();
            navSurface.collectObjects = CollectObjects.All;

            // --- Allées / étagères (A1 à A16), sans aucune pièce à l'intérieur ---
            for (int m = 0; m < modules.Count; m++)
            {
                var module = modules[m];
                float moduleWidth = module.Length == 1 ? SingleAisleWidth : DoubleAisleWidth;
                float aisleWidth = moduleWidth / module.Length;

                for (int i = 0; i < module.Length; i++)
                {
                    int aisleNumber = module[i];
                    float x = cursorX + aisleWidth * (i + 0.5f);
                    var position = new Vector3(x, 0f, aisleCenterZ);
                    BuildShelfUnit(root.transform, $"A{aisleNumber}", aisleWidth * 0.95f, AisleDepth, AisleHeight,
                        position, frameMat, shelfMat);
                }

                cursorX += moduleWidth;

                bool isLastModule = m == modules.Count - 1;
                if (!isLastModule)
                {
                    int corridorIndex = module[module.Length - 1]; // n = dernière allée du module courant
                    var direction = WarehouseLayout.GetCorridorDirection(corridorIndex);

                    var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    stripe.name = $"Marquage_Couloir_A{corridorIndex}_A{corridorIndex + 1}";
                    stripe.transform.SetParent(root.transform);
                    float corridorX = cursorX + CorridorWidth / 2f;
                    stripe.transform.position = new Vector3(corridorX, 0.02f, aisleCenterZ);
                    stripe.transform.localScale = new Vector3(0.4f, 0.04f, AisleDepth);
                    stripe.GetComponent<Renderer>().sharedMaterial =
                        direction == WarehouseLayout.CorridorDirection.Montant ? markMontant : markDescendant;

                    var marker = stripe.AddComponent<CorridorMarker>();
                    marker.corridorIndex = corridorIndex;
                    marker.direction = direction;

                    cursorX += CorridorWidth;
                }
            }

            // --- Zone de départ des robots (R1..Rn), coin gauche de la même bande, + marquage au sol ---
            float departPadWidth = (DefaultRobotCount - 1) * RobotSpacing + 2f;
            float departX = -totalWidth / 2f + CornerInset + departPadWidth / 2f;

            var departZone = new GameObject("ZoneDepart");
            departZone.transform.SetParent(root.transform);
            departZone.transform.position = new Vector3(departX, 0f, zoneRowZ);

            var departPad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            departPad.name = "Marquage_ZoneDepart";
            departPad.transform.SetParent(departZone.transform);
            departPad.transform.localPosition = new Vector3(0f, 0.01f, 0f);
            departPad.transform.localScale = new Vector3(departPadWidth, 0.02f, ZoneStripDepth * 0.8f);
            departPad.GetComponent<Renderer>().sharedMaterial = markDepart;

            for (int i = 1; i <= DefaultRobotCount; i++)
            {
                var marker = new GameObject($"R{i}");
                marker.transform.SetParent(departZone.transform);
                float localX = (i - (DefaultRobotCount + 1) / 2f) * RobotSpacing;
                marker.transform.localPosition = new Vector3(localX, 0f, 0f);
            }

            // --- Zone de dépôt, coin droit de la même bande, + marquage au sol ---
            float depotPadWidth = 8f;
            float depotX = totalWidth / 2f - CornerInset - depotPadWidth / 2f;

            var depotZone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            depotZone.name = "ZoneDepot";
            depotZone.transform.SetParent(root.transform);
            depotZone.transform.position = new Vector3(depotX, 0.01f, zoneRowZ);
            depotZone.transform.localScale = new Vector3(depotPadWidth, 0.02f, ZoneStripDepth * 0.8f);
            depotZone.GetComponent<Renderer>().sharedMaterial = markDepot;

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = root;

            Debug.Log("Environnement de l'entrepôt généré (étagères + sol texturé + marquage, sans pièces en stock). " +
                      "Matériaux dans Assets/Materials, texture de sol dans Assets/Textures. Pensez à : " +
                      "1) enregistrer la scène (Ctrl+S), " +
                      "2) lancer le bake du NavMesh (Window > AI > Navigation > Bake), " +
                      "3) ajuster les dimensions dans WarehouseEnvironmentGenerator selon l'échelle réelle du plan (90m x 43m).");
        }

        /// <summary>Construit une étagère schématique (montants + plateaux) plutôt qu'un simple bloc plein.</summary>
        private static GameObject BuildShelfUnit(Transform parent, string name, float width, float depth,
            float height, Vector3 position, Material frameMaterial, Material shelfMaterial)
        {
            var unit = new GameObject(name);
            unit.transform.SetParent(parent);
            unit.transform.position = position;

            const float postThickness = 0.12f;

            float[] xs = { -(width / 2f - postThickness / 2f), (width / 2f - postThickness / 2f) };
            float[] zs = { -(depth / 2f - postThickness / 2f), (depth / 2f - postThickness / 2f) };

            foreach (var x in xs)
            {
                foreach (var z in zs)
                {
                    var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    post.name = "Montant";
                    post.transform.SetParent(unit.transform);
                    post.transform.localPosition = new Vector3(x, height / 2f, z);
                    post.transform.localScale = new Vector3(postThickness, height, postThickness);
                    post.GetComponent<Renderer>().sharedMaterial = frameMaterial;
                }
            }

            for (int level = 0; level < ShelfLevels; level++)
            {
                float y = 0.1f + (height - 0.2f) * level / (ShelfLevels - 1);
                var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
                board.name = $"Plateau_{level}";
                board.transform.SetParent(unit.transform);
                board.transform.localPosition = new Vector3(0f, y, 0f);
                board.transform.localScale = new Vector3(width, 0.08f, depth);
                board.GetComponent<Renderer>().sharedMaterial = shelfMaterial;
            }

            return unit;
        }

        /// <summary>Charge le matériau s'il existe déjà (Assets/Materials/{name}.mat), sinon le crée.</summary>
        private static Material GetOrCreateMaterial(string assetName, Color color, float metallic, float smoothness)
        {
            EnsureFolder(MaterialsFolder);
            string path = $"{MaterialsFolder}/{assetName}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
                return existing;

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader) { color = color };
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);

            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        /// <summary>Génère (une seule fois) une texture de sol béton procédurale et l'importe comme asset.</summary>
        private static Texture2D GetOrCreateFloorTexture()
        {
            EnsureFolder(TexturesFolder);
            string assetPath = $"{TexturesFolder}/Sol_Beton.png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (existing != null)
                return existing;

            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var rng = new System.Random(12345);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.06f, y * 0.06f) * 0.5f
                            + Mathf.PerlinNoise(x * 0.2f, y * 0.2f) * 0.3f;
                    float speck = rng.NextDouble() < 0.02 ? (float)rng.NextDouble() * 0.15f : 0f;
                    float grey = Mathf.Clamp01(0.5f + (n - 0.4f) * 0.25f - speck);
                    tex.SetPixel(x, y, new Color(grey, grey, grey * 1.02f));
                }
            }
            tex.Apply();

            string systemPath = Path.Combine(Application.dataPath, "Textures/Sol_Beton.png");
            File.WriteAllBytes(systemPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string folderName = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
