using UnityEngine;

public class JefeController : MonoBehaviour
{
    [Header("Movimiento Circular")]
    [SerializeField] private float circleRadius = 5f;
    [SerializeField] private float circleSpeed = 60f;
    [SerializeField] private float pivotHeightOffset = 0f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private int circleDirection = 1;
    [SerializeField] private Vector3 pivotPoint;
    [SerializeField] private float angle = 0f;

    [Header("Detección Jugador")]
    [SerializeField] private float detectionRange = 15f;

    [Header("Cambio de dirección aleatorio")]
    [SerializeField] private float minChangeInterval = 2f;
    [SerializeField] private float maxChangeInterval = 5f;
    private float changeTimer;
    private float nextChangeTime;

    [Header("Brazos")]
    [SerializeField] private Transform brazoIzquierdo;
    [SerializeField] private Transform brazoDerecho;
    [SerializeField] private float brazoOffset = 2f;
    private bool brazosMovidos = false;

    [Header("Ruedas")]
    [SerializeField] private Transform ruedaIzquierda;
    [SerializeField] private Transform ruedaDerecha;
    [SerializeField] private float ruedaOffset = 2f;
    [SerializeField] private float ruedaDescenso = 1f;

    [Header("Disparo")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 1f; // balas por segundo
    private float fireCooldown = 0f;
    private float circlingTimer = 0f;
    private bool canShoot = false;

    private Rigidbody rb;
    private Transform player;
    private bool isCircling = false;

    [SerializeField] private Transform[] bulletSpawnPoints;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        Vector3 planetUp = (transform.position - Planeta.planeta.transform.position).normalized;
        Vector3 forward = Vector3.ProjectOnPlane(-transform.forward, planetUp).normalized;

        pivotPoint = transform.position + forward * circleRadius + planetUp * pivotHeightOffset;

        circleDirection = Random.value > 0.5f ? 1 : -1;

        ResetChangeTimer();
    }

    private void FixedUpdate()
    {
        AlignToPlanetSurface();

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= detectionRange)
            {
                if (!brazosMovidos)
                {
                    MoverBrazosYRuedas();
                    brazosMovidos = true;
                }

                CircleMovement();
                HandleDirectionChange();

                // Contamos el tiempo que ha estado dando vueltas
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
                    TryShoot();
            }
            else
            {
                isCircling = false;
                canShoot = false;
                circlingTimer = 0f;
            }
        }

        // Reducimos cooldown de disparo
        if (fireCooldown > 0f)
            fireCooldown -= Time.fixedDeltaTime;
    }

    private void AlignToPlanetSurface()
    {
        Vector3 planetUp = (transform.position - Planeta.planeta.transform.position).normalized;
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, planetUp) * transform.rotation;
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
    }

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
            circleDirection *= -1;
            ResetChangeTimer();
        }
    }

    private void ResetChangeTimer()
    {
        changeTimer = 0f;
        nextChangeTime = Random.Range(minChangeInterval, maxChangeInterval);
    }

    private void MoverBrazosYRuedas()
    {
        if (brazoIzquierdo != null)
            brazoIzquierdo.localPosition += Vector3.left * brazoOffset;
        if (brazoDerecho != null)
            brazoDerecho.localPosition += Vector3.right * brazoOffset;

        if (ruedaIzquierda != null)
            ruedaIzquierda.localPosition += Vector3.left * ruedaOffset + Vector3.down * ruedaDescenso;
        if (ruedaDerecha != null)
            ruedaDerecha.localPosition += Vector3.right * ruedaOffset + Vector3.down * ruedaDescenso;
    }

    private void TryShoot()
    {
        if (bulletPrefab == null || bulletSpawnPoints.Length == 0) return;

        if (fireCooldown <= 0f)
        {
            Transform spawnPoint = bulletSpawnPoints[Random.Range(0, bulletSpawnPoints.Length)];

            // Guardamos la posición del jugador al momento de disparar
            Vector3 targetPosition = player.position;

            // Instanciamos el misil
            GameObject missile = Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity);

            // Le pasamos la posición objetivo al misil
            MisilController mc = missile.GetComponent<MisilController>();
            if (mc != null)
            {
                mc.SetTarget(targetPosition);
            }

            fireCooldown = 1f / fireRate;
        }
    }
}
