using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager: MonoBehaviour
{
    private GridTable gridTable;
    public UnitTable currentPosition;
    public int currentId = 0;

    public float currentPositionEvaluation = 0;

    private void Awake()
    {
        gridTable = GameObject.FindWithTag("GridTable").GetComponent<GridTable>();
        currentPosition = new UnitTable(gridTable, new List<BattleSlot>(), new List<BattleSlot>(), new List<BattleSlot>(), new List<BattleSlot>(), new List<BattleSlot>(), new Dictionary<int, AttackersData>(), true);
        
        EventMaster.current.AddedObjectToScene += NewSceneObject;
        EventMaster.current.ObjectDestroyed += RemoveObjectFromBattle;
        EventMaster.current.CompleteUnitMove += UnitOnNewPose;
        EventMaster.current.ObjectChangedHealth += ObjectChangeHealth;
        EventMaster.current.ChangeTurn += ChangeTurn;
    }


    public UnitTable ClonePosition(UnitTable position)
    {
        return new UnitTable(gridTable, CopySlotsToNew(position.enemies), CopySlotsToNew(position.allies), CopySlotsToNew(position.enemyBuilds), CopySlotsToNew(position.friendlyBuilds), CopySlotsToNew(position.indestructibles), CopyAttackersDataToNew(position.attackersInfo), position.playerTurn);
    }


    public void ChangeTurn(bool isPlayerTurn)
    {
        currentPosition.playerTurn = isPlayerTurn;
    }

    private void ObjectChangeHealth(GameObject obj, int newHealth)
    {
        currentPosition.ObjectHealthChanged(obj.GetComponent<Destructible>().id, newHealth);
        currentPositionEvaluation = currentPosition.positionEvaluation;
    }


    private void UnitOnNewPose(GameObject unit, int unitId, Vector3[] unitPoses)
    {
        currentPosition.UnitChangePose(unit, unitId, unitPoses);
        currentPositionEvaluation = currentPosition.positionEvaluation;
    }

    public void NewSceneObject(GameObject obj, Vector3[] poses)
    {
        if (obj.tag == "Indestructibles")
        {
            obj.GetComponent<Indestructible>().id = currentId;
        }
        else
        {
            obj.GetComponent<Destructible>().id = currentId;
        }

        currentId++;

        currentPosition.AddObject(currentPosition.CreateNewSlot(obj), poses);
        currentPositionEvaluation = currentPosition.positionEvaluation;
    }


    public void RemoveObjectFromBattle(GameObject obj, Vector3[] poses)
    {
        currentPosition.RemoveObject(obj.GetComponent<Destructible>().id, obj.tag, poses);
        currentPositionEvaluation = currentPosition.positionEvaluation;
        CheckForOverFight();
    }


    public void CheckForOverFight()
    {
        if (currentPosition.allies.Count < 1)
        {
            EventMaster.current.AllPartyUnitDown(false);
            return;
        }
        if (currentPosition.enemies.Count < 1)
        {
            EventMaster.current.AllPartyUnitDown(true);
            return;
        }
    }


    public Dictionary<TurnData, UnitTable> GetAllPositionsFromPosition(UnitTable startPosition)
    {
        Dictionary<TurnData, UnitTable> positions = new Dictionary<TurnData, UnitTable>();

        positions = GetAttackTurnPositions(startPosition.playerTurn, startPosition, positions);

        positions = GetMoveTurnPositions(startPosition.playerTurn, startPosition, positions);
        return positions;
    }


    private Dictionary<TurnData, UnitTable> GetAttackTurnPositions(bool isPlayerTurn, UnitTable currentBattlePosition, Dictionary<TurnData, UnitTable> positions)
    {
        string targetsTag = "Friendly";
        if (isPlayerTurn) targetsTag = "Enemy";

        Dictionary<int, AttackersData> attackersData = CopyAttackersDataToNew(currentBattlePosition.attackersInfo);

        foreach (KeyValuePair<int, AttackersData> record in attackersData)
        {
            if (record.Value.obj.tag.Contains(targetsTag) && record.Value.possibleAttackers.Count() > 0)
            {
                AttackersData currentData = record.Value;
                TurnData turn = new TurnData(1);

                UnitTable newBattlePosition = ClonePosition(currentBattlePosition);

                newBattlePosition.playerTurn = !currentBattlePosition.playerTurn;

                turn.target = record.Value.obj;

                if (currentData.inputDamage >= currentData.obj.health)
                {
                    newBattlePosition.RemoveObject(record.Value.obj.id, record.Value.obj.tag, record.Value.obj.occypiedPoses);
                }
                else
                {
                    newBattlePosition.ObjectHealthChanged(record.Key, currentData.obj.health - currentData.inputDamage);
                }
                positions.Add(turn, newBattlePosition);
                
            }
        }
        return positions;
    }


    private Dictionary<TurnData, UnitTable> GetMoveTurnPositions(bool isPlayerTurn, UnitTable currentBattlePosition, Dictionary<TurnData, UnitTable> positions)
    {
        string targetsTag = "EnemyUnit";
        if (isPlayerTurn) targetsTag = "FriendlyUnit";

        List<BattleSlot> targets = CopySlotsToNew(currentBattlePosition.collections[targetsTag]);

        foreach (BattleSlot target in targets)
        {
            Dictionary<Cell, int> realMoveCells = gridTable.GetRealMoveCells(target.center, target.mobility);

            foreach (KeyValuePair<Cell, int> item in realMoveCells)
            {
                if (item.Key != null && item.Key.cellPose != target.center && item.Key.occypier == null)
                {
                    TurnData turn = new TurnData(2, target, item.Key);

                    UnitTable newBattlePosition = ClonePosition(currentBattlePosition);

                    newBattlePosition.RemoveObject(target.id, target.tag, target.occypiedPoses);

                    BattleSlot unitOnNewPosition = new BattleSlot(target.tag, target.id, target.type, item.Key.cellPose, new Vector3[] {item.Key.cellPose }, target.health, target.damage, target.distance, target.mobility);

                    newBattlePosition.AddObject(unitOnNewPosition, unitOnNewPosition.occypiedPoses);

                    newBattlePosition.playerTurn = !currentBattlePosition.playerTurn;

                    positions.Add(turn, newBattlePosition);
                }
            }
        }
        return positions;
    }



    private Dictionary<int, AttackersData> CopyAttackersDataToNew(Dictionary<int, AttackersData> referenceAttackersInfo)
    {
        Dictionary<int, AttackersData> newAttackersInfo = new Dictionary<int, AttackersData>();
        foreach (KeyValuePair<int, AttackersData> item in referenceAttackersInfo)
        {
            AttackersData newAttackersValue = item.Value.Clone();

            newAttackersInfo.Add(item.Key, newAttackersValue);
        }
        return newAttackersInfo;
    }


    private List<BattleSlot> CopySlotsToNew(List<BattleSlot> slots)
    {
        List<BattleSlot> newSlots = new List<BattleSlot>();
        foreach (BattleSlot slot in slots)
        {
            newSlots.Add(slot.Clone());
        }
        return newSlots;
    }
}


