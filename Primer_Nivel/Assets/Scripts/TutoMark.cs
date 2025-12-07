using UnityEngine;

public class TutorialMarker : MonoBehaviour
{
    [Header("UI y Referencias")]
    public GameObject infoPanel; // Panel que contiene el texto de la explicación
    public GameObject markerIcon; // El ícono de exclamación visible

    [Header("Configuración")]
    public string playerTag = "Player"; // Asegúrate de que tu jugador tenga este tag

    private bool playerIsNearby = false;

    void Start()
    {
        // Asegúrate de que el panel de información esté oculto al inicio
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        // Opcional: Para darle un efecto flotante a la exclamación.
        if (markerIcon != null)
        {
            // Ajusta el movimiento para que parezca que está flotando
            markerIcon.GetComponent<Animator>()?.Play("Flotar");
        }
    }

    void Update()
    {
        // Si el panel está activo y el jugador se aleja, lo desactiva
        if (infoPanel.activeSelf && !playerIsNearby)
        {
            infoPanel.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Detección: Solo responde al objeto con el tag "Player"
        if (other.CompareTag(playerTag))
        {
            playerIsNearby = true;

            // 2. Activación: Muestra el panel de información
            if (infoPanel != null)
            {
                infoPanel.SetActive(true);
            }

            // 3. Desactiva el ícono de exclamación flotante para que no estorbe
            if (markerIcon != null)
            {
                markerIcon.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 1. Detección: Solo responde al objeto con el tag "Player"
        if (other.CompareTag(playerTag))
        {
            playerIsNearby = false;

            // 2. Desactivación: Oculta el panel
            if (infoPanel != null)
            {
                infoPanel.SetActive(false);
            }

            // 3. Vuelve a mostrar el ícono flotante
            if (markerIcon != null)
            {
                markerIcon.SetActive(true);
            }
        }
    }
}