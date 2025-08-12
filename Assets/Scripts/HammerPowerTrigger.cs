using UnityEngine;

public class HammerPowerTrigger : MonoBehaviour
{
    public GameObject rainEffect;
    public LightningController lightning;

    private bool activated = false;

    void Update()
    {
        // Watch hammer rotation
        if (!activated && transform.localRotation.eulerAngles.x > 60)
        {
            activated = true;
            rainEffect.SetActive(true);
            lightning.enabled = true;
        }
    }
}
