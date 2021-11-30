using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitTable : MonoBehaviour
{
    private Unit[] enemies;
    private Unit[] allies;

    private void Start()
    {
        EventMaster.current.UnitDies += UnitDie;
        updateUnitTables();
    }

    private void UnitDie(Unit unit)
    {
        Debug.Log("Event happens: UnitDies!");

        if (enemies.Length - 1 < 1)
        {
            EventMaster.current.FightFinishing(true);
        }

        else if (allies.Length - 1 < 1)
        {
            EventMaster.current.FightFinishing(false);
        }
        unitDown(unit);
    }

    public void updateUnitTables()
    {
        GameObject[] findedEnemies = GameObject.FindGameObjectsWithTag("EnemyUnit");
        GameObject[] findedAllies = GameObject.FindGameObjectsWithTag("FriendlyUnit");

            enemies = new Unit[findedEnemies.Length];
            allies = new Unit[findedAllies.Length];

            enemies = updateTable(findedEnemies, enemies);
            allies = updateTable(findedAllies, allies);
    }


    private Unit[] updateTable(GameObject[] finded, Unit[] collector, Unit ignoreUnit = null)
    {
        if (ignoreUnit == null)
        {
            for (int index = 0; index < finded.Length; index++)
            {
                collector[index] = finded[index].GetComponent<Unit>();
            }

            return collector;
        }

        for (int index = 0; index < finded.Length; index++)
        {
            Unit currentFindedUnit = finded[index].GetComponent<Unit>();
            if (currentFindedUnit != ignoreUnit)
            {
                collector[index] = finded[index].GetComponent<Unit>();
            } 
            
        }

        return collector;

    }

    public int getUnitCount(string tag)
    {
        if (tag == "EnemyUnit")
        {
            return enemies.Length;
        }

        else
        {
            return allies.Length;
        }
        
    }


    private void unitDown(Unit unit)
    {
        if (unit.tag == "EnemyUnit")
        {
            updateTable(GameObject.FindGameObjectsWithTag("EnemyUnit"), enemies, unit);
        }

        else
        {
            updateTable(GameObject.FindGameObjectsWithTag("FriendlyUnit"), allies, unit);
        }

    }
}


