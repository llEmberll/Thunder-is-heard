using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceController : MonoBehaviour
{
    public Text resourceText;

    public string money;
    public string steel;
    public string oil;
    public string command_resource;
    public string count_separator;
    public string value_separator;

    public int money_count;
    public int steel_count;
    public int oil_count;
    public int command_resource_count;
    


    // Update is called once per frame
    void Update()
    {
        UpdateResources();
    }

    private void UpdateResources()
    {
        resourceText.text = money + count_separator + money_count + value_separator + steel + count_separator + steel_count + value_separator + oil + count_separator + oil_count + value_separator + command_resource + count_separator + command_resource_count;
    }
}
