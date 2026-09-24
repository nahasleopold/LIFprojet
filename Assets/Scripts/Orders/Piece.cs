using UnityEngine;

namespace Warehouse
{
    public class Piece : MonoBehaviour
    {
        [Header("Attributs de la Pièce")]
        [SerializeField] private int id_pc;
        [SerializeField] private string nom;
        [SerializeField] private float poids;
        [SerializeField] private Emplacement e_stockage; 

        // --- GETTERS / SETTERS ---
        public int Id_pc
        {
            get => id_pc;
            set => id_pc = value;
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

        public Emplacement E_stockage
        {
            get => e_stockage;
            set => e_stockage = value;
        }
    }

        public string GetAdresseStockage() //retourne l'adresse de l'emplacement du stockage et "non stocké" si aucun
        {
            if (e_stockage != null)
            {
                return e_stockage.GetLogicalAddress();
            }
            return "Non stocké";
        }

        public void RangerDans(Emplacement nouvelEmplacement) //Assigne un nouvel emplacement de stockage à la pièce
        {
            e_stockage = nouvelEmplacement;
            if (e_stockage != null)
            {
                Debug.Log($"La pièce '{nom}' a été rangée à l'emplacement {e_stockage.GetLogicalAddress()}.");
            }
        }


        public void RetirerDuStock() //Retire la pièce de son emplacement actuel
        {
            if (e_stockage != null)
            {
                Debug.Log($"La pièce '{nom}' a été retirée de l'emplacement {e_stockage.GetLogicalAddress()}.");
                e_stockage = null;
            }
        }
}