using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] private float speed = 10f;                         // Velocidad de la bala
    [SerializeField] private float baseHeight = 1f;                     // Altura base sobre el suelo
    [SerializeField] private float maxRaycastDistance = 10f;            // Distancia máxima del raycast hacia el suelo
    [SerializeField] private float lifeTime = 5f;                       // Tiempo total de la bala
    [SerializeField] private float minHeight = 0.1f;                    // Altura mínima al final del lifetime
    private Rigidbody rb;                                               // Referencia al Rigidbody
    private Vector3 lastMoveDirection = Vector3.forward;                // Última dirección de movimiento
    private Quaternion initialRotation;                                 // Rotación inicial de la bala
    private float elapsedTime = 0f;                                     // tiempo desde que se creó la bala

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        initialRotation = transform.rotation;
        Destroy(gameObject, lifeTime); // destrucción automática tras el lifetime
    }

    private void FixedUpdate()
    {
        elapsedTime += Time.fixedDeltaTime;

        // Movimiento hacia adelante
        Vector3 move = transform.up * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
        lastMoveDirection = move.normalized;

        MaintainHeight();
        RotateToDirection();
    }

    private void MaintainHeight()
    {
        Vector3 planetUp = (transform.position - Planeta.planeta.transform.position).normalized;

        if (Physics.Raycast(transform.position, -planetUp, out RaycastHit hit, maxRaycastDistance))
        {
            // Calculamos la altura progresiva según el tiempo
            float t = Mathf.Clamp01(elapsedTime / lifeTime);
            float currentDesiredHeight = Mathf.Lerp(baseHeight, minHeight, t);

            float currentHeight = hit.distance;
            Vector3 pos = rb.position;
            pos += planetUp * (currentDesiredHeight - currentHeight);
            rb.MovePosition(pos);
        }
    }

    private void RotateToDirection()
    {
        if (lastMoveDirection != Vector3.zero)
        {
            Vector3 planetUp = (transform.position - Planeta.planeta.transform.position).normalized;
            Vector3 moveDirOnPlane = Vector3.ProjectOnPlane(lastMoveDirection, planetUp).normalized;

            if (moveDirOnPlane != Vector3.zero)
            {
                Quaternion baseRotation = Quaternion.LookRotation(moveDirOnPlane, planetUp);
                Quaternion finalRotation = baseRotation * Quaternion.Euler(90f, 0f, 0f);
                rb.MoveRotation(finalRotation);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null) player.TakeDamage(1);

            Destroy(gameObject);
        }

        if (!collision.collider.CompareTag("Turret"))
        {
            Destroy(gameObject);
        }
    }
}
