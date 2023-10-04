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
        } // 쌓일 수 있는 아이템이면 쌓기

        ItemSlot emptySlot = GetEmptySlot();

        if (emptySlot != null)
        {
            emptySlot.item = item;
            emptySlot.quantity = 1;
            UpdateUI();
            return;
        }
        // 새로 얻은 아이템이면 빈 공간에 넣기

        ThrowItem(item); // 인벤토리가 꽉 찬 상태면 버리기
    }

    void ThrowItem(ItemData item) // 아이템 버리기
    {
        Instantiate(item.dropPrefab, dropPosition.position, Quaternion.Euler(Vector3.one * Random.value * 360f));
    }

    void UpdateUI()
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item != null)
                uislots[i].Set(slots[i]);
            else
                uislots[i].Clear();
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

        //

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

    }

    public void RemoveItem(ItemData item)
    {

    }

    public bool HasItem(ItemData item, int quantity)
    {
        return false;
    }
}
