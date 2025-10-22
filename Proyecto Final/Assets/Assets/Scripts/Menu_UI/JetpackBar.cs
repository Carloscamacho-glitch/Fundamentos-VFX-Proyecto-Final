using UnityEngine;
using UnityEngine.UI;

public class JetpackBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    // Actualiza el valor del relleno
    public void SetJetpack(float current, float max)
    {
        if (fillImage == null) return;
        fillImage.fillAmount = Mathf.Clamp01(current / max);
    }
}
