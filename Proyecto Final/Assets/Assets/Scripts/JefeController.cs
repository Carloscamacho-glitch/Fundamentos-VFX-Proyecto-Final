using UnityEngine;

public class JefeController : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHealth = 3;                     // Cantidad máxima de vida
    [SerializeField] private int currentHealth;                     // Vida actual
    [SerializeField] private bool canTakeDamageFromTop = true;      // puede recibir daño
    [SerializeField] private bool playerOnTop = false;              // si el jugador está arriba
    [SerializeField] private float damageCooldown = 3f;             // tiempo total de cooldown
    [SerializeField] private float cooldownTimer = 0f;              // temporizador restante
    [SerializeField] private bool cooldownActive = false;           // si el cooldown está corriendo

    [Header("Movimiento Circular")]
    [SerializeField] private float circleRadius = 5f;               // Radio del círculo
    [SerializeField] private float circleSpeed = 60f;               // Grados por segundo
    [SerializeField] private float pivotHeightOffset = 0f;          // Offset vertical del punto pivote
    [SerializeField] private float rotationSpeed = 5f;              // Velocidad de alineación con el suelo
    [SerializeField] private int circleDirection = 1;               // 1 o -1 para dirección del círculo
    [SerializeField] private Vector3 pivotPoint;                    // Punto alrededor del cual gira
    [SerializeField] private float angle = 0f;                      // Ángulo actual en el círculo
    private bool isCircling = false;                                // Si se está en moviendo en círculo
    private float circlingTimer = 0f;                               // Temporizador para contar tiempo en movimiento

    [Header("Cambio de dirección aleatorio")]
    [SerializeField] private float minChangeInterval = 2f;          // Intervalo mínimo para cambiar dirección
    [SerializeField] private float maxChangeInterval = 5f;          // Intervalo máximo para cambiar dirección
    private float changeTimer;                                      // Temporizador para el cambio de dirección
    private float nextChangeTime;                                   // Tiempo hasta el próximo cambio

    [Header("Detección Jugador")]
    [SerializeField] private float detectionRange = 10f;            // Rango de detección del jugador

    [Header("Brazos y Cohetes")]
    [SerializeField] private Transform brazoIzquierdo;              // Transform del brazo izquierdo
    [SerializeField] private Transform brazoDerecho;                // Transform del brazo derecho
    [SerializeField] private float brazoOffset = 2f;                // Offset para abrir los brazos
    private bool brazosMovidos = false;                             // Si los brazos ya están abiertos
    [SerializeField] private Transform CoheteIzquierdo;             // Transform del Cohete izquierdo
    [SerializeField] private Transform CoheteDerecho;               // Transform del Cohete derecho
    [SerializeField] private float CohetesOffset = 0.7f;            // Offset para abrir las Cohetes
    [SerializeField] private float CohetesDescenso = 0.8f;          // Descenso al abrir las Cohetes
    private bool brazosDie = false;                                 // Si los brazos ya están en posición de muerte

    [Header("Disparo")]
    [SerializeField] private float fireRate = 1f;                   // balas por segundo
    private float fireCooldown = 0f;                                // Tiempo restante para el próximo disparo
    private bool canShoot = false;                                  // Si puede disparar

    [Header("Referencias")]
    private Rigidbody rb;                                           // Referencia al Rigidbody
    private Transform player;                                       // Referencia al jugador
    [SerializeField] private GameObject bulletPrefab;               // Prefab de la bala
    [SerializeField] private Transform[] bulletSpawnPoints;         // Puntos de spawn de las balas
    [SerializeField] private GameObject smoke;                      // Prefab de la explosión al recibir daño
    [SerializeField] private GameObject[] coheteParticles;          // Partículas de los cohetes
    [SerializeField] private BossHealthBar bossHealthBar;           // Referencia a la barra de vida del jefe
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        currentHealth = maxHealth;

        Vector3 planetUp = (transform.position - Planeta.planeta.transform.position).normalized;
        Vector3 forward = Vector3.ProjectOnPlane(-transform.forward, planetUp).normalized;

        pivotPoint = transform.position + forward * circleRadius + planetUp * pivotHeightOffset;

        circleDirection = Random.value > 0.5f ? 1 : -1;

        ResetChangeTimer();
        DesactivarCohetes();
    }
    
    private void Update()
    {
        if (bossHealthBar != null)
            bossHealthBar.SetHealth(currentHealth, maxHealth);
    }

    private void FixedUpdate()
    {
        AlignToPlanetSurface();

        // Control del cooldown al recibir daño
        if (cooldownActive && !playerOnTop)
        {
            cooldownTimer -= Time.fixedDeltaTime;
            if (cooldownTimer <= 0f)
            {
                cooldownActive = false;
                canTakeDamageFromTop = true;
            }
        }

        // Si está en cooldown, el jefe no hace nada más
        if (cooldownActive)
            return;


        if (player != null && currentHealth > 0)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= detectionRange)
            {
                if (bossHealthBar != null)
                    bossHealthBar.SetVisible(true);

                rb.constraints = RigidbodyConstraints.None;
                Atack();
            }
            else
            {
                ResetToInitialState();
                if (bossHealthBar != null)
                    bossHealthBar.SetVisible(false);
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
        }
        else
        {
            if (!brazosDie)
            {
                GirarYMoverBrazos();
                DesactivarCohetes();
                brazosDie = true;
            }
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        if (fireCooldown > 0f)
            fireCooldown -= Time.fixedDeltaTime;
    }

    private void Atack()
    {
        if (!brazosMovidos)
        {
            AbrirBrazosYCohetes();
            ActivarCohetes();
            brazosMovidos = true;
        }

        CircleMovement();
        HandleDirectionChange();

        if (!isCircling)
        {
            circlingTimer = 0f;
            isCircling = true;
        }
        else
        {
            circlingTimer += Time.fixedDeltaTime;
            if (circlingTimer >= 2f)
                canShoot = true;
        }

        if (canShoot)
            Shoot();
    }

    // Alinea el jefe al planeta
    private void AlignToPlanetSurface()
    {
        Vector3 planetUp = (transform.position - Planeta.planeta.transform.position).normalized;
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, planetUp) * transform.rotation;
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
    }

    // Movimiento circular alrededor del punto pivote
    private void CircleMovement()
    {
        Vector3 planetUp = (transform.position - Planeta.planeta.transform.position).normalized;
        Vector3 forward = Vector3.Cross(Vector3.right, planetUp).normalized;
        if (forward == Vector3.zero)
            forward = Vector3.Cross(Vector3.forward, planetUp).normalized;

        Vector3 right = Vector3.Cross(planetUp, forward).normalized;

        angle += circleSpeed * circleDirection * Time.fixedDeltaTime;

        Vector3 offset = (Mathf.Cos(angle * Mathf.Deg2Rad) * forward +
                          Mathf.Sin(angle * Mathf.Deg2Rad) * right) * circleRadius;

        Vector3 newPos = pivotPoint + offset;
        rb.MovePosition(newPos);

        Vector3 dirToPivot = (pivotPoint - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(-dirToPivot, planetUp);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, lookRot, rotationSpeed * Time.fixedDeltaTime));
    }

    private void HandleDirectionChange()
    {
        changeTimer += Time.fixedDeltaTime;
        if (changeTimer >= nextChangeTime)
        {
            circleDirection *= -1; // Cambia la dirección de movimiento
            ResetChangeTimer();
        }
    }

    private void ResetChangeTimer()
    {
        changeTimer = 0f;
        nextChangeTime = Random.Range(minChangeInterval, maxChangeInterval);
    }

    private void AbrirBrazosYCohetes()
    {
        if (brazoIzquierdo != null)
            brazoIzquierdo.localPosition += Vector3.left * brazoOffset;
        if (brazoDerecho != null)
            brazoDerecho.localPosition += Vector3.right * brazoOffset;

        if (CoheteIzquierdo != null)
            CoheteIzquierdo.localPosition += Vector3.left * CohetesOffset + Vector3.down * CohetesDescenso;
        if (CoheteDerecho != null)
            CoheteDerecho.localPosition += Vector3.right * CohetesOffset + Vector3.down * CohetesDescenso;
    }

    private void CerrarBrazosYCohetes()
    {
        if (brazoIzquierdo != null)
            brazoIzquierdo.localPosition -= Vector3.left * brazoOffset;
        if (brazoDerecho != null)
            brazoDerecho.localPosition -= Vector3.right * brazoOffset;

        if (CoheteIzquierdo != null)
            CoheteIzquierdo.localPosition -= Vector3.left * CohetesOffset + Vector3.down * CohetesDescenso;
        if (CoheteDerecho != null)
            CoheteDerecho.localPosition -= Vector3.right * CohetesOffset + Vector3.down * CohetesDescenso;
    }

    private void Shoot()
    {
        if (bulletPrefab == null || bulletSpawnPoints.Length == 0) return;

        if (fireCooldown <= 0f)
        {
            Transform spawnPoint = bulletSpawnPoints[Random.Range(0, bulletSpawnPoints.Length)];

            Vector3 targetPosition = player.position;
            GameObject missile = Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity);

            MisilController mc = missile.GetComponent<MisilController>();
            if (mc != null)
            {
                mc.SetTarget(targetPosition); // Asignar el objetivo al misil (Ultima posicion del jugador)
            }

            fireCooldown = 1f / fireRate;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            playerOnTop = true;

            Vector3 contactNormal = collision.contacts[0].normal;
            Vector3 planetUp = (transform.position - Planeta.planeta.transform.position).normalized;

            if (canTakeDamageFromTop && Vector3.Dot(contactNormal, -planetUp) > 0.5f && currentHealth > 0)
            {
                TakeDamage(1);
                canTakeDamageFromTop = false;

                // Iniciamos cooldown (pausado si el jugador sigue arriba)
                cooldownActive = true;
                cooldownTimer = damageCooldown;

                // Instanciar explosión y hacerla hija del jefe
                if (smoke != null)
                {
                    Vector3 spawnPos = transform.position + Vector3.up * 4f;
                    GameObject smokeI = Instantiate(smoke, spawnPos, Quaternion.identity);
                    smokeI.transform.SetParent(transform);
                }

                // Reiniciar al estado inicial
                ResetToInitialState();
            }
            else if (canTakeDamageFromTop && currentHealth == 0)
            {
                Transform jefeC = transform.Find("JefeC");
                if (jefeC != null)
                {
                    Transform centro = jefeC.Find("Centro");
                    if (centro != null)
                        Destroy(centro.gameObject);

                    // Accedemos al componente del jugador que gestiona mel motor
                    PlayerController player = collision.gameObject.GetComponent<PlayerController>();
                    if (player != null)
                    {
                        player.AddMotor(true);
                    }
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            playerOnTop = false; // El jugador ya no está encima
        }
    }

    private void TakeDamage(int amount)
    {
        currentHealth -= amount;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void ResetToInitialState()
    {
        if (brazosMovidos)
        {
            CerrarBrazosYCohetes();
            DesactivarCohetes();
            brazosMovidos = false;
        }
        isCircling = false;
        canShoot = false;
        circlingTimer = 0f;
        fireCooldown = 0f;
    }

    private void Die()
    {
        ResetToInitialState();
        if (bossHealthBar != null)
            bossHealthBar.SetVisible(false);
    }

    private void GirarYMoverBrazos()
    {
        // Brazo Izquierdo
        brazoIzquierdo.localRotation *= Quaternion.Euler(0, 0, 90);
        brazoIzquierdo.localPosition -= Vector3.down * 1.8f;
        brazoIzquierdo.localPosition += Vector3.left * 1.8f;

        // Brazo Derecho
        brazoDerecho.localRotation *= Quaternion.Euler(0, 0, -90);
        brazoDerecho.localPosition -= Vector3.down * 1.8f;
        brazoDerecho.localPosition += Vector3.left * -1.8f;

        // Cohete Izquierdo
        CoheteIzquierdo.localRotation *= Quaternion.Euler(0, 0, 90);
        CoheteIzquierdo.localPosition -= Vector3.down * 1.8f;
        CoheteIzquierdo.localPosition += Vector3.left * 1.8f;

        // Cohete Derecho
        CoheteDerecho.localRotation *= Quaternion.Euler(0, 0, -90);
        CoheteDerecho.localPosition -= Vector3.down * 1.8f;
        CoheteDerecho.localPosition += Vector3.left * -1.8f;
    }
    
    private void ActivarCohetes()
    {
        foreach (GameObject psObj in coheteParticles)
        {
            if (psObj != null)
                psObj.SetActive(true);
        }
    }

    private void DesactivarCohetes()
    {
        foreach (GameObject psObj in coheteParticles)
        {
            if (psObj != null)
                psObj.SetActive(false);
        }
    }
}
