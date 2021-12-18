using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceController : MonoBehaviour
{
    [SerializeField] private int steelCount;
    [SerializeField] private int maxSteelCount;

    [SerializeField] private int oilCount;
    [SerializeField] private int maxOilCount;

    [SerializeField] private int moneyCount; 

    public Text money;
    public Text steel;
    public Text oil;


    // Update is called once per frame
    private void Start()
    {
        UpdateResources();
    }

    private void UpdateResources()
    {
        money.text = $"{moneyCount}";
        steel.text = $"{steelCount}/{maxSteelCount}";
        oil.text = $"{oilCount}/{maxOilCount}";
    }
}
