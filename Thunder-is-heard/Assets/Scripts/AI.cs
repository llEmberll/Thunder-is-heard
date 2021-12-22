using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI : MonoBehaviour
{
    private int mood;
    private GridTable gridTable;
    private UnitTable unitTable;


    private class TargetData
    {
        public Unit target;
        public int ultimateDamage = 0;
        public Unit[] possibleTargetAttackers;
        public float minimalDistance = 0;
        public int attackersIndex = 0;

        public void UpdateMinimalDistance(Unit newAttacker)
        {
            float newDistance = Vector3.Distance(newAttacker.transform.position, target.transform.position);
            if (newDistance > minimalDistance)
            {
                minimalDistance = newDistance;
            }
        }

        public void AddPossibleAttacker(Unit attacker)
        {
            possibleTargetAttackers[attackersIndex] = attacker;

            UpdateMinimalDistance(attacker);

            attackersIndex++;
        }
    }

    private void Start()
    {
        gridTable = GameObject.FindWithTag("GridTable").GetComponent<GridTable>();
        unitTable = GameObject.FindWithTag("UnitTable").GetComponent<UnitTable>();

    }

    public void Solution()
    {
        CollectData();
    }


    private void CollectData()
    {
        List<TargetData> possibleTargetsForAttack = new List<TargetData>();

        for (int index = 0; index < unitTable.allies.Count; index++)
        {
            TargetData targetData = new TargetData();
            Unit possibleTarget = unitTable.allies[index].GetComponent<Unit>();

            targetData.target = possibleTarget;
            targetData.possibleTargetAttackers = new Unit[unitTable.enemies.Count];

            bool existTarget = false;

            foreach (GameObject element in unitTable.enemies)
            {
                Unit attacker = element.GetComponent<Unit>();

                Cell[] attackCells = gridTable.getRange(attacker.transform.position, attacker.distance, true);
                if (attackCells.Contains(gridTable.getCellInfoByPose(possibleTarget.transform.position)))
                {
                    targetData.AddPossibleAttacker(attacker);
                    targetData.ultimateDamage += attacker.damage;
                    existTarget = true;
                }
            }

            if (existTarget)
            {
                possibleTargetsForAttack.Add(targetData);
            }
        }
        

        if (possibleTargetsForAttack.Count > 0)
        {
            ChooseUnit(possibleTargetsForAttack);
        }

        else
        {
            BuildRoute();
        }

    }

    private void Attack()
    {

    }

    private void ChooseUnit(List<TargetData> units)
    {

    }

    private void BuildRoute()
    {

    }

    private void PushToEnd(Unit[] mass, Unit element)
    {
        for (int index = 0; index < mass.Length; index++)
        {
            if (mass[index] == null)
            {
                mass[index] = element;
                break;
            }
        }
    }

}
public class AISolution
{
    public int Action;

    public Unit[] activeUnits;

    public Cell[] route;

    public Cell target;
}

