using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public Dictionary<int, int> units = new Dictionary<int, int>();
    private Prefabs dataBase;

    private void Awake()
    {
        EventMaster.current.UnitAddedToPlayer += AddUnit;
        EventMaster.current.UnitDeletedFromPlayer += DeleteUnit;
        EventMaster.current.UnitStatusChanged += ChangeUnitStatus;

        dataBase = GameObject.FindGameObjectWithTag("Prefabs").GetComponent<Prefabs>();
    }

    private void Start()
    {
        units.Add(0, 2);
        units.Add(1, 3);
        units.Add(2, 1);
        units.Add(3, 2);

    }

    private void AddUnit(int unitId, int count)
    {
        units.Add(unitId, count);
    }

    private void DeleteUnit(int unitId, int count)
    {
        if (count >= units[unitId]) units.Remove(unitId);
        else units[unitId] -= count;
    }

    private void ChangeUnitStatus(int unitId, bool status) 
    {

    }


}
