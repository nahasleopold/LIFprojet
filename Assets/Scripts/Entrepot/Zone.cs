using UnityEngine;

public enum TypeZone
{
    Depart,
    Couloir,
    Depot,
    Couloir_Retour
}

public class Zone : MonoBehaviour
{
    [SerializeField] private int idz;
    [SerializeField] private TypeZone type;
    [SerializeField] private bool estOccupee;
    [SerializeField] private Transform pointEntree;
    [SerializeField] private Transform pointSortie;

    public int Idz { get => idz; set => idz = value; }
    public TypeZone Type { get => type; set => type = value; }
    public bool EstOccupee { get => estOccupee; set => estOccupee = value; }

    public Vector3 Entree => pointEntree != null ? pointEntree.position : transform.position;
    public Vector3 Sortie => pointSortie != null ? pointSortie.position : transform.position;

    public void Configurer(int identifiant, TypeZone typeZone, Transform entree, Transform sortie)
    {
        idz = identifiant;
        type = typeZone;
        pointEntree = entree;
        pointSortie = sortie;
    }

    public bool EstLibre() => !estOccupee;
    public void Occuper() => estOccupee = true;
    public void Liberer() => estOccupee = false;
}
