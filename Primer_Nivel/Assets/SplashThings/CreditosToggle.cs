using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToggleButtonColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button button;
    public Color normalColor = Color.white;
    public Color activeColor = Color.green;
    public Color hoverColor = Color.yellow;

    private bool isActive = false;
    private Image img;

    void Start()
    {
        img = button.GetComponent<Image>();
        img.color = normalColor;
        button.onClick.AddListener(ToggleActive);
    }

    void ToggleActive()
    {
        isActive = !isActive;
        img.color = isActive ? activeColor : normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isActive)
            img.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        img.color = isActive ? activeColor : normalColor;
    }
}