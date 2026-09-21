using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OutilsEditeurEntrepot
{
    /// <summary>
    /// Génère la partie structurelle de l'entrepôt sans détruire les robots
    /// ni le GestionnaireEntrepot déjà présents dans la scène.
    /// </summary>
    public static class GenerateurEntrepot
    {
        public const string NomRacine = "Entrepot";

        public const float LargeurAlleeSimple = 4f;
        public const float LargeurAlleeDouble = 8f;
        public const float ProfondeurAllee = 43f;
        public const float HauteurAllee = 4f;
        public const float LargeurCouloir = 6f;
        public const int NiveauxEtagere = ReglesEntrepot.NombreNiveaux;

        public const float ProfondeurBandeZone = 6f;
        public const float DegagementZone = 14f;
        public const float RetraitCoin = 6f;
        public const float MargeSol = 4f;

        private const string DossierMateriaux = "Assets/Materials";
        private const string DossierTextures = "Assets/Textures";

        [MenuItem("Entrepot/1 - Générer ou mettre à jour l'entrepôt")]
        public static void GenererOuMettreAJour()
        {
            GameObject racine = GameObject.Find(NomRacine);
            if (racine == null)
            {
                racine = new GameObject(NomRacine);
                Undo.RegisterCreatedObjectUndo(racine, "Créer Entrepot");
            }

            Transform rayonnages = ObtenirOuCreerParent(racine.transform, "Rayonnages");
            Transform couloirs = ObtenirOuCreerParent(racine.transform, "Couloirs");
            Transform zones = ObtenirOuCreerParent(racine.transform, "Zone");
            Transform emplacements = ObtenirOuCreerParent(racine.transform, "Emplacements");
            ObtenirOuCreerParent(racine.transform, "Robots");

            // Ces objets servent uniquement de conteneurs. Une ancienne échelle sur l'un
            // d'eux multiplierait la taille de tous leurs enfants générés.
            NormaliserConteneur(rayonnages);
            NormaliserConteneur(couloirs);
            NormaliserConteneur(zones);
            NormaliserConteneur(emplacements);

            // On ne supprime JAMAIS Robots ni GestionnaireEntrepot.
            ViderEnfants(rayonnages);
            ViderEnfants(couloirs);

            var matStructure = ObtenirOuCreerMateriau("Etagere_Structure", new Color(0.28f, 0.29f, 0.31f), 0.6f, 0.4f);
            var matPlateau = ObtenirOuCreerMateriau("Etagere_Plateau", new Color(0.95f, 0.75f, 0.1f), 0.1f, 0.3f);
            var marqMontant = ObtenirOuCreerMateriau("Marquage_Montant", new Color(0.15f, 0.55f, 0.9f), 0f, 0.4f);
            var marqDescendant = ObtenirOuCreerMateriau("Marquage_Descendant", new Color(0.95f, 0.55f, 0.1f), 0f, 0.4f);
            var marqDepart = ObtenirOuCreerMateriau("Marquage_ZoneDepart", new Color(0.25f, 0.75f, 0.35f), 0f, 0.4f);
            var marqDepot = ObtenirOuCreerMateriau("Marquage_ZoneDepot", new Color(0.2f, 0.55f, 0.85f), 0f, 0.4f);
            var textureSol = ObtenirOuCreerTextureSol();
            var matSol = ObtenirOuCreerMateriau("Sol_Beton", Color.white, 0f, 0.15f);
            matSol.mainTexture = textureSol;

            var modules = ReglesEntrepot.ObtenirModules();

            float largeurTotale = 0f;
            foreach (var module in modules)
                largeurTotale += module.Length == 1 ? LargeurAlleeSimple : LargeurAlleeDouble;
            largeurTotale += LargeurCouloir * (modules.Count - 1);

            float zRangeeZone = MargeSol + ProfondeurBandeZone / 2f;
            float zDebutAllees = MargeSol + ProfondeurBandeZone + DegagementZone;
            float zCentreAllee = zDebutAllees + ProfondeurAllee / 2f;
            float profondeurTotale = MargeSol + ProfondeurBandeZone + DegagementZone + ProfondeurAllee + MargeSol;
            float zCentreSol = profondeurTotale / 2f;

            GenererSol(racine.transform, largeurTotale, profondeurTotale, zCentreSol, matSol);

            float curseurX = -largeurTotale / 2f;

            for (int m = 0; m < modules.Count; m++)
            {
                int[] module = modules[m];
                float largeurModule = module.Length == 1 ? LargeurAlleeSimple : LargeurAlleeDouble;
                float largeurAllee = largeurModule / module.Length;

                for (int i = 0; i < module.Length; i++)
                {
                    int numeroAllee = module[i];
                    float x = curseurX + largeurAllee * (i + 0.5f);
                    ConstruireEtagere(
                        rayonnages,
                        $"A{numeroAllee}",
                        largeurAllee * 0.95f,
                        ProfondeurAllee,
                        HauteurAllee,
                        new Vector3(x, 0f, zCentreAllee),
                        matStructure,
                        matPlateau);
                }

                curseurX += largeurModule;

                if (m < modules.Count - 1)
                {
                    int indexCouloir = module[module.Length - 1];
                    GenererMarquageCouloir(couloirs, indexCouloir, curseurX, zCentreAllee, marqMontant, marqDescendant);
                    curseurX += LargeurCouloir;
                }
            }

            Zone zoneDepart = ConfigurerZone(
                zones,
                "ZoneDepart",
                1,
                TypeZone.Depart,
                new Vector3(-largeurTotale / 2f + RetraitCoin + 5f, 0f, zRangeeZone),
                new Vector3(10f, 0.02f, ProfondeurBandeZone * 0.8f),
                marqDepart);

            Zone zoneDepot = ConfigurerZone(
                zones,
                "ZoneDepot",
                2,
                TypeZone.Depot,
                new Vector3(largeurTotale / 2f - RetraitCoin - 4f, 0f, zRangeeZone),
                new Vector3(8f, 0.02f, ProfondeurBandeZone * 0.8f),
                marqDepot);

            ConfigurerGestionnaire(racine.transform, zoneDepart, zoneDepot, rayonnages);

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = racine;

            Debug.Log(
                "Entrepôt généré avec la hiérarchie Entrepot/Rayonnages, Couloirs, Zone, Emplacements et Robots. " +
                "Les robots et le GestionnaireEntrepot existants sont conservés. " +
                "Lance ensuite 'Entrepot > 2 - Générer les emplacements'.");
        }

        private static void GenererSol(Transform racine, float largeur, float profondeur, float zCentre, Material materiau)
        {
            Transform ancien = racine.Find("Sol");
            if (ancien != null)
                Object.DestroyImmediate(ancien.gameObject);

            GameObject sol = GameObject.CreatePrimitive(PrimitiveType.Plane);
            sol.name = "Sol";
            sol.transform.SetParent(racine);
            sol.transform.position = new Vector3(0f, 0f, zCentre);
            sol.transform.localScale = new Vector3(largeur / 10f, 1f, profondeur / 10f);

            const float tailleTuile = 4f;
            materiau.mainTextureScale = new Vector2(largeur / tailleTuile, profondeur / tailleTuile);
            sol.GetComponent<Renderer>().sharedMaterial = materiau;

            NavMeshSurface surface = sol.GetComponent<NavMeshSurface>();
            if (surface == null)
                surface = sol.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
        }

        private static void GenererMarquageCouloir(
            Transform parent,
            int indexCouloir,
            float curseurX,
            float zCentre,
            Material montant,
            Material descendant)
        {
            ReglesEntrepot.SensCouloir sens = ReglesEntrepot.ObtenirSensCouloir(indexCouloir);

            GameObject bande = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bande.name = $"Couloir_A{indexCouloir}_A{indexCouloir + 1}";
            bande.transform.SetParent(parent);
            bande.transform.position = new Vector3(curseurX + LargeurCouloir / 2f, 0.02f, zCentre);
            bande.transform.localScale = new Vector3(0.4f, 0.04f, ProfondeurAllee);
            bande.GetComponent<Renderer>().sharedMaterial =
                sens == ReglesEntrepot.SensCouloir.Montant ? montant : descendant;
        }

        private static Zone ConfigurerZone(
            Transform parentZones,
            string nom,
            int identifiant,
            TypeZone type,
            Vector3 position,
            Vector3 tailleMarquage,
            Material materiau)
        {
            Transform transformZone = parentZones.Find(nom);
            GameObject objetZone;

            if (transformZone == null)
            {
                objetZone = new GameObject(nom);
                objetZone.transform.SetParent(parentZones);
            }
            else
            {
                objetZone = transformZone.gameObject;
                ViderEnfants(objetZone.transform);
            }

            // La Zone est un conteneur logique : elle ne doit jamais porter une échelle.
            objetZone.transform.localRotation = Quaternion.identity;
            objetZone.transform.localScale = Vector3.one;
            objetZone.transform.position = position;

            GameObject marquage = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marquage.name = "Marquage";
            marquage.transform.SetParent(objetZone.transform, false);
            marquage.transform.localPosition = new Vector3(0f, 0.01f, 0f);
            marquage.transform.localRotation = Quaternion.identity;
            marquage.transform.localScale = tailleMarquage;
            marquage.GetComponent<Renderer>().sharedMaterial = materiau;

            Transform entree = new GameObject("PointEntree").transform;
            entree.SetParent(objetZone.transform, false);
            entree.localPosition = Vector3.zero;
            entree.localRotation = Quaternion.identity;
            entree.localScale = Vector3.one;

            Transform sortie = new GameObject("PointSortie").transform;
            sortie.SetParent(objetZone.transform, false);
            sortie.localPosition = Vector3.zero;
            sortie.localRotation = Quaternion.identity;
            sortie.localScale = Vector3.one;

            Zone zone = objetZone.GetComponent<Zone>();
            if (zone == null)
                zone = objetZone.AddComponent<Zone>();

            zone.Configurer(identifiant, type, entree, sortie);
            return zone;
        }

        private static void ConfigurerGestionnaire(Transform racine, Zone depart, Zone depot, Transform rayonnages)
        {
            Transform gestionnaireTransform = racine.Find("GestionnaireEntrepot");
            GameObject gestionnaire;

            if (gestionnaireTransform == null)
            {
                gestionnaire = new GameObject("GestionnaireEntrepot");
                gestionnaire.transform.SetParent(racine);
            }
            else
            {
                gestionnaire = gestionnaireTransform.gameObject;
            }

            Entrepot composant = gestionnaire.GetComponent<Entrepot>();
            if (composant == null)
                composant = gestionnaire.AddComponent<Entrepot>();

            composant.ConfigurerStructure(depart, depot, rayonnages);
            EditorUtility.SetDirty(composant);
        }

        private static Transform ObtenirOuCreerParent(Transform parent, string nom)
        {
            Transform enfant = parent.Find(nom);
            if (enfant != null)
                return enfant;

            GameObject objet = new GameObject(nom);
            objet.transform.SetParent(parent, false);
            objet.transform.localPosition = Vector3.zero;
            objet.transform.localRotation = Quaternion.identity;
            objet.transform.localScale = Vector3.one;
            return objet.transform;
        }

        private static void NormaliserConteneur(Transform conteneur)
        {
            conteneur.localPosition = Vector3.zero;
            conteneur.localRotation = Quaternion.identity;
            conteneur.localScale = Vector3.one;
        }

        private static void ViderEnfants(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }

        private static GameObject ConstruireEtagere(
            Transform parent,
            string nom,
            float largeur,
            float profondeur,
            float hauteur,
            Vector3 position,
            Material materiauStructure,
            Material materiauPlateau)
        {
            GameObject unite = new GameObject(nom);
            unite.transform.SetParent(parent);
            unite.transform.position = position;

            const float epaisseurMontant = 0.12f;
            float[] xs = { -(largeur / 2f - epaisseurMontant / 2f), largeur / 2f - epaisseurMontant / 2f };
            float[] zs = { -(profondeur / 2f - epaisseurMontant / 2f), profondeur / 2f - epaisseurMontant / 2f };

            foreach (float x in xs)
            {
                foreach (float z in zs)
                {
                    GameObject montant = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    montant.name = "Montant";
                    montant.transform.SetParent(unite.transform);
                    montant.transform.localPosition = new Vector3(x, hauteur / 2f, z);
                    montant.transform.localScale = new Vector3(epaisseurMontant, hauteur, epaisseurMontant);
                    montant.GetComponent<Renderer>().sharedMaterial = materiauStructure;
                }
            }

            for (int niveau = 0; niveau < NiveauxEtagere; niveau++)
            {
                float y = 0.1f + (hauteur - 0.2f) * niveau / (NiveauxEtagere - 1);
                GameObject plateau = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plateau.name = $"Plateau_{niveau}";
                plateau.transform.SetParent(unite.transform);
                plateau.transform.localPosition = new Vector3(0f, y, 0f);
                plateau.transform.localScale = new Vector3(largeur, 0.08f, profondeur);
                plateau.GetComponent<Renderer>().sharedMaterial = materiauPlateau;
            }

            return unite;
        }

        private static Material ObtenirOuCreerMateriau(string nom, Color couleur, float metallique, float lissage)
        {
            AssurerDossier(DossierMateriaux);
            string chemin = $"{DossierMateriaux}/{nom}.mat";
            Material existant = AssetDatabase.LoadAssetAtPath<Material>(chemin);
            if (existant != null)
                return existant;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material materiau = new Material(shader) { color = couleur };

            if (materiau.HasProperty("_Metallic")) materiau.SetFloat("_Metallic", metallique);
            if (materiau.HasProperty("_Smoothness")) materiau.SetFloat("_Smoothness", lissage);
            if (materiau.HasProperty("_Glossiness")) materiau.SetFloat("_Glossiness", lissage);

            AssetDatabase.CreateAsset(materiau, chemin);
            return materiau;
        }

        private static Texture2D ObtenirOuCreerTextureSol()
        {
            AssurerDossier(DossierTextures);
            string cheminAsset = $"{DossierTextures}/Sol_Beton.png";
            Texture2D existante = AssetDatabase.LoadAssetAtPath<Texture2D>(cheminAsset);
            if (existante != null)
                return existante;

            const int taille = 256;
            Texture2D texture = new Texture2D(taille, taille, TextureFormat.RGBA32, false);
            var aleatoire = new System.Random(12345);

            for (int y = 0; y < taille; y++)
            {
                for (int x = 0; x < taille; x++)
                {
                    float bruit = Mathf.PerlinNoise(x * 0.06f, y * 0.06f) * 0.5f
                                + Mathf.PerlinNoise(x * 0.2f, y * 0.2f) * 0.3f;
                    float tache = aleatoire.NextDouble() < 0.02 ? (float)aleatoire.NextDouble() * 0.15f : 0f;
                    float gris = Mathf.Clamp01(0.5f + (bruit - 0.4f) * 0.25f - tache);
                    texture.SetPixel(x, y, new Color(gris, gris, gris * 1.02f));
                }
            }

            texture.Apply();

            string cheminSysteme = Path.Combine(Application.dataPath, "Textures/Sol_Beton.png");
            File.WriteAllBytes(cheminSysteme, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(cheminAsset, ImportAssetOptions.ForceUpdate);
            TextureImporter importeur = (TextureImporter)AssetImporter.GetAtPath(cheminAsset);
            importeur.wrapMode = TextureWrapMode.Repeat;
            importeur.filterMode = FilterMode.Bilinear;
            importeur.mipmapEnabled = true;
            importeur.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Texture2D>(cheminAsset);
        }

        private static void AssurerDossier(string chemin)
        {
            if (AssetDatabase.IsValidFolder(chemin))
                return;

            string parent = Path.GetDirectoryName(chemin)?.Replace("\\", "/");
            string nomDossier = Path.GetFileName(chemin);

            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                AssurerDossier(parent);

            AssetDatabase.CreateFolder(parent, nomDossier);
        }
    }
}
