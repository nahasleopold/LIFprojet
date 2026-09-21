using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Données métier d'une pièce stockée dans l'entrepôt.
/// Une pièce référence un composant Emplacement de la scène, mais n'est pas elle-même
/// un MonoBehaviour.
/// </summary>
[Serializable]
public class Piece
{
    [SerializeField]
    [FormerlySerializedAs("idPc")]
    [FormerlySerializedAs("id_pc")]
    private int idPiece;

    [SerializeField]
    private string nom;

    [SerializeField]
    private float poids;

    [SerializeField]
    [FormerlySerializedAs("eStockage")]
    [FormerlySerializedAs("e_stockage")]
    private Emplacement emplacementStockage;

    public int IdPiece
    {
        get => idPiece;
        set => idPiece = value;
    }

    public string Nom
    {
        get => nom;
        set => nom = value;
    }

    public float Poids
    {
        get => poids;
        set => poids = value;
    }

    public Emplacement EmplacementStockage
    {
        get => emplacementStockage;
        set => emplacementStockage = value;
    }

    public bool EstStockee => emplacementStockage != null;

    // Alias temporaires pour ne pas casser le RobotController et Entrepot actuels.
    public int IdPc
    {
        get => IdPiece;
        set => IdPiece = value;
    }

    public Emplacement EStockage
    {
        get => EmplacementStockage;
        set => EmplacementStockage = value;
    }

    public Piece()
    {
    }

    public Piece(int idPiece, string nom, float poids, Emplacement emplacementStockage)
    {
        this.idPiece = idPiece;
        this.nom = nom;
        this.poids = poids;
        this.emplacementStockage = emplacementStockage;
    }

    public string ObtenirAdresseStockage()
    {
        return emplacementStockage != null
            ? emplacementStockage.ObtenirAdresseLogique()
            : "Non stockée";
    }

    public void RangerDans(Emplacement nouvelEmplacement)
    {
        emplacementStockage = nouvelEmplacement;

        if (emplacementStockage != null)
        {
            Debug.Log(
                $"La pièce '{nom}' a été rangée à l'emplacement " +
                $"{emplacementStockage.ObtenirAdresseLogique()}.");
        }
    }

    public void RetirerDuStock()
    {
        if (emplacementStockage == null)
            return;

        Debug.Log(
            $"La pièce '{nom}' a été retirée de l'emplacement " +
            $"{emplacementStockage.ObtenirAdresseLogique()}.");

        emplacementStockage = null;
    }
}
