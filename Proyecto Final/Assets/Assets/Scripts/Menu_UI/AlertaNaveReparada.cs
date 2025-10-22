using UnityEngine;
using UnityEngine.UI;

public class AlertaNaveReparada : MonoBehaviour
{
    [SerializeField] private GameObject AlertadeNaveReparada;

    public void MostrarAvisoNaveReparada(bool mostrar)
    {
        if (AlertadeNaveReparada != null)
            AlertadeNaveReparada.SetActive(mostrar);
    }
}
