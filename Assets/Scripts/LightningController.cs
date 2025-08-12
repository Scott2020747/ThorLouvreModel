using UnityEngine;
using System.Collections;

public class LightningController : MonoBehaviour
{
    public Light lightningLight;
    public float flashInterval = 5f;  // seconds between flashes
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > flashInterval)
        {
            StartCoroutine(FlashLightning());
            timer = 0f;
        }
    }

    IEnumerator FlashLightning()
    {
        lightningLight.intensity = 8f;
        yield return new WaitForSeconds(0.1f);
        lightningLight.intensity = 0f;
    }
}
