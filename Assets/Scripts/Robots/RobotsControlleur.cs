using UnityEngine;

[RequireComponent(typeof(RobotNavigation))]
public class RobotController : MonoBehaviour
{
    [SerializeField]
    private int idr;

    [SerializeField]
    private float capacite = 20f;


    private RobStatut statut = RobStatut.Libre;

    private Commande cmdActuel;

    private Piece pieceCourante;

    private RobotNavigation navigation;

    private Entrepot entrepot;


    public int Idr
    {
        get { return idr; }
        set { idr = value; }
    }

    public float Capacite
    {
        get { return capacite; }
        set { capacite = value; }
    }

    public RobStatut Statut
    {
        get { return statut; }
    }

    public Commande CmdActuel
    {
        get { return cmdActuel; }
    }


    private void Awake()
    {
        navigation = GetComponent<RobotNavigation>();
    }


    public void Initialiser(Entrepot entrepot)
    {
        this.entrepot = entrepot;
    }


    public void DemarrerCommande(Commande commande)
    {
        if (commande == null)
            return;

        if (statut != RobStatut.Libre)
            return;

        if (entrepot == null)
        {
            Debug.LogError(
                "Le robot n'est associé à aucun Entrepot."
            );

            return;
        }

        cmdActuel = commande;

        cmdActuel.Demarrer();

        pieceCourante = cmdActuel.Traitement();

        if (pieceCourante == null)
        {
            Debug.LogWarning(
                "La commande ne contient aucune pièce."
            );

            cmdActuel = null;
            statut = RobStatut.Libre;

            return;
        }

        if (pieceCourante.EStockage == null)
        {
            Debug.LogError(
                "La pièce ne possède pas d'emplacement de stockage."
            );

            cmdActuel = null;
            pieceCourante = null;
            statut = RobStatut.Libre;

            return;
        }

        statut = RobStatut.Vers_prod;

        AllerVers(
            pieceCourante.EStockage.Position
        );
    }


    private void Update()
    {
        switch (statut)
        {
            case RobStatut.Vers_prod:

                if (navigation.EstArrive())
                {
                    PrendrePc();

                    statut = RobStatut.Vers_depot;

                    AllerVers(
                        entrepot.ZoneDepot.Entree
                    );
                }

                break;


            case RobStatut.Vers_depot:

                if (navigation.EstArrive())
                {
                    DeposerPc();

                    cmdActuel.Finaliser();

                    statut = RobStatut.Retour;

                    AllerVers(
                        entrepot.ZoneDepart.Entree
                    );
                }

                break;


            case RobStatut.Retour:

                if (navigation.EstArrive())
                {
                    statut = RobStatut.Libre;

                    cmdActuel = null;
                    pieceCourante = null;

                    Debug.Log(
                        "Robot " + idr +
                        " revenu à la zone de départ."
                    );
                }

                break;
        }
    }


    public void AllerVers(Vector3 destination)
    {
        navigation.AllerVers(destination);
    }


    public void PrendrePc()
    {
        statut = RobStatut.Recup;

        Debug.Log(
            "Robot " + idr +
            " récupère : " +
            pieceCourante.Nom
        );

        /*
         * V1 :
         * récupération logique de la pièce.
         *
         * Plus tard, nous ajouterons ici
         * la représentation graphique
         * de la pièce transportée.
         */
    }


    public void DeposerPc()
    {
        Debug.Log(
            "Robot " + idr +
            " dépose : " +
            pieceCourante.Nom
        );

        /*
         * V1 :
         * dépôt logique de la pièce.
         *
         * Plus tard, nous pourrons déplacer
         * graphiquement l'objet dans
         * la zone de dépôt.
         */
    }
}
