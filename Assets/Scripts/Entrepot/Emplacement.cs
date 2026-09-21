using UnityEngine;

public enum SectionEntrepot
{
    A,
    B,
    C,
    D,
    E,
    F,
    G
}

public class Emplacement : MonoBehaviour
{
    [Header("Adresse")]
    [SerializeField] private int allee = 1;
    [SerializeField] private SectionEntrepot section = SectionEntrepot.A;
    [SerializeField] private int niveau = 1;

    [Header("Navigation du robot")]
    [SerializeField, HideInInspector]
    private Vector3 positionAccesRobot;

    public int Allee => allee;
    public SectionEntrepot Section => section;
    public int Niveau => niveau;

    // Position physique de la pièce sur le rayonnage.
    public Vector3 PositionStockage => transform.position;

    // Position au sol où le robot doit s'arrêter.
    public Vector3 PositionAccesRobot => positionAccesRobot;

    // Compatibilité avec le RobotController actuel de Léo.
    // Il peut continuer à utiliser pieceCourante.EStockage.Position.
    public Vector3 Position => positionAccesRobot;

    public string ObtenirAdresseLogique()
    {
        return $"A{allee}-{section}-{niveau}";
    }

    public void Initialiser(
        int numeroAllee,
        SectionEntrepot nouvelleSection,
        int nouveauNiveau,
        Vector3 nouvellePositionAccesRobot)
    {
        allee = numeroAllee;
        section = nouvelleSection;
        niveau = nouveauNiveau;
        positionAccesRobot = nouvellePositionAccesRobot;

        gameObject.name = ObtenirAdresseLogique();

        Valider(true);
    }

    public void DefinirPositionAccesRobot(Vector3 nouvellePosition)
    {
        positionAccesRobot = nouvellePosition;
    }

    public bool Valider(bool afficherErreur = true)
    {
        bool valide = allee >= 1
            && allee <= ReglesEntrepot.NombreAllees
            && niveau >= 1
            && niveau <= ReglesEntrepot.NombreNiveaux;

        if (!valide && afficherErreur)
        {
            Debug.LogError(
                $"Emplacement invalide : {ObtenirAdresseLogique()}",
                gameObject);
        }

        return valide;
    }

    private void OnValidate()
    {
        // OnValidate sert uniquement à valider les données.
        // La création et le placement des objets restent dans les outils Editor.
        Valider(false);
    }

    private void OnDrawGizmosSelected()
    {
        // Emplacement de stockage.
        Gizmos.DrawWireSphere(transform.position, 0.20f);

        // Point d'arrêt du robot.
        Gizmos.DrawWireSphere(positionAccesRobot, 0.30f);
        Gizmos.DrawLine(transform.position, positionAccesRobot);
    }
}
