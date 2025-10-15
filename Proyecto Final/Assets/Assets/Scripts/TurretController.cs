using UnityEngine;

public class TurretController : MonoBehaviour
{
    [Header("Rotación y Detección")]
    [SerializeField] private float rotationSpeed = 5f;          // Velocidad de alineación con el suelo
    [SerializeField] private float detectionRange = 15f;        // Rango de detección del jugador
    private bool playerInRange = false;                         // Si el jugador está dentro del rango de detección

    [Header("Disparo")]
    [SerializeField] private float fireRate = 1f;               // Balas por segundo
    private float fireCooldown = 0f;                            // Tiempo restante para el próximo disparo
    private bool waitingForFirstShot = false;                   // Si está esperando para disparar la primera vez

    [Header("Referencias")]
    private Rigidbody rb;                                       // Referencia al Rigidbody
    private Transform player;                                   // Referencia al jugador
    [SerializeField] private GameObject bulletPrefab;           // Prefab de la bala
    [SerializeField] private GameObject explosion;              // Prefab de la explosión
    [SerializeField] private GameObject chatarra;              // Prefab de la chatarra

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void FixedUpdate()
    {
        AlignToPlanetSurface();

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= detectionRange)
            {
                if (!playerInRange)
                {
                    // El jugador acaba de entrar en rango
                    playerInRange = true;
                    waitingForFirstShot = true;
                    fireCooldown = 0f;
                }

                RotateTowardsPlayer();
                TryShoot();
            }
            else
            {
                playerInRange = false; // jugador salió del rango
            }
        }

        // Reducir cooldown del disparo
        if (fireCooldown > 0f)
            fireCooldown -= Time.fixedDeltaTime;
    }

    // Alinea la torreta al planeta, en caso de que no se coloque bien
    private void AlignToPlanetSurface()
    {
        Vector3 planetUp = (transform.position - Planeta.planeta.transform.position).normalized;
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, planetUp) * currentRotation;
        rb.MoveRotation(Quaternion.Slerp(currentRotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
    }

    private void RotateTowardsPlayer()
    {
        Vector3 planetUp = (transform.position - Planeta.planeta.transform.position).normalized;
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        //Proyectamos sobre el plano tangente para no inclinar el cañón (hacia arriba/abajo)
        Vector3 flatDirection = Vector3.ProjectOnPlane(directionToPlayer, planetUp).normalized;

        if (flatDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatDirection, planetUp);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
    }

    private void TryShoot()
    {
        if (bulletPrefab == null) return;

        if (waitingForFirstShot)
        {
            // Dispara solo si ya está alineada al jugador
            if (IsAlignedWithPlayer())
            {
                Shoot();
                waitingForFirstShot = false; // ya disparó la primer bala
                fireCooldown = 1f / fireRate;
            }
        }
        else
        {
            if (fireCooldown <= 0f)
            {
                Shoot();
                fireCooldown = 1f / fireRate;
            }
        }
    }

    private void Shoot()
    {
        if (bulletPrefab != null)
        {
            // Offset de spawn: 1 arriba, delante 1 (para que salga del cañon)
            Vector3 spawnPos = transform.position + transform.forward * 1f + transform.up * 1f;

            // Rotación: hacia adelante + offset inicial 90° en X
            Quaternion spawnRot = transform.rotation * Quaternion.Euler(90f, 0f, 0f);

            // Instanciamos la bala
            Instantiate(bulletPrefab, spawnPos, spawnRot);
        }
    }

    //verificar si está alineada con el jugador (dentro de un pequeño margen)
    private bool IsAlignedWithPlayer()
    {
        if (player == null) return false;

        Vector3 planetUp = (transform.position - Planeta.planeta.transform.position).normalized;
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Vector3 flatDirection = Vector3.ProjectOnPlane(directionToPlayer, planetUp).normalized;

        float angle = Vector3.Angle(transform.forward, flatDirection);
        return angle < 5f; // tolerancia de 5 grados
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        Vector3 planetUp = (transform.position - Planeta.planeta.transform.position).normalized;
        PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();

        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 contactNormal = contact.normal;

            // Solo se destruye si el jugador cae sobre la torreta
            if (Vector3.Dot(contactNormal, -planetUp) > 0.5f)
            {
                // Jugador lo pisa desde arriba
                Destroy(gameObject);
                Vector3 spawnPos = transform.position + Vector3.up * 1f;
                Instantiate(explosion, spawnPos, Quaternion.identity);
                spawnPos = transform.position + transform.up * 1f;
                Instantiate(chatarra, spawnPos, transform.rotation * Quaternion.Euler(0, 0, 180));
                return;
            }
            else
            {
                // Colisión lateral: restar vida al jugador pero no destruir torreta
                if (playerController != null)
                    playerController.TakeDamage(1);
            }
        }
    }
}
