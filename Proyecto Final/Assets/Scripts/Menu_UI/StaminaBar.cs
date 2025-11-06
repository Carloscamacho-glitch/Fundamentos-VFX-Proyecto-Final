using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    // Actualiza el valor del relleno
    public void SetStamina(float current, float max)
    {
        if (fillImage == null) return;
        fillImage.fillAmount = Mathf.Clamp01(current / max);
    }
}
