using UnityEngine;

public class SiropeConseguido : MonoBehaviour
{
    private GameObject player;

    void Start()
    {
        player = GameObject.Find("Tortita_Bandita");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Si usas colliders con triggers (por ejemplo, una zona de detección)
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            Debug.Log("Robo conseguido, el primero de muchos");
        }
    }

}
