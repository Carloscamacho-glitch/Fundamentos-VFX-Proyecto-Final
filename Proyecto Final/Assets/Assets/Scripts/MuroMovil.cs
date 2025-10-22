using UnityEngine;

public class MuroMovil : MonoBehaviour
{
    [Header("Movimiento del muro")]
    [SerializeField] private float distanciaMovimiento = 20f;  // Distancia que se moverá sobre su eje Z local
    [SerializeField] private float velocidad = 5f;             // Velocidad del movimiento

    private Vector3 posicionInicial;
    private Vector3 posicionFinal;
    [SerializeField] private bool subiendo = false;
    [SerializeField] private bool bajando = false;

    void Start()
    {
        // Guardamos la posición inicial y calculamos la final en base al eje Z local
        posicionInicial = transform.position;
        posicionFinal = posicionInicial + transform.forward * distanciaMovimiento;
    }

    void Update()
    {
        if (subiendo)
        {
            // Movimiento hacia adelante (subida)
            transform.position = Vector3.MoveTowards(transform.position, posicionFinal, velocidad * Time.deltaTime);

            if (Vector3.Distance(transform.position, posicionFinal) < 0.01f)
                subiendo = false;
        }

        if (bajando)
        {
            // Movimiento hacia atrás (descenso)
            transform.position = Vector3.MoveTowards(transform.position, posicionInicial, velocidad * Time.deltaTime);

            if (Vector3.Distance(transform.position, posicionInicial) < 0.01f)
                bajando = false;
        }
    }

    public void ActivarMovimiento()
    {
        if (!subiendo && !bajando)
            subiendo = true;
    }

    public void DesactivarMovimiento()
    {
        if (!bajando && !subiendo)
            bajando = true;
    }
}
