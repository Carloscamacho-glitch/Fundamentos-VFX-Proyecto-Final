using UnityEngine;

public class DronEnemy : MonoBehaviour
{
    [Header("Patrullaje")]
    [SerializeField] private float patrolDistance;
    private Vector3 patrolStart;
    private Vector3 patrolEnd;
    private bool movingToEnd = true;

    [Header("Persecusion")]
    [SerializeField] private float chaseRange = 7.5f;
    [SerializeField] private float bobbingAmplitude = 0.5f; // Altura máxima del movimiento arriba-abajo
    [SerializeField] private float bobbingFrequency = 5f;   // Frecuencia del movimiento
    [SerializeField] private float baseHeight = 1f; // Altura base sobre el suelo
    private float bobbingTimer = 0f;
    private bool freezeHeight = false;
    private float frozenHeight = 0f;
    private bool canChase = true; // controla si puede perseguir al jugador

    [Header("Movimiento")]
    [SerializeField] private float speed;
    private Vector3 lastMoveDirection = Vector3.forward; // Dirección de movimiento actual
    private bool isGrounded;

    [Header("Referencias")]
    private Rigidbody rb;
    private Transform player;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        patrolStart = transform.position - transform.forward * patrolDistance * 0.5f;
        patrolEnd = transform.position + transform.forward * patrolDistance * 0.5f;
        baseHeight = 1f; // Puedes ajustar esto si tu altura base cambia
    }

    private void Update()
    {
        bobbingTimer += Time.deltaTime;

        if (lastMoveDirection != Vector3.zero)
        {
            // Calcula "arriba" relativo al planeta
            Vector3 planetUp = (transform.position - Planeta.planeta.transform.position).normalized;

            // Proyecta la dirección de movimiento sobre el plano tangente al planeta
            Vector3 moveDirOnPlane = Vector3.ProjectOnPlane(-lastMoveDirection, planetUp).normalized;

            if (moveDirOnPlane != Vector3.zero)
            {
                // Rotación mirando en moveDirOnPlane con "arriba" hacia afuera del planeta
                Quaternion targetRotation = Quaternion.LookRotation(moveDirOnPlane, planetUp);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 0.2f));
            }
        }
    }

    private void FixedUpdate()
    {
        if (player != null && canChase && Vector3.Distance(rb.position, player.position) < chaseRange)
        {
            lastMoveDirection = ChasePlayer();
        }
        else
        {
            lastMoveDirection = Patrol();
        }

        MaintainHeight();
    }

    // Devuelve la dirección de movimiento usada
    private Vector3 Patrol()
    {
        Vector3 target = movingToEnd ? patrolEnd : patrolStart;
        Vector3 flatPosition = new Vector3(rb.position.x, 0f, rb.position.z);
        Vector3 flatTarget = new Vector3(target.x, 0f, target.z);
        Vector3 direction = (flatTarget - flatPosition).normalized;
        Vector3 move = direction * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + new Vector3(move.x, 0f, move.z));

        if (Vector3.Distance(flatPosition, flatTarget) < 0.1f)
        {
            movingToEnd = !movingToEnd;
        }
        return direction;
    }

    // Devuelve la dirección de movimiento usada
    private Vector3 ChasePlayer()
    {
        Vector3 flatPosition = new Vector3(rb.position.x, 0f, rb.position.z);
        Vector3 flatPlayer = new Vector3(player.position.x, 0f, player.position.z);
        Vector3 direction = (flatPlayer - flatPosition).normalized;
        Vector3 move = direction * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + new Vector3(move.x, 0f, move.z));
        return direction;
    }

    private void MaintainHeight()
    {
        RaycastHit hit;
        Vector3 planetUp = (transform.position - Planeta.planeta.transform.position).normalized;

        // Raycast hacia abajo relativo al dron (hacia el centro del planeta)
        if (Physics.Raycast(transform.position, -planetUp, out hit, 10f))
        {
            float desiredHeight = baseHeight + Mathf.Sin(bobbingTimer * bobbingFrequency) * bobbingAmplitude;
            float currentHeight = hit.distance;
            isGrounded = currentHeight <= desiredHeight + 0.05f;

            if (isGrounded)
            {
                // Si está en el suelo, reanudamos movimiento normal
                freezeHeight = false;
                canChase = true;

                Vector3 pos = rb.position;
                pos += planetUp * (desiredHeight - currentHeight);
                rb.MovePosition(pos);
            }
            else
            {
                // Si NO hay suelo, congelamos altura y detenemos persecución
                if (!freezeHeight)
                {
                    frozenHeight = Vector3.Dot(rb.position - Planeta.planeta.transform.position, planetUp);
                    freezeHeight = true;
                }
                canChase = false;

                Vector3 pos = rb.position;
                Vector3 planetCenterToPos = pos - Planeta.planeta.transform.position;
                pos = Planeta.planeta.transform.position + planetUp * frozenHeight + Vector3.ProjectOnPlane(planetCenterToPos, planetUp);
                rb.MovePosition(pos);
            }
        }
        else
        {
            // Raycast no detecta nada, también congelamos altura y detenemos persecución
            if (!freezeHeight)
            {
                frozenHeight = Vector3.Dot(rb.position - Planeta.planeta.transform.position, planetUp);
                freezeHeight = true;
            }
            canChase = false;

            Vector3 pos = rb.position;
            Vector3 planetCenterToPos = pos - Planeta.planeta.transform.position;
            pos = Planeta.planeta.transform.position + planetUp * frozenHeight + Vector3.ProjectOnPlane(planetCenterToPos, planetUp);
            rb.MovePosition(pos);

            isGrounded = false;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        Vector3 planetUp = (transform.position - Planeta.planeta.transform.position).normalized;
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();

        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 contactNormal = contact.normal;
            if (Vector3.Dot(contactNormal, -planetUp) > 0.5f)
            {
                // Jugador lo pisa desde arriba
                Destroy(gameObject);
                return;
            }
            else
            {
                // Colisión lateral: restar vida
                Destroy(gameObject);
                if (player != null) player.TakeDamage(1);
                return;
            }
        }
    }
}