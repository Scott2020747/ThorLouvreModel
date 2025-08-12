using UnityEngine;

public class AddCollidersTool : MonoBehaviour
{
    [ContextMenu("🧱 Add Mesh Colliders to All Children")]
    void AddMeshColliders()
    {
        int count = 0;

        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            // Only add if child has a mesh and no existing collider
            if (child.GetComponent<MeshFilter>() && child.GetComponent<MeshRenderer>() && !child.GetComponent<Collider>())
            {
                MeshCollider mc = child.gameObject.AddComponent<MeshCollider>();

                // Optional: enable convex (useful for particle/rigidbody interaction)
                mc.convex = true; // 🔁 Change to false if you want it only for static blocking

                count++;
            }
        }

        Debug.Log($"✅ Added MeshColliders to {count} children under {gameObject.name}");
    }
}
