using UnityEngine;

public class InteriorKitTestDoor : MonoBehaviour
{
    public Transform leaf;
    private bool open;
    public void Toggle()
    {
        open = !open;
        if (leaf) leaf.localRotation = Quaternion.Euler(0, open ? -95 : 0, 0);
        Physics.SyncTransforms();
    }
}