public class BattleSlot
{
    public int id, type, health;
    public int damage = 0;
    public int distance = 0;
    public int mobility = 0;
    public int side = -1;
    public Vector3 center;
    public Vector3[] occypiedPoses;
    public string tag;

    public BattleSlot(string Tag, int Id, int Type, Vector3 Center, Vector3[] poses, int Health, int Damage = 0, int Distance = 0, int Mobility = 0)
    {
        tag = Tag; id = Id; type = Type; health = Health; damage = Damage; distance = Distance; mobility = Mobility; center = Center; occypiedPoses = poses;
        if (tag.Contains("Friendly")) side = 1;
    }

    public BattleSlot Clone()
    {
        return new BattleSlot(tag, id, type, center, occypiedPoses, health, damage, distance, mobility);
    }
}


public class UnitTable
{
    public GridTable gridTable;
    public List<BattleSlot> enemies;
    public string enemyUnitTag = "EnemyUnit";
    public List<BattleSlot> allies;
    public string friendlyUnitTag = "FriendlyUnit";
    public List<BattleSlot> enemyBuilds;
    public string enemyBuildTag = "EnemyBuild";
    public List<BattleSlot> friendlyBuilds;
    public string friendlyBuildTag = "FriendlyBuild";
    public List<BattleSlot> indestructibles;
    public string indestructibleTag = "Indestructibles";

