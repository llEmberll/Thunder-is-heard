using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseItemTweaker : MonoBehaviour
{
    [SerializeField] private RectTransform prefab;
    [SerializeField] private RectTransform content;

    private int elementsCount = 0;

    private Prefabs dataBase;
    private PlayerData playerData;

    private void Awake()
    {
        dataBase = GameObject.FindGameObjectWithTag("Prefabs").GetComponent<Prefabs>();
        playerData = GameObject.FindGameObjectWithTag("PlayerData").GetComponent<PlayerData>();
    }



    public void FillContent()
    {
        FillBuilds(playerData.builds);
        FillUnits(playerData.units);
    }


    private void FillBuilds(Dictionary<int, int> builds)
    {
        foreach (KeyValuePair<int, int> item in builds)
        {
            BuildData buildData = dataBase.GetBuildData(item.Key);
            if (buildData != null)
            {
                AddBuild(buildData, item.Value);
                elementsCount++;
            }
        }
    }


    private void FillUnits(Dictionary<int, int> units)
    {
        foreach (KeyValuePair<int, int> item in units)
        {
            UnitData unitData = dataBase.GetUnitData(item.Key);
            if (unitData != null)
            {
                AddUnit(unitData, item.Value);
                elementsCount++;
            }
        }
    }


    private void AddBuild(BuildData buildData, int count)
    {
        var instance = GameObject.Instantiate(prefab.gameObject);
        instance.transform.SetParent(content, false);

        instance.transform.Find("Name").GetComponent<Text>().text = buildData.name;
        instance.transform.Find("Count").GetComponent<Text>().text = $"x{count}";
        instance.GetComponent<Image>().sprite = buildData.previewImage;

        instance.GetComponent<LandableUnit>().enabled = false;
        instance.tag = "LandableBuild";
        LandableBuild instanceClass = instance.GetComponent<LandableBuild>();
        instanceClass.buttonId = elementsCount;
        instanceClass.previewId = buildData.id;

        instanceClass.health = $"{buildData.maxHealth}";

        instanceClass.count = count;

        
    }


    private void AddUnit(UnitData unitData, int count)
    {
        var instance = GameObject.Instantiate(prefab.gameObject);
        instance.transform.SetParent(content, false);

        instance.transform.Find("Name").GetComponent<Text>().text = unitData.name;
        instance.transform.Find("Count").GetComponent<Text>().text = $"x{count}";
        instance.GetComponent<Image>().sprite = unitData.previewImage;

        instance.GetComponent<LandableBuild>().enabled = false;
        instance.tag = "LandableUnit";
        LandableUnit instanceClass = instance.GetComponent<LandableUnit>();
        instanceClass.buttonId = elementsCount;
        instanceClass.previewId = unitData.id;

        instanceClass.health = $"{unitData.maxHealth}"; instanceClass.damage = $"{unitData.damage}"; instanceClass.distance = $"{unitData.distance}"; instanceClass.mobility = $"{unitData.mobility}";

        instanceClass.count = count;

    }
}
