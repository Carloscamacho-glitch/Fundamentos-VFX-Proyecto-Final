using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Rigidbody rb;          // referencia al rigidbody
    [SerializeField] private Transform model;         // transform del modelo (hijo del jugador)
    [SerializeField] private Transform floor;           // transform para detectar suelo
    [SerializeField] private LayerMask floorMask;       // capa del suelo
    [SerializeField] private Transform cameraTransform; // la cámara principal o Cinemachine FreeLook

    [Header("Movimiento")]
    [SerializeField] private float speedMovement = 5f;  // velocidad de movimiento
    [SerializeField] private float turnTime = 1f;     // tiempo de giro
    [SerializeField] private float runMultiplier = 3f;
    [SerializeField] private float maxStamina;
    [SerializeField] private float stamina;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float staminaRecoveryRate = 1f;
    [SerializeField] private bool canRun;
    [SerializeField] private bool isRunnig;

    [Header("Salto")]
    [SerializeField] private float jumpForce = 5f;      // fuerza de salto
    [SerializeField] private bool doubleJumpUnlocked = false; // condición para habilitar doble salto
    [SerializeField] private bool isJumping = true;        // detectar si el jugador esta saltando
    [SerializeField] private bool canDoubleJump;    // si el jugador aún tiene el segundo salto disponible
    [SerializeField] private float floorDistance = 0.1f;// radio de detección
    [SerializeField] private bool jumpRequest;        // si el jugador ha solicitado un salto (input)
    [SerializeField] private bool inFloor;         // si el jugador está en el suelo

    [Header("Jetpack")]
    [SerializeField] private bool jetpack;         // si el jugador tiene jetpack
    [SerializeField] private bool jetpackActive;   // si el jetpack está activo
    [SerializeField] private float jetpackTimer;    // fuerza del jetpack
    [SerializeField] private float maxJetpackTime = 3f; // duración máxima del jetpack
    [SerializeField] private float jetpackForce;   // fuerza del jetpack

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        model = transform.GetChild(0).gameObject.transform;

        // Importante para que no se vuelque con físicas raras
        rb.freezeRotation = true;
    }

    void Update()
    {
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

        if (MovX == 0f && MovZ == 0f) return;

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
        if (Input.GetKey(KeyCode.LeftShift) && inFloor && canRun)
        {
            isRunnig = true;
            currentSpeed *= runMultiplier;
            stamina -= Time.deltaTime;
        }
        else
        {
            isRunnig = false;
            currentSpeed = speedMovement;
        }

        // Mover al jugador
        rb.MovePosition(rb.position + direction * currentSpeed * Time.deltaTime);

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
            isJumping = false;
            canDoubleJump = doubleJumpUnlocked;
            jetpackActive = false;
            jetpackTimer = 0f;
        }
        else
        {
            isJumping = true;
        }

        // Detectar input de salto y jetpack
        if (Input.GetKeyDown(KeyCode.Space) && inFloor)
        {
            jumpRequest = true;
            jetpackActive = false;
        }
        else if (Input.GetKeyDown(KeyCode.Space) && !inFloor && canDoubleJump)
        {
            jumpRequest = true;
            jetpackActive = false;
        }
        else if (Input.GetKeyDown(KeyCode.Space) && isJumping && !inFloor && jetpack)
        {
            jetpackActive = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space) && isJumping && !inFloor && jetpack)
        {
            jetpackActive = false;
        }
    }

    private void Jump()
    {
        // Detectar suelo
        inFloor = Physics.CheckSphere(floor.position, floorDistance, floorMask);

        // Calcular la dirección de la gravedad
        Vector3 gravityUp = (transform.position - Planeta.planeta.transform.position).normalized;

        // Primer salto
        if (jumpRequest && inFloor)
        {
            rb.AddForce(gravityUp * jumpForce, ForceMode.Impulse);
            jumpRequest = false;
        }
        // Segundo salto (en el aire)
        else if (jumpRequest && !inFloor && canDoubleJump)
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
}
