using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Gravity : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Planeta.planeta.objetos.Add(GetComponent<Rigidbody>());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
