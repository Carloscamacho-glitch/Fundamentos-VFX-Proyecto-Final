using UnityEngine;

public class AlertaInteraccionJefe : MonoBehaviour
{
    [SerializeField] private GameObject AvisodeInteraccionJefe;

    public void MostrarAvisodeInteraccionJefe(bool mostrar)
    {
        if (AvisodeInteraccionJefe != null)
            AvisodeInteraccionJefe.SetActive(mostrar);
    }
}
