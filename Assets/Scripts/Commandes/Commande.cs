using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum StatutCommande
{
    EnAttente,
    EnCours,
    Finie
}

/// <summary>
/// Données d'une commande.
/// Ce n'est volontairement pas un MonoBehaviour : une commande est une donnée métier
/// créée et manipulée par le runtime, pas un composant à attacher à un GameObject.
/// </summary>
[Serializable]
public class Commande
{
    [SerializeField]
    [FormerlySerializedAs("idc")]
    [FormerlySerializedAs("idCommande")]
    private int idCommande;

    [SerializeField]
    private StatutCommande statut = StatutCommande.EnAttente;

    [SerializeField]
    [FormerlySerializedAs("pcs")]
    [FormerlySerializedAs("pieces")]
    private List<Piece> pieces = new List<Piece>();

    public int IdCommande
    {
        get => idCommande;
        set => idCommande = value;
    }

    public StatutCommande Statut => statut;

    /// <summary>
    /// Liste des pièces de la commande.
    /// La liste est toujours non nulle.
    /// </summary>
    public List<Piece> Pieces
    {
        get
        {
            AssurerListePieces();
            return pieces;
        }
    }

    public int NombrePieces
    {
        get
        {
            AssurerListePieces();
            return pieces.Count;
        }
    }

    public bool EstVide => NombrePieces == 0;

    // Alias temporaires pour ne pas casser immédiatement l'ancien code de Léo.
    public int Idc
    {
        get => IdCommande;
        set => IdCommande = value;
    }

    public List<Piece> Pcs => Pieces;

    public Commande()
    {
        statut = StatutCommande.EnAttente;
        pieces = new List<Piece>();
    }

    public Commande(int idCommande)
    {
        this.idCommande = idCommande;
        statut = StatutCommande.EnAttente;
        pieces = new List<Piece>();
    }

    public void AjouterPiece(Piece piece)
    {
        AssurerListePieces();

        if (piece == null || pieces.Contains(piece))
            return;

        pieces.Add(piece);
    }

    public void RetirerPiece(Piece piece)
    {
        AssurerListePieces();

        if (piece == null)
            return;

        pieces.Remove(piece);
    }

    /// <summary>
    /// Retourne la première pièce à traiter.
    /// Pour la V1 du projet, une commande contient une seule pièce.
    /// </summary>
    public Piece ObtenirPremierePiece()
    {
        AssurerListePieces();

        if (pieces.Count == 0)
            return null;

        return pieces[0];
    }

    /// <summary>
    /// Conservé pour compatibilité avec RobotController actuel.
    /// </summary>
    public Piece Traitement()
    {
        return ObtenirPremierePiece();
    }

    public float CalculerPoidsTotal()
    {
        AssurerListePieces();

        float poidsTotal = 0f;

        foreach (Piece piece in pieces)
        {
            if (piece != null)
                poidsTotal += piece.Poids;
        }

        return poidsTotal;
    }

    public void Demarrer()
    {
        if (statut != StatutCommande.EnAttente)
            return;

        statut = StatutCommande.EnCours;
        Debug.Log($"Commande {idCommande} démarrée.");
    }

    public void Finaliser()
    {
        statut = StatutCommande.Finie;
        Debug.Log($"Commande {idCommande} finalisée.");
    }

    private void AssurerListePieces()
    {
        if (pieces == null)
            pieces = new List<Piece>();
    }
}
