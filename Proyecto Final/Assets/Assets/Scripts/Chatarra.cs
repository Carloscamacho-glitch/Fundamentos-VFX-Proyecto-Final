using UnityEngine;

public class Chatarra : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f; // velocidad de rotación
    [SerializeField] private float cooldownTiempo; // tiempo de espera antes de poder recoger
    private bool puedeRecogerse = false; // controla si puede ser recogida

    private void Start()
    {
        // Comienza el cooldown
        StartCoroutine(ActivarRecogidaDespuesDeTiempo());
    }

    private void Update()
    {
        // Rotar continuamente alrededor del eje Y local
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!puedeRecogerse) return;

        // Solo responde si el que colisiona es el jugador
        if (!collision.gameObject.CompareTag("Player")) return;

        // Cantidad aleatoria entre 1 y 10
        int cantidadChatarra = Random.Range(1, 11);


        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.AddChatarra(cantidadChatarra);
        }

        // Destruimos el objeto tras recogerlo
        Destroy(gameObject);
    }

    private System.Collections.IEnumerator ActivarRecogidaDespuesDeTiempo()
    {
        // Espera 1 segundos antes de permitir la recogida
        yield return new WaitForSeconds(cooldownTiempo);
        puedeRecogerse = true;
    }
}
