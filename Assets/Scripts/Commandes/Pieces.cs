using System;
using UnityEngine;

[Serializable]
public class Piece
{
    [SerializeField]
    private int idPc;

    [SerializeField]
    private string nom;

    [SerializeField]
    private float poids;

    [SerializeField]
    private Emplacement eStockage;


    public int IdPc
    {
        get { return idPc; }
        set { idPc = value; }
    }

    public string Nom
    {
        get { return nom; }
        set { nom = value; }
    }

    public float Poids
    {
        get { return poids; }
        set { poids = value; }
    }

    public Emplacement EStockage
    {
        get { return eStockage; }
        set { eStockage = value; }
    }


    public Piece()
    {
    }


    public Piece(
        int idPc,
        string nom,
        float poids,
        Emplacement eStockage)
    {
        this.idPc = idPc;
        this.nom = nom;
        this.poids = poids;
        this.eStockage = eStockage;
    }
}