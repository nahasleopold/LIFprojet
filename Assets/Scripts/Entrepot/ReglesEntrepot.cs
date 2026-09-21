using System;
using System.Collections.Generic;

public static class ReglesEntrepot
{
    public const int NombreAllees = 16;
    public const int NombreSections = 7;
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
        return numeroAllee % 2 == 1
            && numeroAllee >= 1
            && numeroAllee <= NombreAllees - 1;
    }

    public static SensCouloir ObtenirSensCouloir(int numeroAllee)
    {
        if (!EstIndexCouloir(numeroAllee))
        {
            throw new ArgumentException(
                $"{numeroAllee} n'est pas un index de couloir valide.");
        }

        return numeroAllee % 4 == 1
            ? SensCouloir.Montant
            : SensCouloir.Descendant;
    }

    public static List<int[]> ObtenirModules()
    {
        var modules = new List<int[]> { new[] { 1 } };

        for (int numero = 2; numero <= NombreAllees - 2; numero += 2)
        {
            modules.Add(new[] { numero, numero + 1 });
        }

        modules.Add(new[] { NombreAllees });
        return modules;
    }

    public static (int allee, char section, int niveau) AnalyserAdresse(string adresse)
    {
        if (string.IsNullOrWhiteSpace(adresse))
        {
            throw new FormatException("L'adresse est vide.");
        }

        string[] parties = adresse.Split('-');

        if (parties.Length != 3
            || parties[0].Length < 2
            || parties[0][0] != 'A')
        {
            throw new FormatException(
                $"Adresse invalide : '{adresse}'. Format attendu : A6-C-2.");
        }

        if (!int.TryParse(parties[0].Substring(1), out int allee))
        {
            throw new FormatException($"Allée invalide dans l'adresse '{adresse}'.");
        }

        if (parties[1].Length != 1)
        {
            throw new FormatException($"Section invalide dans l'adresse '{adresse}'.");
        }

        char section = char.ToUpperInvariant(parties[1][0]);

        if (!int.TryParse(parties[2], out int niveau))
        {
            throw new FormatException($"Niveau invalide dans l'adresse '{adresse}'.");
        }

        if (allee < 1 || allee > NombreAllees)
        {
            throw new ArgumentOutOfRangeException(nameof(adresse), $"Allée hors limites : A{allee}.");
        }

        if (section < 'A' || section > 'G')
        {
            throw new ArgumentOutOfRangeException(nameof(adresse), $"Section hors limites : {section}.");
        }

        if (niveau < 1 || niveau > NombreNiveaux)
        {
            throw new ArgumentOutOfRangeException(nameof(adresse), $"Niveau hors limites : {niveau}.");
        }

        return (allee, section, niveau);
    }
}
