using System;
using System.Collections.Generic;

public enum CmdStatut
{
    En_attente,
    En_cours,
    Finie
}


[Serializable]
public class Commande
{
    private int idc;
    private CmdStatut statut;
    private List<Piece> pcs;


    public int Idc
    {
        get { return idc; }
        set { idc = value; }
    }

    public CmdStatut Statut
    {
        get { return statut; }
    }

    public List<Piece> Pcs
    {
        get { return pcs; }
    }


    public Commande()
    {
        statut = CmdStatut.En_attente;
        pcs = new List<Piece>();
    }


    public Commande(int idc)
    {
        this.idc = idc;

        statut = CmdStatut.En_attente;

        pcs = new List<Piece>();
    }


    public void AjouterPiece(Piece piece)
    {
        pcs.Add(piece);
    }


    public Piece Traitement()
    {
        if (pcs.Count == 0)
            return null;

        // V1 : la commande contient une seule pièce.
        return pcs[0];
    }


    public void Demarrer()
    {
        statut = CmdStatut.En_cours;
    }


    public void Finaliser()
    {
        statut = CmdStatut.Finie;
    }
}