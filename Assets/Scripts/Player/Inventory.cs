using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ItemSlot
{
    public ItemData item;
    public int quantity;
}

public class Inventory : MonoBehaviour
{
    public ItemSlotUI[] uislots;
    public ItemSlot[] slots;

    public GameObject inventoryWindow;
    public Transform dropPosition;

    private ItemSlot selectedItem;
    private int selectedItemIndex;
    [Header("# Selected Item")]
    public TextMeshProUGUI seletedItemName;
    public TextMeshProUGUI seletedItemDescription;
    public TextMeshProUGUI seletedItemStatNames;
    public TextMeshProUGUI seletedItemStatValues;
    public GameObject useButton;
    public GameObject equipButton;
    public GameObject unEquipButton;
    public GameObject dropButton;

    private int curEquipIndex;

    private PlayerController controller;
    private PlayerConditions condition;

    [Header("# Events")]
    public UnityEvent onOpenInventory;
    public UnityEvent onCloseInventory;

    public static Inventory instance;

    void Awake()
    {
        instance = this;
        controller = GetComponent<PlayerController>();
        condition = GetComponent<PlayerConditions>();
    }

    private void Start()
    {
        inventoryWindow.SetActive(false);
        slots = new ItemSlot[uislots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new ItemSlot();
            uislots[i].index = i;
            uislots[i].Clear();
        }

        ClearSelectedItemWindow();
    }

    public void OnInventoryButton(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.phase == InputActionPhase.Started)
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        if(inventoryWindow.activeInHierarchy)
        {
            inventoryWindow.SetActive(false);
            onCloseInventory?.Invoke();
            controller.ToggleCursor(false);
        }
        else
        {
            inventoryWindow.SetActive(true);
            onOpenInventory?.Invoke();
            controller.ToggleCursor(true);
        }
    }   

    public bool IsOpen()
    {
        return inventoryWindow.activeInHierarchy;
    }

    public void AddItem(ItemData item)
    {
        if(item.canStack)
        {
            ItemSlot slotToStackTo = GetItemStack(item);
            if(slotToStackTo != null)
            {
                slotToStackTo.quantity++;
                UpdateUI();
                return;
            }
        } // ���� �� �ִ� �������̸� �ױ�

        ItemSlot emptySlot = GetEmptySlot();

        if (emptySlot != null)
        {
            emptySlot.item = item;
            emptySlot.quantity = 1;
            UpdateUI();
            return;
        }
        // ���� ���� �������̸� �� ������ �ֱ�

        ThrowItem(item); // �κ��丮�� �� �� ���¸� ������
    }

    void ThrowItem(ItemData item) // ������ ������
    {
        Instantiate(item.dropPrefab, dropPosition.position, Quaternion.Euler(Vector3.one * Random.value * 360f));
    }

    void UpdateUI() // UI ������Ʈ
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item != null)
                uislots[i].Set(slots[i]);
            // �������� ������ ���� �Ҵ� - Set()
            else
                uislots[i].Clear();
            // �������� ������ ���� ���� - Clear();
        }
    }

    ItemSlot GetItemStack(ItemData item)
    {
        for(int i = 0; i<slots.Length; i++)
        {
            if (slots[i].item == item && slots[i].quantity < item.maxStackAmount)
                return slots[i];
        }
        return null;
    }

    ItemSlot GetEmptySlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == null)
                return slots[i];
        }
        return null;
    }

    public void SelectItem(int index)
    {
        if (slots[index].item == null)
            return;

        selectedItem = slots[index];
        selectedItemIndex = index;

        seletedItemName.text = selectedItem.item.displayName;
        seletedItemDescription.text = selectedItem.item.description;

        seletedItemStatNames.text = string.Empty;
        seletedItemStatValues.text = string.Empty;

        for (int i = 0; i < selectedItem.item.consumables.Length; i++)
        {
            seletedItemStatNames.text += selectedItem.item.consumables[i].type.ToString() + "\n";
            seletedItemStatValues.text += selectedItem.item.consumables[i].value.ToString() + "\n";
        }

        useButton.SetActive(selectedItem.item.type == ItemType.Consumable);
        equipButton.SetActive(selectedItem.item.type == ItemType.Equipable && !uislots[index].equipped);
        unEquipButton.SetActive(selectedItem.item.type == ItemType.Equipable && uislots[index].equipped);
        dropButton.SetActive(true);
    }

    private void ClearSelectedItemWindow()
    {
        selectedItem = null;
        seletedItemName.text = string.Empty;
        seletedItemDescription.text = string.Empty;

        seletedItemStatNames.text = string.Empty;
        seletedItemStatValues.text = string.Empty;

        useButton.SetActive(false);
        equipButton.SetActive(false);
        unEquipButton.SetActive(false);
        dropButton.SetActive(false);
    }

    public void OnClickUseButton()
    {
        if(selectedItem.item.type == ItemType.Consumable)
        {
            for (int i = 0; i < selectedItem.item.consumables.Length; i++)
            {
                switch(selectedItem.item.consumables[i].type)
                {
                    case ConsumableType.Health:
                        condition.Heal(selectedItem.item.consumables[i].value);
                        break;
                    case ConsumableType.Hunger:
                        condition.Eat(selectedItem.item.consumables[i].value);
                        break;
                }
            }
            // ������ Ÿ���� �Ҹ��� - �Ҹ��� Ÿ��(ü��,�����) �� ���� 
            // Heal, Eat ���� (value ���� �Ű����� amount �� ����)
        }
        RemoveSelectedItem(); // ����� ������ ����
    }

    public void OnClickEquipButton()
    {

    }

    void UnEquip(int index)
    {

    }

    public void OnClickUnEquipButton()
    {

    }

    public void OnClickDropButton()
    {
        ThrowItem(selectedItem.item);
        RemoveSelectedItem();
    }

    private void RemoveSelectedItem()
    {
        selectedItem.quantity--;
        // ���� ����
        if(selectedItem.quantity <= 0 ) // ������ 0 ���� ���� ���
        {
            if (uislots[selectedItemIndex].equipped)
            {
                UnEquip(selectedItemIndex);
            } // �����Ǿ������� ���� ����

            selectedItem.item = null;
            ClearSelectedItemWindow();
            // �κ��丮 ����
        }

        UpdateUI();
    }

    public void RemoveItem(ItemData item)
    {

    }

    public bool HasItem(ItemData item, int quantity)
    {
        return false;
    }
}
