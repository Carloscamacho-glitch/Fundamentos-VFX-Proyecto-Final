using UnityEngine;

public class AlertaInteraccionNave : MonoBehaviour
{
    [SerializeField] private GameObject AvisodeInteraccionNave;

    public void MostrarAvisodeInteraccionNave(bool mostrar)
    {
        if (AvisodeInteraccionNave != null)
            AvisodeInteraccionNave.SetActive(mostrar);
    }
}
