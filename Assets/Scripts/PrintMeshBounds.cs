using UnityEngine;

public class PrintMeshBounds : MonoBehaviour
{
    void Start()
    {
        var renderers = GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length == 0)
        {
            Debug.LogWarning("❌ No MeshRenderers found under " + gameObject.name);
            return;
        }

        Bounds combinedBounds = renderers[0].bounds;
        foreach (var rend in renderers)
        {
            combinedBounds.Encapsulate(rend.bounds);
        }

        Debug.Log($"✅ Louvre bounds in world units: {combinedBounds.size}");
    }
}
