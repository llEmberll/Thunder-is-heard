using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class UnitTable : MonoBehaviour
{
    private GridTable gridTable;
    public List<GameObject> enemies;
    public string enemyUnitTag = "EnemyUnit";
    public List<GameObject> allies;
    public string friendlyUnitTag = "FriendlyUnit";
    public List<GameObject> enemyBuilds;
    public string enemyBuildTag = "EnemyBuild";
    public List<GameObject> friendlyBuilds;
    public string friendlyBuildTag = "FriendlyBuild";
    public List<GameObject> indestructibles;
    public string indestructibleTag = "Indestructibles";

    public Dictionary<GameObject, AttackersData> attackersInfo;

    private Dictionary<string, ElementCollection> collections;

    private class ElementCollection
    {
        public List<GameObject> elementCollection;

        public ElementCollection(List<GameObject> ElementCollection)
        {
            elementCollection = ElementCollection;
        }
    }


    private void Awake()
    {
        attackersInfo = new Dictionary<GameObject, AttackersData>();

        collections = new Dictionary<string, ElementCollection>
        {
            { enemyUnitTag, new ElementCollection(enemies) },
            { enemyBuildTag, new ElementCollection(enemyBuilds) },
            { friendlyUnitTag, new ElementCollection(allies) },
            { friendlyBuildTag, new ElementCollection(friendlyBuilds) },
            { indestructibleTag, new ElementCollection(indestructibles) }
        };

        gridTable = GameObject.FindWithTag("GridTable").GetComponent<GridTable>();
        EventMaster.current.AddedObjectToScene += AddObject;
        EventMaster.current.ObjectDestroyed += RemoveObject;
        EventMaster.current.CompleteUnitMove += UnitChangePose;
    }

    private void UnitChangePose(GameObject unit, int unitId, Vector3[] unitPoses)
    {
        RemoveObject(unit, unitPoses);
        AddObject(unit, unitPoses);
    }

    private void AddObject(GameObject obj, Vector3[] poses)
    {
        List<GameObject> objList = collections[obj.tag].elementCollection;
        if (!objList.Contains(obj))
        {
            objList.Add(obj);

            UpdateAttackers(obj, poses, GetEnemyUnitTagByTag(obj.tag));
            UpdateTargets(obj);
        }
    }


    private void RemoveObject(GameObject obj, Vector3[] poses)
    {
        attackersInfo.Remove(obj);
        collections[obj.tag].elementCollection.Remove(obj);
        RemoveUnitAttackers(obj);
        

    }


    private void RemoveUnitAttackers(GameObject unit) //Очистка списков, где удаленный элемент был атакующим
    {
        if (!unit.tag.Contains("Unit")) return;

        foreach (KeyValuePair<GameObject, AttackersData> record in attackersInfo)
        {
            record.Value.possibleTargetAttackers.Remove(unit);
        }
    }


    private void UpdateAttackers(GameObject element, Vector3[] poses, string tag) //Кто может атаковать эту цель
    {
        List<GameObject> possibleAttackers = collections[tag].elementCollection;

        AttackersData attackersData = new AttackersData();
        attackersData.possibleTargetAttackers = new List<GameObject>();

        foreach (GameObject obj in possibleAttackers)
        {
            if (obj.transform.position != element.transform.position)
            {
                Unit attacker = obj.GetComponent<Unit>();
                Cell[] attackCells = gridTable.getRange(attacker.transform.position, attacker.distance, true);

                if (ElementIntersection(attackCells, poses))
                {
                    attackersData.AddAttacker(obj);

                }
            }
        }
        attackersInfo.Add(element, attackersData);
    }

    private bool ElementIntersection(Cell[] cells, Vector3[] poses) 
    { 
        foreach (Vector3 pose in poses)
        {
            if (cells.Contains(gridTable.getCellInfoByPose(pose)))
            {
                return true;
            }
        }
        return false;
    }


    public string[] GetEnemyTagsByTag(string objectTag)
    {
        if (objectTag.Contains("Friendly"))
        {
            return new string[] { enemyUnitTag, enemyBuildTag };
        }
        return new string[] { friendlyUnitTag, friendlyBuildTag };
    }

    public string GetEnemyUnitTagByTag(string objectTag)
    {
        if (objectTag.Contains("Friendly"))
        {
            return enemyUnitTag;
        }
        return friendlyUnitTag;
    }

    public bool IsThisEnemy(string tag, GameObject target)
    {
        if (tag.Contains("Friendly"))
        {
            if (target.tag.Contains("Enemy"))
            {
                return true;
            }
        }
        else
        {
            if (target.tag.Contains("Friendly"))
            {
                return true;
            }
        }
        return false;
    }


    private void UpdateTargets(GameObject element) //Кого может атаковать эта цель
    {
        if (!element.tag.Contains("Unit")) return;

        Unit attacker = element.GetComponent<Unit>();
        Cell[] attackCells = gridTable.getRange(attacker.transform.position, attacker.distance, true);

        string[] enemyTags = GetEnemyTagsByTag(element.tag);

        for (int index = 0; index < attackCells.Length; index++)
        {
            Cell currentCell = attackCells[index];

            if (currentCell != null && currentCell.occypier != null && currentCell.occypier != element)
            {
                GameObject occypier = currentCell.occypier;

                if (enemyTags.Contains(occypier.tag))
                {
                    if (attackersInfo.ContainsKey(occypier))
                    {
                        attackersInfo[occypier].AddAttacker(element);
                    }
                    else
                    {
                        AddObject(occypier, occypier.GetComponent<Destructible>().occypiedPoses);
                    }
                }
            }
        }
    }
    
}

public class AttackersData
{
    public List<GameObject> possibleTargetAttackers;


    public void AddAttacker(GameObject attacker)
    {
        if (!possibleTargetAttackers.Contains(attacker))
        {
            possibleTargetAttackers.Add(attacker);
        }
        
    }

    public void DeleteAttacker(GameObject attacker)
    {
        if (possibleTargetAttackers.Contains(attacker))
        {
            possibleTargetAttackers.Remove(attacker);
        }
    }
}