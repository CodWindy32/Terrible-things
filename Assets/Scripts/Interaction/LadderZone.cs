using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LadderZone : MonoBehaviour
{
    [Header("Ladder Geometry")]
    [SerializeField] private Transform ladderBottom;

    [Tooltip("Top of the climbable area.")]
    [SerializeField] private Transform ladderTop;

    [Tooltip("Position where the player is placed after reaching the top.")]
    [SerializeField] private Transform dismountPoint;

    [Header("Settings")]
    [Tooltip("How far from the ladder surface the player stands while climbing.")]
    [SerializeField] private float playerStandDistance = 0.45f;

    public Transform LadderBottom => ladderBottom;
    public Transform LadderTop => ladderTop;
    public Transform DismountPoint => dismountPoint;
    public float PlayerStandDistance => playerStandDistance;

    public Vector3 OutwardDirection => ladderBottom.forward;

    public Vector3 GetClimbPosition(float y)
    {
        Vector3 basePos = ladderBottom.position;
        Vector3 offset = OutwardDirection * playerStandDistance;
        return new Vector3(basePos.x + offset.x, y, basePos.z + offset.z);
    }

    private void OnDrawGizmosSelected()
    {
        if (ladderBottom == null || ladderTop == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(ladderBottom.position, ladderTop.position);
        Gizmos.DrawWireSphere(ladderBottom.position, 0.15f);
        Gizmos.DrawWireSphere(ladderTop.position, 0.15f);

        Gizmos.color = Color.cyan;
        Vector3 outward = OutwardDirection * playerStandDistance;
        Gizmos.DrawLine(ladderBottom.position, ladderBottom.position + outward);
        Gizmos.DrawLine(ladderTop.position, ladderTop.position + outward);

        if (dismountPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(dismountPoint.position, 0.2f);
        }
    }
}
