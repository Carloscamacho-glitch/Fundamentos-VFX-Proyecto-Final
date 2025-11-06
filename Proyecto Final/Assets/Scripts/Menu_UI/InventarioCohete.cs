using UnityEngine;
using UnityEngine.UI;

public class InventarioCohete : MonoBehaviour
{
    [SerializeField] private Image materialIcon;
    [SerializeField] private Image fillImage;

    private void Start()
    {
        materialIcon.enabled = false;
    }

    public void SetCohete()
    {
        materialIcon.enabled = true;

        if (fillImage == null) return;
            fillImage.fillAmount = 1;
    }
}
