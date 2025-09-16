using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Rigidbody rb;          // referencia al rigidbody

    [Header("Destruccion")]
    [SerializeField] private float fallDelay;   //Tiempo de espera para que caiga
    private float destroyDelay = 3f;    //Tiempo de espera para destruir la plataforma

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Para que al inicio no caiga
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Invoke("DropPlatform", fallDelay);
        }
    }

    void DropPlatform()
    {
        rb.isKinematic = false; // Dejar que la gravedad la tire
        Destroy(gameObject, destroyDelay);
    }
}
