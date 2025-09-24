using UnityEngine;

public class DronController : MonoBehaviour
{
    [Header("Patrullaje")]
    [SerializeField] private float patrolDistance;
    private Vector3 patrolStart;
    private Vector3 patrolEnd;
    private bool movingToEnd = true;

    [Header("Persecusión")]
    [SerializeField] private float chaseRange = 7.5f;
    [SerializeField] private float bobbingAmplitude = 0.5f;
    [SerializeField] private float bobbingFrequency = 5f;
    [SerializeField] private float baseHeight = 1f;
    private float bobbingTimer = 0f;
    private bool freezeHeight = false;
    private float frozenHeight = 0f;
    private bool canChase = true;

    [Header("Movimiento")]
    [SerializeField] private float speed;
    private Vector3 lastMoveDirection = Vector3.forward;
    private bool isGrounded;

    [Header("Ataque")]
    [SerializeField] private float closeRange = 3f;
    [SerializeField] private float closeDuration = 3f;
    private float closeTimer = 0f;
    private bool inCloseRange = false;
    [SerializeField] private float retreatDuration = 0.5f; // tiempo que retrocede antes del ataque
    private float retreatTimer = 0f;
    private bool isRetreating = false;

    [Header("Referencias")]
    private Rigidbody rb;
    private Transform player;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        patrolStart = transform.position - transform.forward * patrolDistance * 0.5f;
        patrolEnd = transform.position + transform.forward * patrolDistance * 0.5f;
    }

    private void Update()
    {
        bobbingTimer += Time.deltaTime;

        // Solo rota si NO está en ataque recto
        if (!inCloseRange && lastMoveDirection != Vector3.zero)
        {
            Vector3 planetUp = (transform.position - Planeta.planeta.transform.position).normalized;
            Vector3 moveDirOnPlane = Vector3.ProjectOnPlane(-lastMoveDirection, planetUp).normalized;

            if (moveDirOnPlane != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirOnPlane, planetUp);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 0.2f));
            }
        }
    }

    private void FixedUpdate()
    {
        if (player != null && canChase)
        {
            float distance = Vector3.Distance(rb.position, player.position);

            if (inCloseRange)
            {
                if (isRetreating)
                {
                    RetreatBehavior();
                }
                else
                {
                    AttackBehavior();
                }
            }
            else
            {
                if (distance < closeRange)
                {
                    StartAttack(); // inicia el ataque con retroceso primero
                }
                else if (distance < chaseRange)
                {
                    lastMoveDirection = ChasePlayer();
                }
                else
                {
                    lastMoveDirection = Patrol();
                }
            }
        }
        else
        {
            lastMoveDirection = Patrol();
        }

        MaintainHeight();
    }

    private void StartAttack()
    {
        inCloseRange = true;
        closeTimer = closeDuration;

        // Dirección hacia el jugador
        Vector3 flatPosition = new Vector3(rb.position.x, 0f, rb.position.z);
        Vector3 flatPlayer = new Vector3(player.position.x, 0f, player.position.z);
        lastMoveDirection = (flatPlayer - flatPosition).normalized;

        // Activa retroceso
        isRetreating = true;
        retreatTimer = retreatDuration;
    }

    // 🔹 Retrocede antes del ataque
    private void RetreatBehavior()
    {
        float moveSpeed = speed; // velocidad normal al retroceder
        rb.MovePosition(rb.position - lastMoveDirection * moveSpeed * Time.fixedDeltaTime);

        retreatTimer -= Time.fixedDeltaTime;
        if (retreatTimer <= 0f)
        {
            isRetreating = false; // pasa al ataque real
        }
    }

    // 🔹 Ataque hacia adelante
    private void AttackBehavior()
    {
        float moveSpeed = speed * 2f;
        rb.MovePosition(rb.position + lastMoveDirection * moveSpeed * Time.fixedDeltaTime);

        closeTimer -= Time.fixedDeltaTime;
        if (closeTimer <= 0f)
        {
            inCloseRange = false; // termina ataque
        }
    }

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

        if (Physics.Raycast(transform.position, -planetUp, out hit, 10f))
        {
            float desiredHeight = baseHeight + Mathf.Sin(bobbingTimer * bobbingFrequency) * bobbingAmplitude;
            float currentHeight = hit.distance;

            

            // Actualiza estado de grounded
            isGrounded = Mathf.Abs(currentHeight - desiredHeight) < 0.8f;

            if (isGrounded)
            {
                // Si está en el suelo, reanudamos movimiento normal
                freezeHeight = false;
                canChase = true;

                // Ajusta altura siempre, pegado al suelo
                Vector3 pos = rb.position;
                pos += planetUp * (desiredHeight - currentHeight);
                rb.MovePosition(pos);
            }
            else
            {
                // Si no hay suelo, congelar altura en el último frozenHeight
                if (!freezeHeight)
                {
                    // Guardamos altura relativa al planeta
                    frozenHeight = Vector3.Dot(rb.position - Planeta.planeta.transform.position, planetUp);
                    freezeHeight = true;
                }

                canChase = false;

                // Mantener altura congelada mientras sigue moviéndose sobre el plano tangente
                Vector3 pos = rb.position;
                Vector3 planetCenterToPos = pos - Planeta.planeta.transform.position;
                pos = Planeta.planeta.transform.position + planetUp * frozenHeight + Vector3.ProjectOnPlane(planetCenterToPos, planetUp);
                rb.MovePosition(pos);

                isGrounded = false;
            }
        }
        else
        {
            // Si no hay suelo, congelar altura en el último frozenHeight
            if (!freezeHeight)
            {
                // Guardamos altura relativa al planeta
                frozenHeight = Vector3.Dot(rb.position - Planeta.planeta.transform.position, planetUp);
                freezeHeight = true;
            }

            canChase = false;

            // Mantener altura congelada mientras sigue moviéndose sobre el plano tangente
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
                Destroy(gameObject);
                return;
            }
            else
            {
                Destroy(gameObject);
                if (player != null) player.TakeDamage(1);
                return;
            }
        }
    }
}
