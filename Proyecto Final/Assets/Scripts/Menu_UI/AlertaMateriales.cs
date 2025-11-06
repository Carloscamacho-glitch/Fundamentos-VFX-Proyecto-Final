using UnityEngine;
using UnityEngine.UI;

public class AlertaMateriales : MonoBehaviour
{
    [SerializeField] private GameObject AlertaFaltanMateriales;

    public void MostrarAvisoFaltanMateriales(bool mostrar)
    {
        if (AlertaFaltanMateriales != null)
            AlertaFaltanMateriales.SetActive(mostrar);
    }
}
