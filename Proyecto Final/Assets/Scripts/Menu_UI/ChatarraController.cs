using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatarraController : MonoBehaviour
{
    [SerializeField] private Text chatarraText;

    public void UpdateChatarra(int cantidad)
    {
        chatarraText.text = cantidad.ToString();
    }
}
