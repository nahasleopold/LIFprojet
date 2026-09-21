using UnityEngine;

public class Emplacement : MonoBehaviour
{
    [Header("Adresse")]
    [SerializeField] private int allee;
    [SerializeField] private string section = "A";
    [SerializeField] private int niveau = 1;

    [Header("Placement")]
    [SerializeField] private float margeAuDessusPlateau = 0.05f;
    [SerializeField] private float distanceRobotRayonnage = 1f;

    [SerializeField, HideInInspector]
    private Vector3 positionAccesRobot;

    public int Allee => allee;
    public string Section => section;
    public int Niveau => niveau;

    /// <summary>Position où le robot doit s'arrêter.</summary>
    public Vector3 Position => positionAccesRobot;

    /// <summary>Alias explicite de Position.</summary>
    public Vector3 PositionAccesRobot => positionAccesRobot;

    /// <summary>Position physique de stockage sur le rayonnage.</summary>
    public Vector3 PositionStockage => transform.position;

    private void Reset()
    {
        LireNom();
    }

    private void OnValidate()
    {
        LireNom();
    }

    public void Initialiser(int numeroAllee, string nouvelleSection, int nouveauNiveau)
    {
        allee = numeroAllee;
        section = nouvelleSection.ToUpperInvariant();
        niveau = nouveauNiveau;
        gameObject.name = ObtenirAdresseLogique();
    }

    public string ObtenirAdresseLogique()
    {
        return $"A{allee}-{section}-{niveau}";
    }

    public bool Valider()
    {
        return allee >= 1 && allee <= ReglesEntrepot.NombreAllees &&
               section != null && section.Length == 1 &&
               section[0] >= 'A' && section[0] <= 'G' &&
               niveau >= 1 && niveau <= ReglesEntrepot.NombreNiveaux;
    }

    [ContextMenu("Placer cet emplacement")]
    public void PlacerEmplacement()
    {
        if (!LireNom() || !Valider())
            return;

        GameObject racine = GameObject.Find("Entrepot");
        if (racine == null)
        {
            Debug.LogError("GameObject 'Entrepot' introuvable.", this);
            return;
        }

        Transform rayonnage = racine.transform.Find($"Rayonnages/A{allee}");
        if (rayonnage == null)
        {
            Debug.LogError($"Rayonnage A{allee} introuvable.", this);
            return;
        }

        Transform plateau = rayonnage.Find($"Plateau_{niveau - 1}");
        if (plateau == null)
        {
            Debug.LogError($"Plateau_{niveau - 1} introuvable dans A{allee}.", this);
            return;
        }

        Renderer[] renderers = rayonnage.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogError($"Aucun Renderer trouvé dans A{allee}.", this);
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        int indexSection = section[0] - 'A';
        float longueurSection = bounds.size.z / ReglesEntrepot.NombreSections;
        float z = bounds.min.z + longueurSection * (indexSection + 0.5f);
        float x = bounds.center.x;

        Renderer rendererPlateau = plateau.GetComponent<Renderer>();
        float y = rendererPlateau != null
            ? rendererPlateau.bounds.max.y + margeAuDessusPlateau
            : plateau.position.y + margeAuDessusPlateau;

        transform.position = new Vector3(x, y, z);

        float xRobot = ReglesEntrepot.EstAccessibleParGauche(allee)
            ? bounds.min.x - distanceRobotRayonnage
            : bounds.max.x + distanceRobotRayonnage;

        positionAccesRobot = new Vector3(xRobot, 0f, z);
    }

    private bool LireNom()
    {
        try
        {
            var adresse = ReglesEntrepot.AnalyserAdresse(gameObject.name);
            allee = adresse.allee;
            section = adresse.section.ToString();
            niveau = adresse.niveau;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, 0.25f);
        Gizmos.DrawWireSphere(positionAccesRobot, 0.35f);
        Gizmos.DrawLine(transform.position, positionAccesRobot);
    }
}
