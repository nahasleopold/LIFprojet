using UnityEngine;

public class Emplacement : MonoBehaviour
{
    [Header("Adresse")]
    public int allee;
    public string section;
    public int niveau;

    [Header("Placement")]
    [SerializeField]
    private float margeAuDessusPlateau = 0.05f;

    [SerializeField]
    private float distanceRobotRayonnage = 1f;

    // Position où le robot doit s'arrêter
    [SerializeField, HideInInspector]
    private Vector3 positionRobot;


    public int Allee => allee;
    public string Section => section;
    public int Niveau => niveau;


    /*
     * IMPORTANT :
     * RobotController utilise déjà :
     *
     * pieceCourante.EStockage.Position
     *
     * Donc Position représente le point au sol
     * où le robot doit s'arrêter.
     */
    public Vector3 Position
    {
        get { return positionRobot; }
    }


    /*
     * Appelé quand on ajoute le composant
     * Emplacement au GameObject.
     */
    private void Reset()
    {
        PlacerEmplacement();
    }


    /*
     * Permet de mettre à jour les informations
     * si le nom change.
     */
    private void OnValidate()
    {
        LireNom();
    }


    /*
     * Clic droit sur Emplacement dans l'Inspector :
     * "Placer cet emplacement"
     */
    [ContextMenu("Placer cet emplacement")]
    public void PlacerEmplacement()
    {
        if (!LireNom())
            return;


        // ============================
        // 1. Trouver Entrepot
        // ============================

        GameObject racineEntrepot =
            GameObject.Find("Entrepot");

        if (racineEntrepot == null)
        {
            Debug.LogError(
                "GameObject 'Entrepot' introuvable.",
                this
            );

            return;
        }


        // ============================
        // 2. Trouver le rayonnage
        // ============================

        Transform rayonnage =
            racineEntrepot.transform.Find(
                "Rayonnages/A" + allee
            );

        if (rayonnage == null)
        {
            Debug.LogError(
                "Rayonnage A" + allee +
                " introuvable.",
                this
            );

            return;
        }


        // ============================
        // 3. Trouver le plateau
        //
        // niveau 1 -> Plateau_0
        // niveau 2 -> Plateau_1
        // niveau 3 -> Plateau_2
        // niveau 4 -> Plateau_3
        // ============================

        Transform plateau =
            rayonnage.Find(
                "Plateau_" + (niveau - 1)
            );

        if (plateau == null)
        {
            Debug.LogError(
                "Plateau_" + (niveau - 1) +
                " introuvable dans A" +
                allee,
                this
            );

            return;
        }


        // ============================
        // 4. Dimensions du rayonnage
        // ============================

        Renderer[] renderers =
            rayonnage.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            Debug.LogError(
                "Aucun Renderer trouvé dans A" +
                allee,
                this
            );

            return;
        }


        Bounds bounds =
            renderers[0].bounds;

        for (int i = 1;
             i < renderers.Length;
             i++)
        {
            bounds.Encapsulate(
                renderers[i].bounds
            );
        }


        // ============================
        // 5. Calcul de la section A-G
        // ============================

        /*
         *
         *        profondeur du rayonnage
         *
         * A | B | C | D | E | F | G
         *
         */

        int indexSection =
            section[0] - 'A';

        float longueurSection =
            bounds.size.z / 7f;

        float z =
            bounds.min.z +
            longueurSection *
            (indexSection + 0.5f);


        // ============================
        // 6. Position X
        // ============================

        float x =
            bounds.center.x;


        // ============================
        // 7. Hauteur du niveau
        // ============================

        Renderer rendererPlateau =
            plateau.GetComponent<Renderer>();

        float y;

        if (rendererPlateau != null)
        {
            // Juste au-dessus du plateau
            y =
                rendererPlateau.bounds.max.y +
                margeAuDessusPlateau;
        }
        else
        {
            y =
                plateau.position.y +
                margeAuDessusPlateau;
        }


        // ============================
        // 8. Déplacer réellement
        //    A6-A-2
        // ============================

        transform.position =
            new Vector3(
                x,
                y,
                z
            );


        // ============================
        // 9. Position du robot
        // ============================

        /*
         * Selon ton modèle :
         *
         * allée paire  -> accès gauche
         * allée impaire -> accès droite
         */

        float robotX;

        if (allee % 2 == 0)
        {
            robotX =
                bounds.min.x -
                distanceRobotRayonnage;
        }
        else
        {
            robotX =
                bounds.max.x +
                distanceRobotRayonnage;
        }


        positionRobot =
            new Vector3(
                robotX,
                0f,
                z
            );


        Debug.Log(
            gameObject.name +
            " placé dans A" +
            allee +
            " / Section " +
            section +
            " / Niveau " +
            niveau +
            " à " +
            transform.position,
            this
        );
    }


    // ================================
    // Lecture du nom
    // ================================

    private bool LireNom()
    {
        // Exemple : A6-A-2

        string[] parties =
            gameObject.name.Split('-');


        if (parties.Length != 3)
        {
            Debug.LogWarning(
                "Nom invalide : " +
                gameObject.name +
                "\nFormat attendu : A6-A-2",
                this
            );

            return false;
        }


        // ---------- Allée ----------

        if (!parties[0].StartsWith("A"))
            return false;


        if (!int.TryParse(
                parties[0].Substring(1),
                out allee))
        {
            return false;
        }


        if (allee < 1 || allee > 16)
        {
            Debug.LogWarning(
                "Allée invalide : " +
                allee,
                this
            );

            return false;
        }


        // ---------- Section ----------

        section =
            parties[1].ToUpper();


        if (section.Length != 1 ||
            section[0] < 'A' ||
            section[0] > 'G')
        {
            Debug.LogWarning(
                "Section invalide : " +
                section +
                " | Attendu : A à G",
                this
            );

            return false;
        }


        // ---------- Niveau ----------

        if (!int.TryParse(
                parties[2],
                out niveau))
        {
            return false;
        }


        if (niveau < 1 ||
            niveau > 4)
        {
            Debug.LogWarning(
                "Niveau invalide : " +
                niveau +
                " | Attendu : 1 à 4",
                this
            );

            return false;
        }


        return true;
    }


    // Petit repère visible dans Scene
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            0.25f
        );
    }
}