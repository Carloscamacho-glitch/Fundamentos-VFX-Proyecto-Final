using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planeta : MonoBehaviour
{
    [SerializeField] public static Planeta planeta;
    [SerializeField] private float gravity;

    public List<CharacterController> objetos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        planeta = this;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (CharacterController objeto in objetos)
        {
            Vector3 dirGravity = (objeto.transform.position - transform.position).normalized;
            Vector3 localUp = objeto.transform.up;
            objeto.Move(dirGravity * gravity * Time.fixedDeltaTime);
            //Quaternion targetRotation = Quaternion.FromToRotation(dirGravity, localUp) * objeto.transform.rotation;
            //objeto.transform.rotation = Quaternion.Slerp(objeto.transform.rotation, targetRotation, 50 * Time.fixedDeltaTime);
        }
    }
}