    public Dictionary<int, AttackersData> attackersInfo;

    public Dictionary<string, List<BattleSlot>> collections;

    public float positionEvaluation = 0; // "-" Позиция в пользу ИИ
    public float nearestDistanceToEnemy = 9999;

    public bool playerTurn;


    public UnitTable(GridTable cellData, List<BattleSlot> Enemies, List<BattleSlot> Allies, List<BattleSlot> EnemyBuilds, List<BattleSlot> FriendlyBuilds, List<BattleSlot> Indestructibles, Dictionary<int, AttackersData> AttackersInfo, bool PlayerTurn)
    {
        gridTable = cellData; enemies = Enemies; allies = Allies; enemyBuilds = EnemyBuilds; friendlyBuilds = FriendlyBuilds; indestructibles = Indestructibles; attackersInfo = AttackersInfo; playerTurn = PlayerTurn;
        collections = new Dictionary<string, List<BattleSlot>>
        {
            { enemyUnitTag, (enemies) },
            { enemyBuildTag, (enemyBuilds) },
            { friendlyUnitTag, (allies) },
            { friendlyBuildTag, (friendlyBuilds) },
            { indestructibleTag, (indestructibles) }
        };
    }

    public void ObjectHealthChanged(int objId, int newHealth)
    {
        attackersInfo[objId].obj.health = newHealth;
        UpdateEvaluationFromUnits();
        UpdatePositionEvaluation();
    }


    public float SetNearestEnemyUnitDistanceByUnit(AttackersData unitRecord, string enemyTag)
    {
        float nearestDistance = 9999;

        foreach (BattleSlot enemy in collections[enemyTag])
        {
            float newDistance = Vector3.Distance(enemy.center, unitRecord.obj.center);
            if (newDistance < nearestDistance)
            {

                unitRecord.nearestUnit = enemy;
                nearestDistance = newDistance;
            }
        }
        unitRecord.distanceToNearestEnemyUnit = nearestDistance;
        return nearestDistance;
    }



    public void UnitChangePose(GameObject unit, int unitId, Vector3[] unitPoses)
    {
        RemoveObject(unitId, unit.tag, unitPoses);
        AddObject(CreateNewSlot(unit), unitPoses);
    }

    public void AddObject(BattleSlot obj, Vector3[] poses)
    {
        List<BattleSlot> objList = collections[obj.tag];

        if (!IsExistIdOnList(objList, obj.id))
        {
            objList.Add(obj);

            if (obj.tag != "Indestructibles")
            {

                UpdateAttackersData(obj, poses, GetEnemyUnitTagByTag(obj.tag));
                UpdateEvaluationFromUnits();
                UpdatePositionEvaluation();

                if (obj.tag.Contains("Unit"))
                {
                    float unitDistanceToEnemy = SetNearestEnemyUnitDistanceByUnit(attackersInfo[obj.id], GetEnemyUnitTagByTag(obj.tag));
                    if (unitDistanceToEnemy > nearestDistanceToEnemy) nearestDistanceToEnemy = unitDistanceToEnemy;
                    GlobalUpdateNearestUnitToEnemyByTag(GetEnemyUnitTagByTag(obj.tag));
                }
            }
            
        }
    }


    public void GlobalUpdateNearestUnitToEnemyByTag(string unitsTag)
    {
        List<BattleSlot> units = collections[unitsTag];
        foreach (BattleSlot unit in units)
        {
            SetNearestEnemyUnitDistanceByUnit(attackersInfo[unit.id], GetEnemyUnitTagByTag(unit.tag));
        }
    }

