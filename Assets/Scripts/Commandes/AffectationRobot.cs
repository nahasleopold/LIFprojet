using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Représente le résultat du calcul d'affectation pour un robot.
/// Cette classe ne déplace pas le robot : elle indique simplement
/// quelles pièces lui sont attribuées et quelle capacité est utilisée.
/// </summary>
[Serializable]
public class AffectationRobot
{
    [SerializeField] private RobotController robot;
    [SerializeField] private List<Piece> pieces = new List<Piece>();
    [SerializeField] private float poidsTotal;

    public RobotController Robot => robot;
    public List<Piece> Pieces => pieces;
    public float PoidsTotal => poidsTotal;
    public int NombrePieces => pieces != null ? pieces.Count : 0;

    public float CapaciteRestante
    {
        get
        {
            if (robot == null)
                return 0f;

            return Mathf.Max(0f, robot.Capacite - poidsTotal);
        }
    }

    public AffectationRobot(RobotController robot)
    {
        this.robot = robot;
        pieces = new List<Piece>();
        poidsTotal = 0f;
    }

    public bool PeutAjouter(Piece piece)
    {
        if (robot == null || piece == null)
            return false;

        if (piece.Poids < 0f)
            return false;

        return poidsTotal + piece.Poids <= robot.Capacite;
    }

    public bool AjouterPiece(Piece piece)
    {
        if (piece == null || !PeutAjouter(piece))
            return false;

        if (pieces == null)
            pieces = new List<Piece>();

        if (pieces.Contains(piece))
            return false;

        pieces.Add(piece);
        poidsTotal += piece.Poids;
        return true;
    }

    public void Vider()
    {
        if (pieces == null)
            pieces = new List<Piece>();
        else
            pieces.Clear();

        poidsTotal = 0f;
    }
}
