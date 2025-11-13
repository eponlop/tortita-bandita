using UnityEngine;

public class SiropeConseguido : MonoBehaviour
{
   
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($" Robo conseguido, primero de muchos.");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($" Robo conseguido, primero de muchos.");
    }
}
