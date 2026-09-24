using UnityEngine;

namespace Warehouse
{
    /// <summary>
    /// Repère (sans géométrie visible) placé dans un couloir généré par
    /// WarehouseEnvironmentGenerator, indiquant le sens de circulation autorisé.
    /// À utiliser plus tard par la couche de navigation (ex: filtrage des couloirs
    /// autorisés selon l'état du robot, ou ancrage d'un Off-Mesh Link à sens unique).
    /// </summary>
    public class CorridorMarker : MonoBehaviour
    {
        public WarehouseLayout.CorridorDirection direction;

        [Tooltip("Numéro d'allée n tel que le couloir relie An et An+1.")]
        public int corridorIndex;

        private void OnDrawGizmos()
        {
            Gizmos.color = direction == WarehouseLayout.CorridorDirection.Montant ? Color.cyan : Color.magenta;
            Vector3 arrowDir = direction == WarehouseLayout.CorridorDirection.Montant ? Vector3.forward : Vector3.back;
            Gizmos.DrawLine(transform.position, transform.position + arrowDir * 2f);
            Gizmos.DrawSphere(transform.position + arrowDir * 2f, 0.3f);
        }
    }
}