    public void RemoveObject(int objId, string objTag, Vector3[] poses)
    {
        AttackersData attackersData = attackersInfo[objId];

        RemoveUnitAttackers(objId, objTag);
        
        RemoveUnitTargets(objId);

        attackersInfo.Remove(objId);
        RemoveObjectFromCollection(objId, objTag);
        UpdatePositionEvaluation();

        if ((attackersData.distanceToNearestEnemyUnit) == nearestDistanceToEnemy)
        {
            UpdateNearestUnitToEnemy(GetEnemyUnitTagByTag(objTag));
        }
    }



    public void UpdateNearestUnitToEnemy(string enemyUnitTag)
    {
        List<BattleSlot> enemyUnits = collections[enemyUnitTag];
        foreach (BattleSlot enemy in enemyUnits)
        {
            if (attackersInfo[enemy.id].distanceToNearestEnemyUnit < nearestDistanceToEnemy) nearestDistanceToEnemy = attackersInfo[enemy.id].distanceToNearestEnemyUnit;
        }
    }


    private bool IsExistIdOnList(List<BattleSlot> list, int id)
    {
        foreach (BattleSlot slot in list)
        {
            if (slot.id == id) return true;
        }
        return false;
    }

    private void RemoveObjectFromCollection(int id, string objTag)
    {
        List<BattleSlot> collection = collections[objTag];
        foreach (BattleSlot slot in collection)
        {
            if (slot.id == id)
            {
                collection.Remove(slot);
                break;
            }
        }
    }


    private void RemoveUnitAttackers(int objId, string objTag) //Очистка списков, где удаленный элемент был атакующим
    {
        if (!objTag.Contains("Unit")) return;
        foreach (KeyValuePair<BattleSlot, int> item in attackersInfo[objId].possibleTargets)
        {
            attackersInfo[item.Key.id].DeleteAttakerById(objId);
        }
    }


    private void RemoveUnitTargets(int objId) //Очистка списков, где удаленный элемент был целью
    {
        foreach (KeyValuePair<BattleSlot, int> item in attackersInfo[objId].possibleAttackers)
        {
            attackersInfo[item.Key.id].DeleteTargetById(objId);
        }
    }


    private BattleSlot FindSlotInAttackersCollectionById(int id, Dictionary<BattleSlot, int> collection)
    {
        foreach (KeyValuePair<BattleSlot, int> item in collection)
        {
            if (item.Key.id == id) return item.Key;
        }
        return null;
    }


    private void UpdateAttackersData(BattleSlot element, Vector3[] poses, string tag)
    {
        List<BattleSlot> possibleAttackers = collections[tag];
        

        AttackersData attackersData = new AttackersData((element));

        UpdateAttackers(element, poses, possibleAttackers, attackersData);

        attackersInfo.Add(element.id, attackersData);

        UpdateTargets(element, attackersData);
    }


    private void UpdateAttackers(BattleSlot element, Vector3[] poses, List<BattleSlot> possibleAttackers, AttackersData attackersData) //Кто может атаковать эту цель
    {
        foreach (BattleSlot attacker in possibleAttackers)
        {
            if (attacker.occypiedPoses != element.occypiedPoses)
            {
                Cell[] attackCells = gridTable.GetRange(attacker.center, attacker.distance, true);

                if (ElementIntersection(attackCells, poses))
                {
                    attackersData.AddAttacker(attacker);
                    attackersInfo[attacker.id].AddTarget((element), attacker.damage);
                }

            }
        }
    }


