using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Génère des commandes de test à partir des emplacements déjà présents
/// dans la scène.
///
/// V2.1 : ce composant crée uniquement les données de la commande.
/// Il ne choisit pas de robot et ne lance aucun déplacement.
/// </summary>
public class GenerateurCommande : MonoBehaviour
{
    [Header("Source des emplacements")]
    [SerializeField] private Transform conteneurEmplacements;

    [Header("Génération")]
    [SerializeField, Min(1)] private int nombrePiecesParCommande = 6;
    [SerializeField, Min(0.01f)] private float poidsMinimum = 1f;
    [SerializeField, Min(0.01f)] private float poidsMaximum = 10f;

    [Header("Identifiants")]
    [SerializeField, Min(1)] private int prochainIdCommande = 1;
    [SerializeField, Min(1)] private int prochainIdPiece = 1;

    [Header("Résultat de test")]
    [SerializeField] private Commande derniereCommandeGeneree;

    public Commande DerniereCommandeGeneree => derniereCommandeGeneree;

    private void Reset()
    {
        TrouverConteneurEmplacements();
    }

    private void Awake()
    {
        TrouverConteneurEmplacements();
    }

    /// <summary>
    /// Génère une commande avec le nombre de pièces configuré dans l'Inspector.
    /// </summary>
    public Commande GenererCommande()
    {
        return GenererCommande(nombrePiecesParCommande);
    }

    /// <summary>
    /// Génère une commande contenant jusqu'à nombrePieces pièces.
    /// Chaque pièce reçoit un emplacement différent dans cette commande.
    /// </summary>
    public Commande GenererCommande(int nombrePieces)
    {
        if (nombrePieces <= 0)
        {
            Debug.LogWarning("Le nombre de pièces à générer doit être supérieur à 0.", this);
            return null;
        }

        List<Emplacement> emplacementsDisponibles = RecupererEmplacementsDisponibles();

        if (emplacementsDisponibles.Count == 0)
        {
            Debug.LogError(
                "Aucun emplacement n'a été trouvé dans Entrepot/Emplacements. " +
                "Générez d'abord les emplacements de l'entrepôt.",
                this);
            return null;
        }

        int nombreAProduire = Mathf.Min(nombrePieces, emplacementsDisponibles.Count);

        if (nombreAProduire < nombrePieces)
        {
            Debug.LogWarning(
                $"Seulement {nombreAProduire} emplacement(s) disponible(s) pour " +
                $"{nombrePieces} pièce(s) demandée(s).",
                this);
        }

        Commande commande = new Commande(prochainIdCommande++);

        for (int i = 0; i < nombreAProduire; i++)
        {
            int indexAleatoire = Random.Range(0, emplacementsDisponibles.Count);
            Emplacement emplacementChoisi = emplacementsDisponibles[indexAleatoire];

            // On retire l'emplacement de la liste temporaire afin d'éviter
            // deux pièces au même emplacement dans la même commande de test.
            emplacementsDisponibles.RemoveAt(indexAleatoire);

            int idPiece = prochainIdPiece++;
            float poids = Random.Range(poidsMinimum, poidsMaximum);

            Piece piece = new Piece(
                idPiece,
                $"Piece_{idPiece}",
                poids,
                emplacementChoisi);

            commande.AjouterPiece(piece);
        }

        derniereCommandeGeneree = commande;
        AfficherCommandeDansConsole(commande);

        return commande;
    }

    [ContextMenu("Générer une commande de test")]
    private void GenererCommandeDepuisInspector()
    {
        GenererCommande();
    }

    /// <summary>
    /// Retrouve automatiquement Entrepot/Emplacements si aucune référence
    /// n'a été renseignée manuellement.
    /// </summary>
    private void TrouverConteneurEmplacements()
    {
        if (conteneurEmplacements != null)
            return;

        GameObject racineEntrepot = GameObject.Find("Entrepot");
        if (racineEntrepot == null)
            return;

        conteneurEmplacements = racineEntrepot.transform.Find("Emplacements");
    }

    private List<Emplacement> RecupererEmplacementsDisponibles()
    {
        TrouverConteneurEmplacements();

        List<Emplacement> resultat = new List<Emplacement>();

        if (conteneurEmplacements == null)
            return resultat;

        Emplacement[] emplacements =
            conteneurEmplacements.GetComponentsInChildren<Emplacement>(true);

        foreach (Emplacement emplacement in emplacements)
        {
            if (emplacement != null && emplacement.Valider())
                resultat.Add(emplacement);
        }

        return resultat;
    }

    private void AfficherCommandeDansConsole(Commande commande)
    {
        if (commande == null)
            return;

        Debug.Log(
            $"Commande {commande.IdCommande} générée : " +
            $"{commande.NombrePieces} pièce(s), " +
            $"poids total = {commande.CalculerPoidsTotal():0.00} kg.",
            this);

        foreach (Piece piece in commande.Pieces)
        {
            if (piece == null)
                continue;

            Debug.Log(
                $"  - {piece.Nom} | {piece.Poids:0.00} kg | " +
                $"{piece.ObtenirAdresseStockage()}",
                this);
        }
    }

    private void OnValidate()
    {
        if (poidsMinimum < 0.01f)
            poidsMinimum = 0.01f;

        if (poidsMaximum < poidsMinimum)
            poidsMaximum = poidsMinimum;
    }
}
