using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planeta : MonoBehaviour
{
    [SerializeField] public static Planeta planeta;
    [SerializeField] private float gravity;

    public List<Rigidbody> objetos = new List<Rigidbody>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        planeta = this;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (Rigidbody objeto in objetos)
        {
            Vector3 dirGravity = (objeto.transform.position - transform.position).normalized;
            Vector3 localUp = objeto.transform.up;
            objeto.AddForce(dirGravity * gravity * Time.fixedDeltaTime);
            Quaternion targetRotation = Quaternion.FromToRotation(localUp, dirGravity) * objeto.transform.rotation;
            objeto.transform.rotation = Quaternion.Slerp(objeto.transform.rotation, targetRotation, 50 * Time.fixedDeltaTime);
        }
    }
}