    public BattleSlot CreateNewSlot(GameObject obj)
    {
        
        if (obj.tag.Contains("Unit"))
        {
            Unit unit = obj.GetComponent<Unit>();
            return new BattleSlot(unit.tag, unit.id, unit.type, unit.center, unit.occypiedPoses, unit.health, unit.damage, unit.distance, unit.mobility);
        }
        else if (obj.tag.Contains("Build"))
        {
            Build build = obj.GetComponent<Build>();
            return new BattleSlot(build.tag, build.id, build.type, build.center, build.occypiedPoses, build.health);
        }
        else
        {
            Indestructible indestructible = obj.GetComponent<Indestructible>();
            return new BattleSlot(indestructible.tag, indestructible.id, indestructible.type, indestructible.center, indestructible.occypiedPoses, indestructible.health);
        }
    }


    private void UpdateTargets(BattleSlot attacker, AttackersData attackersData) //Кого может атаковать эта цель
    {
        if (!attacker.tag.Contains("Unit")) return;
        Cell[] attackCells = gridTable.GetRange(attacker.center, attacker.distance, true);

        string[] enemyTags = GetEnemyTagsByTag(attacker.tag);

        for (int index = 0; index < attackCells.Length; index++)
        {
            Cell currentCell = attackCells[index];

            if (currentCell != null && currentCell.cellPose != attacker.center && currentCell.occypier != null)
            {
                GameObject occypier = currentCell.occypier;

                if (enemyTags.Contains(occypier.tag))
                {
                    Destructible occypierClass = occypier.GetComponent<Destructible>();

                    if (attackersInfo.ContainsKey(occypierClass.id))
                    {
                        attackersInfo[occypierClass.id].AddAttacker(attacker);

                        attackersData.AddTarget(attackersInfo[occypierClass.id].obj, attacker.damage);
                    }
                    else
                    {
                        BattleSlot occypierSlot = CreateNewSlot(occypier);

                        AddObject(occypierSlot, occypierClass.occypiedPoses);

                        attackersData.AddTarget(occypierSlot, attacker.damage);
                    }
                }
            }
        }
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


    private void UpdateEvaluationFromUnits()
    {
        foreach (KeyValuePair<int, AttackersData> record in attackersInfo)
        {
            record.Value.UpdateEvaluation();
        }
    }


    private void UpdatePositionEvaluation()
    {
        positionEvaluation = 0;

        foreach (KeyValuePair<int, AttackersData> record in attackersInfo)
        {

            positionEvaluation += record.Value.evaluation;


        }
    }
}


    public class AttackersData
{
    public BattleSlot obj;
    public int side = -1;
    public Dictionary<BattleSlot, int> possibleAttackers = new Dictionary<BattleSlot, int>();
    public Dictionary<BattleSlot, int> possibleTargets = new Dictionary<BattleSlot, int>();
    public Dictionary<BattleSlot, int> possibleUnitTargets = new Dictionary<BattleSlot, int>();
    public int inputDamage = 0;
    public int outputDamage = 0;
    public float evaluation = 0;

    public float distanceToNearestEnemyUnit = 9999;
    public BattleSlot nearestUnit;

    public AttackersData(BattleSlot element)
    {
        obj = element;
        side = element.side;
        UpdateEvaluation();
    } 


    public AttackersData Clone()
    {
        AttackersData Clone = new AttackersData(obj.Clone());
        Clone.side = side; Clone.inputDamage = inputDamage; Clone.outputDamage = outputDamage; Clone.evaluation = evaluation; Clone.distanceToNearestEnemyUnit = distanceToNearestEnemyUnit; Clone.nearestUnit = nearestUnit;
        Clone.possibleAttackers = CopyAttackersDataCollectionToNew(possibleAttackers);
        Clone.possibleTargets = CopyAttackersDataCollectionToNew(possibleTargets);
        Clone.possibleUnitTargets = CopyAttackersDataCollectionToNew(possibleUnitTargets);

        return Clone;
    }


    public Dictionary<BattleSlot, int> CopyAttackersDataCollectionToNew(Dictionary<BattleSlot, int> referenceCollection)
    {
        Dictionary<BattleSlot, int> newCollection = new Dictionary<BattleSlot, int>();
        foreach (KeyValuePair<BattleSlot, int> item in referenceCollection)
        {
            BattleSlot newBattleSlot = item.Key.Clone();
            newCollection.Add(newBattleSlot, item.Value);
        }
        return newCollection;
    }


