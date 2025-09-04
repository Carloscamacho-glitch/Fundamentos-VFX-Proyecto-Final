using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;

    [Header("Movimiento")]
    //Velocidad
    private Vector3 speed;
    //velocidad de movimiento
    [SerializeField] private float speedMovement;
    //Velocidad de giro
    private float turnSpeed;
    //tiempo de giro
    [SerializeField] private float turnTime;

    [Header("Salto")]
    //gravedad
    [SerializeField] private float gravity;
    //si esta en el suelo
    private bool inFloor;
    //fuerza de salto
    [SerializeField] private float jumpForce;
    //altura de salto
    [SerializeField] private float jumpHeight;
    //transform del suelo
    [SerializeField] private Transform floor;
    //distancia al suelo
    [SerializeField] private float floorDistance;
    //capa del suelo
    [SerializeField] private LayerMask floorMask;

    private Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Walk();
    }

    private void FixedUpdate()
    {
        //jump();
    }

    private void Walk()
    {
        float MoveX = Input.GetAxisRaw("Horizontal");
        float MoveZ = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(MoveX, 0, MoveZ);

        if (direction != Vector3.zero)
        {
            float rotationAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref turnSpeed, turnTime);

            Vector3 moveDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
            transform.rotation = Quaternion.Euler(0, angle, 0);
            controller.Move(moveDirection.normalized * speedMovement * Time.deltaTime);
        }
    }

    private void jump()
    {
        inFloor = Physics.CheckSphere(floor.position, floorDistance, floorMask);
        if (inFloor && speed.y < 0)
        {
            speed.y = jumpForce;
        }
        if (Input.GetKeyDown(KeyCode.Space) && inFloor)
        {
            speed.y = Mathf.Sqrt(jumpHeight * jumpForce * gravity);
        }
        speed.y += gravity * Time.deltaTime;
        controller.Move(speed * Time.deltaTime);
    }
}
