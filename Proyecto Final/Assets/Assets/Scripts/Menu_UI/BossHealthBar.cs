using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    public void SetHealth(float current, float max)
    {
        if (fillImage != null)
            fillImage.fillAmount = current / max;
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
