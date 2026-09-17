using UnityEngine;

public enum TypeZone
{
    Depart,
    Couloir,
    Depot,
    Couloir_Ret
}


public class Zone : MonoBehaviour
{
    [SerializeField]
    private int idz;

    [SerializeField]
    private TypeZone type;

    [SerializeField]
    private bool estOccupee = false;

    [SerializeField]
    private Transform pointEntree;

    [SerializeField]
    private Transform pointSortie;


    public int Idz
    {
        get { return idz; }
        set { idz = value; }
    }

    public TypeZone Type
    {
        get { return type; }
        set { type = value; }
    }

    public bool EstOccupee
    {
        get { return estOccupee; }
        set { estOccupee = value; }
    }

    public Vector3 Entree
    {
        get
        {
            if (pointEntree != null)
                return pointEntree.position;

            return transform.position;
        }
    }

    public Vector3 Sortie
    {
        get
        {
            if (pointSortie != null)
                return pointSortie.position;

            return transform.position;
        }
    }


    public bool EstLibre()
    {
        return !estOccupee;
    }


    public void Occuper()
    {
        estOccupee = true;
    }


    public void Liberer()
    {
        estOccupee = false;
    }
}