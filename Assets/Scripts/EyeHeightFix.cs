using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class EyeHeightFix : MonoBehaviour
{
    public Transform xrRigRoot;             // XR Origin (XR Rig)
    public Transform xrCamera;              // Main Camera inside XR Rig
    public float targetEyeHeight = 1.7f;    // Desired eye level height in meters
    public float triggerHeight = 3.0f;      // If height is above this, apply fix

    void Start()
    {
        StartCoroutine(AdjustEyeLevel());
    }

    System.Collections.IEnumerator AdjustEyeLevel()
    {
        // Wait a short moment until XR is fully initialized
        yield return new WaitForSeconds(1f);

        float currentEyeY = xrCamera.position.y;

        if (currentEyeY > triggerHeight)
        {
            float offset = currentEyeY - targetEyeHeight;
            Vector3 rigPos = xrRigRoot.position;
            xrRigRoot.position = new Vector3(rigPos.x, rigPos.y - offset, rigPos.z);
            Debug.Log($"[EyeHeightFix] XR Rig moved down by {offset:F2}m to match eye level.");
        }
        else
        {
            Debug.Log($"[EyeHeightFix] XR Rig height OK: {currentEyeY:F2}m.");
        }
    }
}
