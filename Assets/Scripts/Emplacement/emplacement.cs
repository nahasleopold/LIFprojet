using UnityEngine;
using System;

namespace Warehouse
{
    /// <summary>
    /// Represents an emplacement in a warehouse.
    /// Combine une adresse logique (A[n]-[Z|A]-[m]) avec une position 3D.
    /// </summary>
    
    public enum Section
    {
        /// <summary>
        /// Une allee est divisée en plusieurs sections (de A à G)
        /// </summary>
        A, B, C, D, E, F, G
    }

    public class Emplacement : MonoBehaviour
    {
        /// <summary>
        /// Definition des variables.
        /// </summary>
        
        /// <summary>Numéro d'allée [1,16] (ex: 6 pour A6).</summary>
        [SerializeField]
        private int allee;

        /// <summary>Section de A à G</summary>
        [SerializeField]
        private Section section;

        /// <summary>Niveau [1,4].</summary>
        [SerializeField]
        private int niveau;

        /// <summary>
        /// Definition des fonctions
        /// </summary>

        /// <summary> GETTERS ET SETTERS </summary>
        public int Allee
        {
            get { return allee; }
            set { allee = value; }
        }

        public Section Section
        {
            get { return section; }
            set { section = value; }
        }

        public int Niveau
        {
            get { return niveau; }
            set { niveau = value; }
        }

        /// <summary>Retourne l'adresse logique de l'emplacement sous la forme A[n]-[Z|A]-[m].</summary>
        public string GetLogicalAddress()
        {
            char sectionChar = (char)('A' + (int)section); /// Convertit l'enum Section en caractère (A, B, C, D, E, F, G)
            return $"A{allee}-{sectionChar}-{niveau}";
        }

        /// <summary>Retourne l'emplacement 3d dans la scene </summary>
        public Vector3 PositionMonde => transform.position;
    
        /// <summary>Initialise un emplacement</summary> 
        public void Initialize(int alleeNum, Section sec, int niv)
        {
            allee = alleeNum;
            section = sec;
            niveau = niv;
            Validate();
        }

        public bool Validate()
        {
            bool valid = allee >= 1 && allee <= WarehouseLayout.AisleCount
                       && niveau >= 1 && niveau <= 4;
            if (!valid)
                Debug.LogError($"[Emplacement] Invalide : {GetLogicalAddress()}", gameObject);
            return valid;
        }

        public override string ToString() => GetLogicalAddress();
    }

}