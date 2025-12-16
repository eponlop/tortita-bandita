using UnityEngine;
// No necesitamos 'UnityEngine.SceneManagement' ya que no vamos a cargar una escena.
// using UnityEngine.SceneManagement; 

public class Siguiente_Nivel_2 : MonoBehaviour
{
    // 1. Variable pública para asignar el Canvas en el Inspector
    // Asegúrate de que este Canvas está inicialmente desactivado en la jerarquía de Unity.
    public GameObject canvasAMostrar;

    private void OnTriggerEnter(Collider other)
    {
        // **VERIFICACIÓN CLAVE:** // Comprueba si el objeto que entró en el trigger (other) tiene el tag "Player".
        if (other.CompareTag("Player"))
        {
            // 2. Comprobación de seguridad: 
            // Si el Canvas a mostrar no es nulo, actívalo.
            if (canvasAMostrar != null)
            {
                // Muestra el Canvas estableciendo su estado activo a 'true'.
                canvasAMostrar.SetActive(true);
            }
            else
            {
                Debug.LogError("¡ERROR! El Canvas a mostrar no está asignado en el Inspector de Unity en el objeto: " + gameObject.name);
            }

            // Opcional: Desactiva el Collider o este script si solo quieres que se active una vez.
            // gameObject.SetActive(false); 
        }
    }
}
