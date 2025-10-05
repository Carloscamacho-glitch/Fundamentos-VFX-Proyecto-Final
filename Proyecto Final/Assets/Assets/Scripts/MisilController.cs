using UnityEngine;

public class MisilController : MonoBehaviour
{
    [SerializeField] private float speed = 10f;                 // Velocidad del misil
    [SerializeField] private float stopDistance = 0.1f;         // Distancia mínima para destruirlo
    private Rigidbody rb;                                       // Referencia al Rigidbody
    private Vector3 targetPosition;                             // Posición del objetivo
    private bool targetSet = false;                             // Si el objetivo ha sido asignado
    [SerializeField] private GameObject explosion;              // Prefab de la explosión

    public void SetTarget(Vector3 target)
    {
        targetPosition = target;    // Asignar el objetivo
        targetSet = true;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!targetSet) return;

        // Dirección hacia el objetivo
        Vector3 moveDir = (targetPosition - transform.position).normalized;

        // Movimiento
        Vector3 newPos = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        // Rotación para que el misil apunte a la dirección de movimiento
        if (moveDir != Vector3.zero)
        {
            Vector3 planetUp = (transform.position - Planeta.planeta.transform.position).normalized;
            Vector3 moveDirOnPlane = Vector3.ProjectOnPlane(moveDir, planetUp).normalized;
            if (moveDirOnPlane != Vector3.zero)
            {
                Quaternion lookRot = Quaternion.LookRotation(moveDirOnPlane, planetUp);
                rb.MoveRotation(lookRot);
            }
        }

        // Destruir al llegar al objetivo
        if (Vector3.Distance(transform.position, targetPosition) <= stopDistance)
        {
            Destroy(gameObject);
            Instantiate(explosion, transform.position, Quaternion.identity);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null) player.TakeDamage(1);

            Destroy(gameObject);
            Instantiate(explosion, transform.position, Quaternion.identity);
        }

        // Destruye el misil si golpea cualquier cosa que no sea la torreta
        if (!collision.collider.CompareTag("Turret"))
        {
            Destroy(gameObject);
            Instantiate(explosion, transform.position, Quaternion.identity);
        }
    }
}