    public int GetDamageForUnits()
    {
        int maxDamage = 0;
        foreach (KeyValuePair<BattleSlot, int> item in possibleTargets)
        {
            int currentDamage = 0;
            if (item.Key.type == 1)
            {
                if (item.Key.health <= item.Value)
                {
                    currentDamage = item.Key.health;
                    if (currentDamage >= obj.damage) return obj.damage;
                    if (currentDamage > maxDamage) maxDamage = currentDamage;
                }
                else
                {
                    currentDamage = item.Value;
                    if (currentDamage >= obj.damage) return obj.damage;
                    if (currentDamage > maxDamage) maxDamage = currentDamage; 
                }
            }
        }
        return maxDamage;
    }

    public void AddAttacker(BattleSlot attacker)
    {
        if (!IsExistInAttackerCollection(possibleAttackers, attacker.id))
        {


            int attackerDamage = attacker.damage;


            if (attackerDamage >= obj.health) attackerDamage = obj.health;
            possibleAttackers.Add(attacker, attackerDamage);
            inputDamage += attackerDamage;

            UpdateEvaluation();
        }
    }


    private bool IsExistInAttackerCollection(Dictionary<BattleSlot, int> collection, int objId)
    {
        foreach (KeyValuePair<BattleSlot, int> item in collection)
        {
            if (item.Key.id == objId) return true;
        }
        return false;
    }


    public void DeleteAttacker(BattleSlot attacker)
    {
        if (possibleAttackers.ContainsKey(attacker))
        {
            inputDamage -= possibleAttackers[attacker];
            possibleAttackers.Remove(attacker);
            UpdateEvaluation();
        }
    }


    public void DeleteAttakerById(int attackerId)
    {
        foreach (KeyValuePair<BattleSlot, int> item in possibleAttackers)
        {
            if (item.Key.id == attackerId)
            {
                inputDamage -= possibleAttackers[item.Key];
                possibleAttackers.Remove(item.Key);
                UpdateEvaluation();
                break;
            }
        }
    }


    public void DeleteTargetById(int targetId)
    {
        foreach (KeyValuePair<BattleSlot, int> item in possibleTargets)
        {
            if (item.Key.id == targetId)
            {
                outputDamage -= possibleTargets[item.Key];
                possibleTargets.Remove(item.Key);

                UpdateEvaluation();

                if (possibleUnitTargets.ContainsKey(item.Key))
                {
                    possibleUnitTargets.Remove(item.Key);
                }

                break;
            }
        }
    }


    public void AddTarget(BattleSlot target, int damageToTarget)
    {
        if (!IsExistInAttackerCollection(possibleTargets, target.id))
        {
            int targetHealth = target.health;
            if (damageToTarget >= targetHealth) damageToTarget = targetHealth;
            outputDamage += damageToTarget;
            possibleTargets.Add(target, damageToTarget);
            UpdateEvaluation();

            if (target.tag.Contains("Unit"))
            {
                possibleUnitTargets.Add(target, damageToTarget);
            }
        }

    }

    public void DeleteTarget(BattleSlot target)
    {
        if (possibleTargets.ContainsKey(target))
        {
            outputDamage -= possibleTargets[target];
            possibleTargets.Remove(target);
            
            UpdateEvaluation();

            if (possibleUnitTargets.ContainsKey(target))
            {
                possibleUnitTargets.Remove(target);
            }
        }
    }

    public void UpdateEvaluation()
    {
        if (obj.type == 1)
        {
            

            if (inputDamage >= obj.health)
            {
                inputDamage = obj.health;
                
            }
            float powerByStats = ((obj.health - (inputDamage) + (obj.damage + GetDamageForUnits()) + (obj.mobility / 2)) + (4 * obj.distance));

            evaluation = ((powerByStats * side));
        }
    }
}
