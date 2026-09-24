using System;
using System.Collections.Generic;

namespace Warehouse
{
    /// <summary>
    /// Modèle géométrique et logique de l'entrepôt : 16 allées (A1 à A16), regroupées en
    /// 9 modules physiques (A1 seule, A2/A3, A4/A5, ..., A14/A15, A16 seule), reliés par
    /// 8 couloirs à sens unique alternés (montant / descendant).
    ///
    /// Règles issues de précisions_entrepot.txt :
    /// - Zones An (n impair) : accessibles uniquement par la droite (entre An et An+1).
    /// - Zones Am (m pair) : accessibles uniquement par la gauche (entre Am-1 et Am).
    /// - Couloirs An/An+1 avec n dans {1,5,9,13} : sens montant uniquement.
    /// - Couloirs An/An+1 avec n dans {3,7,11,15} : sens descendant uniquement.
    ///
    /// Cette classe ne dépend pas d'UnityEngine (hors positionnement) afin de rester
    /// testable indépendamment de la scène.
    /// </summary>
    public static class WarehouseLayout
    {
        public const int AisleCount = 16;

        public enum CorridorDirection
        {
            Montant,
            Descendant
        }

        /// <summary>A1 et A16 sont des allées simples ; toutes les autres sont doublées (A2/A3, A4/A5, ...).</summary>
        public static bool IsSingleAisle(int aisleNumber) => aisleNumber == 1 || aisleNumber == AisleCount;

        /// <summary>Zones impaires : accessibles uniquement par la droite (entre An et An+1).</summary>
        public static bool IsAccessibleFromRight(int aisleNumber) => aisleNumber % 2 == 1;

        /// <summary>Zones paires : accessibles uniquement par la gauche (entre An-1 et An).</summary>
        public static bool IsAccessibleFromLeft(int aisleNumber) => aisleNumber % 2 == 0;

        /// <summary>Un couloir existe entre An et An+1 uniquement pour n impair (8 couloirs au total).</summary>
        public static bool IsCorridorIndex(int n) => n % 2 == 1 && n >= 1 && n <= AisleCount - 1;

        /// <summary>Sens de circulation imposé du couloir entre An et An+1.</summary>
        public static CorridorDirection GetCorridorDirection(int n)
        {
            if (!IsCorridorIndex(n))
                throw new ArgumentException(
                    $"{n} n'est pas un index de couloir valide (doit être impair, entre 1 et {AisleCount - 1}).");

            // n dans {1,5,9,13} -> montant (s'éloigne de la zone de départ/dépôt)
            // n dans {3,7,11,15} -> descendant (se rapproche de la zone de départ/dépôt)
            return (n % 4 == 1) ? CorridorDirection.Montant : CorridorDirection.Descendant;
        }

        /// <summary>Regroupe les 16 allées en 9 modules physiques (blocs de rayonnage adjacents).</summary>
        public static List<int[]> GetModules()
        {
            var modules = new List<int[]> { new[] { 1 } };
            for (int n = 2; n <= AisleCount - 2; n += 2)
                modules.Add(new[] { n, n + 1 });
            modules.Add(new[] { AisleCount });
            return modules;
        }

        /// <summary>
        /// Parse une adresse de pièce au format "A[n]-[Z|A]-[m]" (ex: "A6-Z-4") en ses composantes.
        /// n dans [1,16], section 'A' (bas) ou 'Z' (haut), m dans [1,10].
        /// </summary>
        public static (int aisle, char section, int level) ParseAddress(string address)
        {
            // Format attendu : "A{n}-{A|Z}-{m}", ex. "A6-Z-4", "A16-A-1"
            var parts = address.Split('-');
            if (parts.Length != 3 || parts[0].Length < 2 || parts[0][0] != 'A')
                throw new FormatException($"Adresse invalide : '{address}'. Format attendu 'A[n]-[Z|A]-[m]'.");

            int aisle = int.Parse(parts[0].Substring(1));
            char section = parts[1][0];
            int level = int.Parse(parts[2]);

            if (aisle < 1 || aisle > AisleCount)
                throw new ArgumentOutOfRangeException(nameof(address), $"Allée hors bornes : A{aisle}.");
            if (section != 'A' && section != 'Z')
                throw new ArgumentOutOfRangeException(nameof(address), $"Section inconnue : {section} (attendu 'A' ou 'Z').");
            if (level < 1 || level > 10)
                throw new ArgumentOutOfRangeException(nameof(address), $"Étage hors bornes : {level}.");

            return (aisle, section, level);
        }
    }
}
