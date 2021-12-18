using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UnitContentMaker : MonoBehaviour
{
    [SerializeField] private RectTransform prefab;
    [SerializeField] private RectTransform content;

    [SerializeField] private Text health, damage, distance, mobility;

    private int elementsCount = 0;

    private Prefabs dataBase;
    private PlayerData playerData;


    private void Awake()
    {
        EventMaster.current.FightIsStarted += PrepareOver;

        dataBase = GameObject.FindGameObjectWithTag("Prefabs").GetComponent<Prefabs>();
        playerData = GameObject.FindGameObjectWithTag("PlayerData").GetComponent<PlayerData>();
    }

    private void Start()
    {
        FillContent(playerData.units);
    }

    private void FillContent(Dictionary<int, int> elementData)
    {
        foreach (KeyValuePair<int, int> item in elementData)
        {
            UnitData unitData = dataBase.GetUnitData(item.Key);
            if (unitData != null)
            {
                AddElement(unitData, item.Value);
            }
        }
    }

    private void AddElement(UnitData unitData, int count)
    {
        var instance = GameObject.Instantiate(prefab.gameObject);
        instance.transform.SetParent(content, false);

        instance.transform.Find("Name").GetComponent<Text>().text = unitData.name;
        instance.transform.Find("Count").GetComponent<Text>().text = $"x{count}";
        instance.tag = "LandableUnit";
        LandableUnit instanceClass = instance.GetComponent<LandableUnit>();
        instanceClass.buttonId = elementsCount;
        instanceClass.previewId = unitData.id;

        instanceClass.Health = $"{unitData.maxHealth}"; instanceClass.Damage = $"{unitData.damage}"; instanceClass.Distance = $"{unitData.distance}"; instanceClass.Mobility = $"{unitData.mobility}";

        instanceClass.UIHealth = health;
        instanceClass.UIDamage = damage;
        instanceClass.UIDistance = distance;
        instanceClass.UIMobility = mobility;
        instanceClass.count = count;

        elementsCount++;
    }

    private void PrepareOver()
    {
        EventMaster.current.FightIsStarted -= PrepareOver;

        Destroy(this.gameObject);
    }


    
}
