using UnityEngine;

public class Entrepot : MonoBehaviour
{
    [Header("Robot")]
    [SerializeField]
    private RobotController robot;


    [Header("Zones principales")]
    [SerializeField]
    private Zone zoneDepart;

    [SerializeField]
    private Zone zoneDepot;


    [Header("Rayonnages")]
    [SerializeField]
    private Transform rayonnages;

    [Header("Test déplacement simple")]
    [SerializeField]
    private bool testerDeplacementSimple = false;

    [SerializeField]
    private Emplacement emplacementTest;


    [Header("Test V1")]
    [SerializeField]
    private bool lancerTestAuDemarrage = false;

    [SerializeField]
    private int idCommandeTest = 1;

    [SerializeField]
    private Piece pieceTest;


    public Zone ZoneDepart
    {
        get { return zoneDepart; }
    }

    public Zone ZoneDepot
    {
        get { return zoneDepot; }
    }


    private void Reset()
    {
        TrouverRayonnages();
    }


    private void OnValidate()
    {
        TrouverRayonnages();
    }


    private void TrouverRayonnages()
    {
        if (rayonnages != null)
            return;

        // Cherche d'abord le parent "Entrepot"
        Transform courant = transform;

        while (courant != null)
        {
            if (courant.name == "Entrepot")
            {
                rayonnages = courant.Find("Rayonnages");
                break;
            }

            courant = courant.parent;
        }

        // Sécurité si Entrepot.cs est sur GestionnaireEntrepot
        if (rayonnages == null)
        {
            GameObject racine = GameObject.Find("Entrepot");

            if (racine != null)
            {
                rayonnages =
                    racine.transform.Find("Rayonnages");
            }
        }
    }


    public bool TryGetPositionEmplacement(
    int allee,
    char section,
    int niveau,
    out Vector3 position)
    {
        position = Vector3.zero;

        TrouverRayonnages();

        if (rayonnages == null)
        {
            Debug.LogError(
                "Impossible de trouver Entrepot/Rayonnages."
            );

            return false;
        }


        // Vérification allée
        if (allee < 1 || allee > 16)
        {
            Debug.LogError(
                "Allée invalide : A" + allee
            );

            return false;
        }


        // Vérification section A-G
        if (section < 'A' || section > 'G')
        {
            Debug.LogError(
                "Section invalide : " + section
            );

            return false;
        }


        // Vérification niveau 1-4
        if (niveau < 1 || niveau > 4)
        {
            Debug.LogError(
                "Niveau invalide : " + niveau
            );

            return false;
        }


        // Recherche du rayonnage
        Transform rayonnage =
            rayonnages.Find("A" + allee);

        if (rayonnage == null)
        {
            Debug.LogError(
                "Rayonnage A" + allee +
                " introuvable."
            );

            return false;
        }


        // Recherche du plateau correspondant
        Transform plateau =
            rayonnage.Find(
                "Plateau_" + (niveau - 1)
            );

        if (plateau == null)
        {
            Debug.LogError(
                "Plateau du niveau " +
                niveau +
                " introuvable dans A" +
                allee
            );

            return false;
        }


        // Dimensions du rayonnage
        Renderer[] renderers =
            rayonnage.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return false;


        Bounds bounds = renderers[0].bounds;

        for (int i = 1;
             i < renderers.Length;
             i++)
        {
            bounds.Encapsulate(
                renderers[i].bounds
            );
        }


        /*
         * SECTION A-G
         *
         * On partage la profondeur du rayonnage
         * en 7 parties égales.
         */

        int indexSection =
            section - 'A';

        float longueurSection =
            bounds.size.z / 7f;

        float z =
            bounds.min.z +
            longueurSection *
            (indexSection + 0.5f);


        /*
         * X = centre du rayonnage.
         */
        float x =
            bounds.center.x;


        /*
         * Y = hauteur réelle du plateau.
         */
        float y =
            plateau.position.y;


        position =
            new Vector3(
                x,
                y,
                z
            );


        return true;
    }


    private void Awake()
    {
        if (robot != null)
        {
            robot.Initialiser(this);
        }
    }


    private void Start()
    {
        // ==============================
        // TEST SIMPLE SANS PIECE
        // ==============================

        if (testerDeplacementSimple)
        {
            if (robot == null)
            {
                Debug.LogError(
                    "Aucun robot assigné."
                );

                return;
            }

            if (emplacementTest == null)
            {
                Debug.LogError(
                    "Aucun emplacement de test assigné."
                );

                return;
            }

            Debug.Log(
                "Test déplacement vers : " +
                emplacementTest.name
            );

            robot.AllerVers(
                emplacementTest.Position
            );

            return;
        }


        // ==============================
        // TON ANCIEN TEST AVEC COMMANDE
        // ==============================

        if (!lancerTestAuDemarrage)
            return;

        if (robot == null)
        {
            Debug.LogError(
                "Aucun robot n'est assigné à l'entrepôt."
            );

            return;
        }

        if (zoneDepart == null || zoneDepot == null)
        {
            Debug.LogError(
                "La zone de départ ou la zone de dépôt n'est pas configurée."
            );

            return;
        }

        if (pieceTest == null ||
            pieceTest.EStockage == null)
        {
            Debug.LogError(
                "La pièce de test ou son emplacement n'est pas configuré."
            );

            return;
        }

        Commande commande =
            new Commande(idCommandeTest);

        commande.AjouterPiece(pieceTest);

        robot.DemarrerCommande(commande);
    }
}