using UnityEngine;

public class MobilePlatform : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform planetCenter;//Centro del planeta para usarlo como pivote

    [Header("Movimiento")]
    [SerializeField] private float rotationSpeed;   //Velocidad con la que se realizara el movimiento
    [SerializeField] private float maxAngle;        // límite de rotación en grados
    private float currentAngle = 0f;                //coloca el angulo actual como 0 para aplicar las transfomraciones apartir de ahi
    private int direction = 1;                      // 1 = ida, -1 = vuelta

    [SerializeField] private Vector3 rotationAxis;                    // Eje en el que se realiza el movimiento de la plataforma

    // Se aplica la rotación a la plataforma
    private void FixedUpdate()
    {
        if (planetCenter == null) return;

        // cálculo rotación
        float deltaAngle = rotationSpeed * Time.fixedDeltaTime * direction;

        if (Mathf.Abs(currentAngle + deltaAngle) > maxAngle)
        {
            deltaAngle = maxAngle * direction - currentAngle;
            direction *= -1;
        }

        transform.RotateAround(planetCenter.position, rotationAxis, deltaAngle);
        currentAngle += deltaAngle;
    }

    // Se aplica la rotación al jugador
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody rb = collision.rigidbody;
            if (rb != null)
            {
                // velocidad tangencial real de la plataforma
                Vector3 tangentialVelocity = Vector3.Cross(
                    rotationAxis.normalized,
                    collision.transform.position - planetCenter.position
                ) * (rotationSpeed * Mathf.Deg2Rad * direction);

                rb.MovePosition(rb.position + tangentialVelocity * Time.fixedDeltaTime);
            }
        }
    }
}
