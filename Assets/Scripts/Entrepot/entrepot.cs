using UnityEngine;

public class Entrepot : MonoBehaviour
{
    [Header("Robot")]
    [SerializeField] private RobotController robot;

    [Header("Zones principales")]
    [SerializeField] private Zone zoneDepart;
    [SerializeField] private Zone zoneDepot;

    [Header("Rayonnages")]
    [SerializeField] private Transform rayonnages;

    [Header("Test déplacement simple")]
    [SerializeField] private bool testerDeplacementSimple;
    [SerializeField] private Emplacement emplacementTest;

    [Header("Test V1")]
    [SerializeField] private bool lancerTestAuDemarrage;
    [SerializeField] private int idCommandeTest = 1;
    [SerializeField] private Piece pieceTest;

    public Zone ZoneDepart => zoneDepart;
    public Zone ZoneDepot => zoneDepot;

    private void Reset()
    {
        TrouverStructure();
    }

    private void OnValidate()
    {
        TrouverStructure();
    }

    public void ConfigurerStructure(Zone nouvelleZoneDepart, Zone nouvelleZoneDepot, Transform nouveauxRayonnages)
    {
        zoneDepart = nouvelleZoneDepart;
        zoneDepot = nouvelleZoneDepot;
        rayonnages = nouveauxRayonnages;
    }

    public void TrouverStructure()
    {
        GameObject racine = GameObject.Find("Entrepot");
        if (racine == null)
            return;

        if (rayonnages == null)
            rayonnages = racine.transform.Find("Rayonnages");

        if (zoneDepart == null)
        {
            Transform t = racine.transform.Find("Zone/ZoneDepart");
            if (t != null) zoneDepart = t.GetComponent<Zone>();
        }

        if (zoneDepot == null)
        {
            Transform t = racine.transform.Find("Zone/ZoneDepot");
            if (t != null) zoneDepot = t.GetComponent<Zone>();
        }
    }

    public bool TryGetPositionEmplacement(int allee, char section, int niveau, out Vector3 position)
    {
        position = Vector3.zero;
        TrouverStructure();

        if (rayonnages == null)
            return false;

        if (allee < 1 || allee > ReglesEntrepot.NombreAllees ||
            section < 'A' || section > 'G' ||
            niveau < 1 || niveau > ReglesEntrepot.NombreNiveaux)
            return false;

        Transform rayonnage = rayonnages.Find($"A{allee}");
        if (rayonnage == null)
            return false;

        Transform plateau = rayonnage.Find($"Plateau_{niveau - 1}");
        if (plateau == null)
            return false;

        Renderer[] renderers = rayonnage.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return false;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        int indexSection = section - 'A';
        float longueurSection = bounds.size.z / ReglesEntrepot.NombreSections;
        float z = bounds.min.z + longueurSection * (indexSection + 0.5f);

        position = new Vector3(bounds.center.x, plateau.position.y, z);
        return true;
    }

    private void Awake()
    {
        TrouverStructure();
        if (robot != null)
            robot.Initialiser(this);
    }

    private void Start()
    {
        if (testerDeplacementSimple)
        {
            if (robot == null || emplacementTest == null)
                return;

            robot.AllerVers(emplacementTest.Position);
            return;
        }

        if (!lancerTestAuDemarrage)
            return;

        if (robot == null || zoneDepart == null || zoneDepot == null ||
            pieceTest == null || pieceTest.EStockage == null)
            return;

        Commande commande = new Commande(idCommandeTest);
        commande.AjouterPiece(pieceTest);
        robot.DemarrerCommande(commande);
    }
}
