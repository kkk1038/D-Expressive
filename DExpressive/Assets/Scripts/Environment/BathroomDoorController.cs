using UnityEngine;

/// <summary>A local bathroom door, independent of story choices and the entrance ending.</summary>
public class BathroomDoorController : MonoBehaviour
{
    [SerializeField] private Transform hinge;
    [SerializeField] private BoxCollider leafCollider;
    [SerializeField] private float openAngle = 95f;
    [SerializeField] private float degreesPerSecond = 150f;
    private float angle;
    private bool wantsOpen;
    private CharacterController player;

    public bool IsOpen => Mathf.Abs(angle - openAngle) < 0.1f;
    public bool IsClosed => angle < 0.1f;
    public string Prompt => wantsOpen ? "화장실 문 닫기" : "화장실 문 열기";

    public void Initialize(Transform doorHinge, BoxCollider doorCollider)
    {
        hinge = doorHinge;
        leafCollider = doorCollider;
        angle = 0;
        wantsOpen = false;
    }

    public void Toggle() => wantsOpen = !wantsOpen;

    private void Update()
    {
        if (!hinge || !leafCollider || Mathf.Approximately(Time.timeScale, 0)) return;
        float next = Mathf.MoveTowards(angle, wantsOpen ? openAngle : 0, degreesPerSecond * Time.deltaTime);
        if (Mathf.Approximately(next, angle)) return;
        if (!player) player = FindFirstObjectByType<CharacterController>();
        hinge.localRotation = Quaternion.Euler(0, next, 0);
        Physics.SyncTransforms();
        if (player && player.enabled && Physics.ComputePenetration(
                leafCollider, leafCollider.transform.position, leafCollider.transform.rotation,
                player, player.transform.position, player.transform.rotation, out _, out float depth) && depth > 0.003f)
        {
            hinge.localRotation = Quaternion.Euler(0, angle, 0);
            Physics.SyncTransforms();
            return;
        }
        angle = next;
    }
}
