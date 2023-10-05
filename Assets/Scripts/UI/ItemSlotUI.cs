using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{

    public Button button;
    public Image icon;
    public TextMeshProUGUI quatityText;
    public ItemSlot curSlot;
    private Outline outline;

    public int index;
    public bool equipped;

    private void Awake()
    {
        outline = GetComponent<Outline>();
    }

    private void OnEnable()
    {
        outline.enabled = equipped;
    }

    public void Set(ItemSlot slot)
    {
        curSlot = slot;
        icon.gameObject.SetActive(true);
        icon.sprite = slot.item.icon;
        quatityText.text = slot.quantity > 1 ? slot.quantity.ToString() : string.Empty;
        // 현재 슬롯에 아이콘 및 수량 표시
        if (outline != null)
        {
            outline.enabled = equipped;
        }
        // 장착되어있으면 아웃라인 강조
    }

    public void Clear()
    {
        curSlot = null;
        icon.gameObject.SetActive(false);
        quatityText.text = string.Empty;
        // 아이콘 끄기, 수량 표기 없애기
    }

    public void OnClickButton()
    {
        Inventory.instance.SelectItem(index);
    }
}
