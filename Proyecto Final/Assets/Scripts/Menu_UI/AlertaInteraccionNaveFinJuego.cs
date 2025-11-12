using UnityEngine;

public class AlertaInteraccionNaveFinJuego : MonoBehaviour
{
    [SerializeField] private GameObject AvisodeInteraccionNaveFinJuego;

    public void MostrarAvisodeInteraccionNaveFinJuego(bool mostrar)
    {
        if (AvisodeInteraccionNaveFinJuego != null)
            AvisodeInteraccionNaveFinJuego.SetActive(mostrar);
    }
}
