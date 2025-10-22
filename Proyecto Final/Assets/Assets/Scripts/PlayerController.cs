using System.Diagnostics;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Rigidbody rb;                          // referencia al rigidbody
    [SerializeField] private Transform model;                       // transform del modelo (hijo del jugador)
    [SerializeField] private Transform floor;                       // transform para detectar suelo
    [SerializeField] private LayerMask floorMask;                   // capa del suelo
    [SerializeField] private Transform cameraTransform;             // la cámara principal o Cinemachine FreeLook
    [SerializeField] private HealthBar healthBar;                   // referencia a la barra de vida
    [SerializeField] private StaminaBar staminaBar;                 // referencia a la barra de stamina
    [SerializeField] private ChatarraController chatarraController; // referencia a la barra de vida
    [SerializeField] private Animator animator;                     // referencia al animator
    [SerializeField] private GameObject[] coheteParticles;          // Partículas de los cohetes

    [Header("Movimiento")]
    [SerializeField] private float speedMovement = 5f;              // velocidad de movimiento
    [SerializeField] private float turnTime = 1f;                   // tiempo de giro
    [SerializeField] private float runMultiplier = 4f;              // multiplicador de velocidad al correr
    [SerializeField] private float maxStamina;                      // stamina máxima
    [SerializeField] private float stamina;                         // stamina actual
    [SerializeField] private float currentSpeed;                    // velocidad actual
    [SerializeField] private float staminaRecoveryRate = 1f;        // tasa de recuperación de stamina
    [SerializeField] private bool canRun;                           // si el jugador puede correr
    [SerializeField] private bool isRunnig;                         // si el jugador está corriendo

    [Header("Salto")]
    [SerializeField] private float jumpForce = 5f;                  // fuerza de salto
    [SerializeField] private bool doubleJumpUnlocked = false;       // condición para habilitar doble salto
    [SerializeField] private bool isJumping = true;                 // detectar si el jugador esta saltando
    [SerializeField] private bool canDoubleJump;                    // si el jugador aún tiene el segundo salto disponible
    [SerializeField] private float floorDistance = 0.1f;            // radio de detección
    [SerializeField] private bool jumpRequest;                      // si el jugador ha solicitado un salto (input)
    [SerializeField] private bool inFloor;                          // si el jugador está en el suelo

    [Header("Jetpack")]
    [SerializeField] private bool jetpack;                          // si el jugador tiene jetpack
    [SerializeField] private bool jetpackActive;                    // si el jetpack está activo
    [SerializeField] private float jetpackTimer;                    // fuerza del jetpack
    [SerializeField] private float maxJetpackTime = 3f;             // duración máxima del jetpack
    [SerializeField] private float jetpackForce;                    // fuerza del jetpack

    [Header("Configuración de Vida")]
    [SerializeField] private int maxHealth = 3;                     // Máxima cantidad de vida
    [SerializeField] private int currentHealth = 3;                 // Vida actual

    [Header("Objetos recogibles")]
    [SerializeField] private int totalChatarra = 0;                 // Cantidad total de chatarra recogida
    [SerializeField] private int totalMateriales = 0;               // Cantidad total de materiales recogidos
    [SerializeField] private bool Motor = false;                    // Si el jugador tiene el motor

    [Header("Respawn")]
    public Transform spawnPoint;                                    // Lugar donde reaparecerá al morir

    public bool TieneMotor() => Motor;
    public int GetMateriales() => totalMateriales;

    [SerializeField] private bool hasTouchedGround = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        model = transform.GetChild(0).gameObject.transform;

        rb.freezeRotation = true;
    }

    void Update()
    {
        if (healthBar != null)
            healthBar.SetHealth(currentHealth, maxHealth);

        if (staminaBar != null)
            staminaBar.SetStamina(stamina, maxStamina);

        if (chatarraController != null)
            chatarraController.UpdateChatarra(totalChatarra);
        
        Walk();
        JumpRequest();
    }

    void FixedUpdate()
    {
        Jump();
    }

    private void Walk()
    {
        float MovX = Input.GetAxisRaw("Horizontal");
        float MovZ = Input.GetAxisRaw("Vertical");

        if (MovX != 0f || MovZ != 0f)
        {
            animator.SetBool("Walk", true);

            // --- Dirección de la cámara ---
            Vector3 gravityUp = (transform.position - Planeta.planeta.transform.position).normalized;

            // Forward de la cámara proyectado en el plano tangente al planeta
            Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, gravityUp).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, gravityUp).normalized;

            // Dirección de movimiento en función de la cámara
            Vector3 direction = (camRight * MovX + camForward * MovZ).normalized;

            // Rotación del modelo hacia la dirección de movimiento
            Quaternion targetRot = Quaternion.LookRotation(direction, gravityUp);
            model.transform.rotation = Quaternion.Slerp(model.transform.rotation, targetRot, turnTime * 10f * Time.deltaTime);

            currentSpeed = speedMovement;

            // Sprint
            if (Input.GetKey(KeyCode.LeftShift) && inFloor && 0f <= stamina && canRun)
            {
                isRunnig = true;
                currentSpeed *= runMultiplier;
                stamina -= Time.deltaTime;
                animator.speed = 2f;
            }
            else
            {
                isRunnig = false;
                currentSpeed = speedMovement;
                animator.speed = 1f;
            }

            // Mover al jugador
            rb.MovePosition(rb.position + direction * currentSpeed * Time.deltaTime);
        }
        else
        {
            animator.SetBool("Walk", false);
        }

        // Recuperar stamina si no corre
        if (!isRunnig && stamina < maxStamina)
        {
            stamina += staminaRecoveryRate * Time.deltaTime;
            canRun = stamina >= 2f;
        }
    }

    private void JumpRequest()
    {
        // Reset al tocar el suelo
        if (inFloor)
        {
            DesactivarCohetes();
            isJumping = false;
            canDoubleJump = doubleJumpUnlocked;
            jetpackActive = false;
            jetpackTimer = 0f;
            animator.SetBool("Jumping", false);
            animator.SetBool("Jumping2", false);
            animator.SetBool("Jetpack", false);
            animator.SetBool("Fall", false);
        }
        else
        {
            isJumping = true;
        }

        // Detectar input de salto y jetpack
        if (Input.GetKeyDown(KeyCode.Space) && inFloor && !isJumping)
        {
            animator.SetBool("Jumping", true);
            jumpRequest = true;
            jetpackActive = false;
            hasTouchedGround = false;
        }
        else if (Input.GetKeyDown(KeyCode.Space) && isJumping && !inFloor && canDoubleJump)
        {
            animator.SetBool("Jumping2", true);
            jumpRequest = true;
            jetpackActive = false;
            animator.SetBool("Jumping", false);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && isJumping && !inFloor && jetpack)
        {
            ActivarCohetes();
            animator.SetBool("Jetpack", true);
            jetpackActive = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space) && isJumping && !inFloor && jetpack)
        {
            DesactivarCohetes();
            jetpackActive = false;
            animator.SetBool("Jumping2", false);
            animator.SetBool("Jetpack", false);
            animator.SetBool("Fall", true);
        }
    }

    private void Jump()
    {
        // Detectar suelo
        inFloor = Physics.CheckSphere(floor.position, floorDistance, floorMask);

        // Calcular la dirección de la gravedad
        Vector3 gravityUp = (transform.position - Planeta.planeta.transform.position).normalized;

        // Primer salto
        if (jumpRequest && inFloor && !isJumping)
        {
            rb.AddForce(gravityUp * jumpForce, ForceMode.Impulse);
            jumpRequest = false;
        }
        // Segundo salto (en el aire)
        else if (jumpRequest && isJumping && !inFloor && canDoubleJump)
        {
            // Reiniciar la velocidad vertical antes de aplicar el impulso
            Vector3 velocity = rb.linearVelocity;
            float upVelocity = Vector3.Dot(velocity, gravityUp);
            rb.linearVelocity = velocity - gravityUp * upVelocity;

            rb.AddForce(gravityUp * jumpForce, ForceMode.Impulse);

            canDoubleJump = false; // ya gastó el segundo salto
        }
        // Jetpack
        else if (jetpackActive && jetpackTimer < maxJetpackTime)
        {
            // Aplicar fuerza continua mientras se mantiene presionada
            rb.AddForce(gravityUp * jetpackForce, ForceMode.Impulse);

            jetpackTimer += Time.fixedDeltaTime;

            // Limitar duración del jetpack
            if (jetpackTimer >= maxJetpackTime)
                jetpackActive = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Planet"))
        {
            TakeDamage(3);
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Floor") && !hasTouchedGround)
        {
            hasTouchedGround = true;
            StopAllCoroutines(); // por si cae varias veces seguidas
            StartCoroutine(TemporaryKinematic());
        }
    }

    private System.Collections.IEnumerator TemporaryKinematic()
    {
        // Detener movimiento antes de volverlo kinematic
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Esperar un pequeño frame antes de activar kinematic
        yield return new WaitForFixedUpdate();

        rb.isKinematic = true;
        yield return new WaitForSeconds(0.05f); // pausa corta
        rb.isKinematic = false;
    }

    // Llamar a esta función cuando el jugador reciba daño
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        // Reinicia la vida
        currentHealth = maxHealth;

        // Regresa al punto de inicio
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }
    }

    public void AddChatarra(int cantidad)
    {
        totalChatarra += cantidad;
    }

    public void AddMateriales(int cantidad)
    {
        totalMateriales += cantidad;
    }

    public void AddMotor(bool valor)
    {
        Motor = valor;
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
