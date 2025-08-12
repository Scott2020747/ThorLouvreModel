using UnityEngine;

public class RainLooper : MonoBehaviour
{
    public GameObject rainEffect;     // Assign your particle system here
    public float rainDuration = 30f;  // Rain active time
    public float pauseDuration = 5f;  // Rain pause time

    private float timer = 0f;
    private bool isRaining = true;

    void Start()
    {
        if (rainEffect != null)
        {
            rainEffect.SetActive(true); // Start with rain
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (isRaining && timer >= rainDuration)
        {
            StopRain();
        }
        else if (!isRaining && timer >= pauseDuration)
        {
            StartRain();
        }
    }

    void StartRain()
    {
        if (rainEffect != null)
        {
            rainEffect.SetActive(true);
            isRaining = true;
            timer = 0f;
        }
    }

    void StopRain()
    {
        if (rainEffect != null)
        {
            rainEffect.SetActive(false);
            isRaining = false;
            timer = 0f;
        }
    }
}
