using UnityEngine;

public class Chatarra : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f;

    private void Update()
    {
        // Rotar continuamente alrededor del eje Y local
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        // Cantidad aleatoria entre 1 y 10
        int cantidadChatarra = Random.Range(1, 11);

        // Accedemos al componente del jugador que gestiona la chatarra
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.AddChatarra(cantidadChatarra);
        }

        // Destruimos el objeto tras recogerlo
        Destroy(gameObject);
    }
}
