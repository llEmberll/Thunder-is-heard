using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitTable : MonoBehaviour
{
    public List<Unit> enemies;
    public List<Unit> allies;
    public List<Build> enemyBuilds;
    public List<Build> friendlyBuilds;
    public List<Vector3> indestructibles;

    private void Awake()
    {
        EventMaster.current.AddedBuildToScene += AddBuild;
        EventMaster.current.AddedUnitToScene += AddUnit;
        EventMaster.current.UnitDies += UnitDie;
        EventMaster.current.BuildDestroed += BuildDestroyed;
    }

    private void AddBuild(Build build, bool enemy)
    {
        if (enemy)
        {
            enemyBuilds.Add(build); return;
        }
        friendlyBuilds.Add(build);
    }

    private void AddUnit(Unit unit, bool enemy)
    {
        if (enemy)
        {
            enemies.Add(unit); return;
        }
        allies.Add(unit);
    }

    private void RemoveBuild(Build build, bool enemy)
    {
        if (enemy)
        {
            enemyBuilds.Remove(build); return;
        }
        friendlyBuilds.Remove(build);
    }

    private void RemoveUnit(Unit unit, bool enemy)
    {
        if (enemy)
        {
            enemies.Remove(unit); return;
        }
        allies.Remove(unit);
    }

    private void UnitDie(Unit unit)
    {
        Debug.Log("Event happens: UnitDies!");

        if (enemies.Count - 1 < 1)
        {
            EventMaster.current.AllPartyUnitDown(true);
        }

        else if (allies.Count - 1 < 1)
        {
            EventMaster.current.AllPartyUnitDown(false);
        }

        RemoveUnit(unit, unit.CompareTag("EnemyUnit"));
    }

    private void BuildDestroyed(Build build, Vector3[] occypyPoses)
    {
        Debug.Log("Event happens: BuildDestroyed!");

        if (enemies.Count - 1 < 1)
        {
        }

        else if (allies.Count - 1 < 1)
        {
        }

        RemoveBuild(build, build.CompareTag("EnemyBuild"));
    }

}

public class AttackersData
{
    public List<Unit> possibleTargetAttackers;


    public void AddAttacker(Unit attacker)
    {
        if (!possibleTargetAttackers.Contains(attacker))
        {
            possibleTargetAttackers.Add(attacker);
        }
        
    }

    public void DeleteAttacker(Unit attacker)
    {
        if (possibleTargetAttackers.Contains(attacker))
        {
            possibleTargetAttackers.Remove(attacker);
        }
    }
}