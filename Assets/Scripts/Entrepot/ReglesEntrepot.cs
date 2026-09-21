using System;
using System.Collections.Generic;

/// <summary>
/// Règles logiques communes de l'entrepôt.
/// Ne dépend pas de la scène Unity.
/// </summary>
public static class ReglesEntrepot
{
    public const int NombreAllees = 16;
    public const int NombreSections = 7; // A à G
    public const int NombreNiveaux = 4;

    public enum SensCouloir
    {
        Montant,
        Descendant
    }

    public static bool EstAlleeSimple(int numeroAllee)
    {
        return numeroAllee == 1 || numeroAllee == NombreAllees;
    }

    public static bool EstAccessibleParDroite(int numeroAllee)
    {
        return numeroAllee % 2 == 1;
    }

    public static bool EstAccessibleParGauche(int numeroAllee)
    {
        return numeroAllee % 2 == 0;
    }

    public static bool EstIndexCouloir(int numeroAllee)
    {
        return numeroAllee % 2 == 1 &&
               numeroAllee >= 1 &&
               numeroAllee <= NombreAllees - 1;
    }

    public static SensCouloir ObtenirSensCouloir(int numeroAllee)
    {
        if (!EstIndexCouloir(numeroAllee))
            throw new ArgumentException(
                $"{numeroAllee} n'est pas un index de couloir valide.");

        return numeroAllee % 4 == 1
            ? SensCouloir.Montant
            : SensCouloir.Descendant;
    }

    public static List<int[]> ObtenirModules()
    {
        var modules = new List<int[]> { new[] { 1 } };

        for (int n = 2; n <= NombreAllees - 2; n += 2)
            modules.Add(new[] { n, n + 1 });

        modules.Add(new[] { NombreAllees });
        return modules;
    }

    public static (int allee, char section, int niveau) AnalyserAdresse(string adresse)
    {
        if (string.IsNullOrWhiteSpace(adresse))
            throw new FormatException("L'adresse est vide.");

        string[] parties = adresse.Split('-');

        if (parties.Length != 3 ||
            parties[0].Length < 2 ||
            parties[0][0] != 'A')
        {
            throw new FormatException(
                $"Adresse invalide : '{adresse}'. Format attendu : A6-C-3.");
        }

        if (!int.TryParse(parties[0].Substring(1), out int allee))
            throw new FormatException($"Allée invalide dans '{adresse}'.");

        string texteSection = parties[1].ToUpperInvariant();
        if (texteSection.Length != 1)
            throw new FormatException($"Section invalide dans '{adresse}'.");

        char section = texteSection[0];

        if (!int.TryParse(parties[2], out int niveau))
            throw new FormatException($"Niveau invalide dans '{adresse}'.");

        if (allee < 1 || allee > NombreAllees)
            throw new ArgumentOutOfRangeException(nameof(adresse), $"Allée hors bornes : A{allee}.");

        if (section < 'A' || section > 'G')
            throw new ArgumentOutOfRangeException(nameof(adresse), $"Section hors bornes : {section}. Attendu A à G.");

        if (niveau < 1 || niveau > NombreNiveaux)
            throw new ArgumentOutOfRangeException(nameof(adresse), $"Niveau hors bornes : {niveau}. Attendu 1 à {NombreNiveaux}.");

        return (allee, section, niveau);
    }
}
