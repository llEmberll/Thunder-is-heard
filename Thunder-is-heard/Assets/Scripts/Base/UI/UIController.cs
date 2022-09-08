using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private BaseItemTweaker itemTweaker;

    [SerializeField] private Image itemTweakPanel;

    [SerializeField] private Image shopButton, inventoryButton;


    private void Start()
    {
        itemTweakPanel.enabled = false;
        shopButton.enabled = inventoryButton.enabled = true;
    }

    public void PressShop()
    {
        itemTweakPanel.enabled = !itemTweakPanel.enabled;
        if (itemTweakPanel.enabled) itemTweaker.FillContent();
    }

    public void PressInventory()
    {
        itemTweakPanel.enabled = !itemTweakPanel.enabled;
    }
}
