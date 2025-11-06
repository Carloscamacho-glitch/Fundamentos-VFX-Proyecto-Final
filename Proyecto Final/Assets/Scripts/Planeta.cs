using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planeta : MonoBehaviour
{
    [Header("Referencias")]
    public List<Rigidbody> objetos = new List<Rigidbody>();         // lista de objetos afectados por la gravedad del planeta
    public static Planeta planeta;                                  // referencia estática al planeta

    [Header("Gravedad del Planeta")]
    [SerializeField] private float gravity;                         // fuerza de gravedad del planeta
    [SerializeField] private float radiusGravity;                   // radio de influencia de la gravedad del planeta

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        planeta = this;
    }

    void FixedUpdate()
    {
        foreach (Rigidbody objeto in objetos)
        {
            // Aplica la gravedad si los objeotos estan dentro del radio de gravedad del planeta
            if ((transform.position - objeto.transform.position).magnitude <= radiusGravity)
            {
                // Obitene los valores de direccion de la gravedad y de "arriba" de los objetos
                Vector3 dirGravity = (objeto.transform.position - transform.position).normalized;
                Vector3 localUp = objeto.transform.up;

                // Agrega la fuerza de gravedad al objeto
                objeto.AddForce(dirGravity * gravity * Time.fixedDeltaTime);

                // Aplica la rotacion al objeto para ajustarlo a la orientacion del planeta
                Quaternion targetRotation = Quaternion.FromToRotation(localUp, dirGravity) * objeto.transform.rotation;
                objeto.transform.rotation = Quaternion.Slerp(objeto.transform.rotation, targetRotation, 50 * Time.fixedDeltaTime);
            }
        }
    }
}
