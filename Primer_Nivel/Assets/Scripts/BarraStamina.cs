using UnityEngine;
using UnityEngine.UI;

public class BarraStamina : MonoBehaviour
{
    public Slider slider;
    public Image fill;

    public Color originalColor = Color.green;
    public Color lowColor = Color.red;

    public float flashSpeed = 4f;
    public float flashThreshold = 25f; // 25%

    private bool flashing = false;
    private float flashTimer = 0f;

    private bool reachedZero = false; // Recuerda si ha llegado a 0

    public void SetMaxStamina(float max)
    {
        slider.maxValue = max;
        slider.value = max;

        fill.color = originalColor;
        flashing = false;
        reachedZero = false;
    }

    public void SetStamina(float stamina)
    {
        slider.value = stamina;

        // Si alguna vez llega a 0, se activa la condición 2
        if (stamina <= 0f)
        {
            reachedZero = true;
        }

        // Condición 1: Por debajo del 25%
        bool lowStamina = stamina <= flashThreshold;

        // Condición 2: Ha llegado a 0 y aún no ha vuelto a 100%
        bool recoveringFromZero = reachedZero && stamina < slider.maxValue;

        // Activar parpadeo si cualquiera de los dos es true (OR)
        bool shouldFlash = lowStamina || recoveringFromZero;

        if (shouldFlash && !flashing)
        {
            flashing = true;
            flashTimer = 0f;
        }

        // Detener parpadeo cuando ya NO se cumple ninguna condición
        if (!shouldFlash && flashing)
        {
            flashing = false;
            reachedZero = false;
            fill.color = originalColor;
        }
    }

    void Update()
    {
        if (flashing)
        {
            flashTimer += Time.deltaTime * flashSpeed;
            float t = Mathf.PingPong(flashTimer, 1f);
            fill.color = Color.Lerp(originalColor, lowColor, t);
        }
    }
}
