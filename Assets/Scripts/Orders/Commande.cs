using System.Collections.Generic;
using UnityEngine;

namespace Warehouse
{
    
    public enum cmd_statut 
    { 
        En_attente, 
        En_cours, 
        Finie 
    }

    public class Commande : MonoBehaviour
    {
        [Header("Attributs de la Commande")]
        [SerializeField] private int idc;
        [SerializeField] private cmd_statut statut = cmd_statut.En_attente;
        [SerializeField] private List<Pièce> pcs = new List<Piece>();

        // --- GETTERS / SETTERS ---
        public int Idc
        {
            get => idc;
            set => idc = value;
        }

        public cmd_statut Statut
        {
            get => statut;
            set => statut = value;
        }

        public List<Piece> Pcs
        {
            get => pcs;
            set => pcs = value;
        }

        
        public void Demarrer()
        {
            if (statut == cmd_statut.En_attente)
            {
                statut = cmd_statut.En_cours;
                Debug.Log($"Commande {idc} démarrée.");
            }
        }

        public void Traitement()
        {
            Debug.Log($"Traitement de la commande {idc} en cours..."); //Commande en cours 
            
        }

       
        public void Finaliser()
        {
            statut = cmd_statut.Finie;
            Debug.Log($"Commande {idc} finalisée (Finie).");
        }
    

        public void AjouterPiece(Piece piece)
        {
            if (piece != null && !pcs.Contains(piece))
            {
                pcs.Add(piece);
                Debug.Log($"Pièce {piece.Nom} ajoutée à la commande {idc}.");
            }
        }



        public void RetirerPiece(Piece piece)
        {
            if (pcs.Contains(piece))
            {
                pcs.Remove(piece);
                Debug.Log($"Pièce {piece.Nom} retirée de la commande {idc}.");
            }
        }


        public float CalculerPoidsTotal() //Calcule le poids total de la commande en additionnant toutes ses pièces
        {
            float poidsTotal = 0f;
            foreach (var piece in pcs)
            {
                if (piece != null)
                {
                    poidsTotal += piece.Poids;
                }
            }
            return poidsTotal;
        }

     }
     }   //public void AfficherFeuilleDeRoute() 

