using UnityEngine;

public class AddCollidersToChildren : MonoBehaviour
{
    [ContextMenu("Add Mesh Colliders to Children")]
    void AddColliders()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<MeshFilter>() && !child.GetComponent<Collider>())
            {
                MeshCollider mc = child.gameObject.AddComponent<MeshCollider>();
                mc.convex = true;
                //mc.inflateMesh = true;
                mc.isTrigger = false;
            }
        }

        Debug.Log("Mesh Colliders added to all children!");
    }
}
