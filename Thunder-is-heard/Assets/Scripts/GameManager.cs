using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] public Unit activeUnit;

    private GridTable gridTable;
    private UnitTable currentBattlePosition;

    private Cell[] landableCells;

    private Cell[] possibleMoveCells;
    private Cell[] moveRoute;

    private bool idle, existPreview;

    private bool fightIsOver = false;

    private int previewId;

    private int turnCounter;


    public bool playerTurn;

    private Dictionary<Cell, int> realMoveCells;

    private AI AI;

    // Start is called before the first frame update
    void Start()
    {
        gridTable = GameObject.FindWithTag("GridTable").GetComponent<GridTable>();
        currentBattlePosition = GameObject.FindWithTag("BattleManager").GetComponent<BattleManager>().currentPosition;
        AI = GameObject.FindWithTag("AI").GetComponent<AI>();

        EventMaster.current.AllPartyUnitDowned += AllUnitsDown;
        EventMaster.current.CompleteUnitMove += MoveComplete;
        EventMaster.current.ClickedOnCell += clickedOnCell;
        EventMaster.current.ObjectDestroyed += ObjectDestroyed;
        EventMaster.current.MouseOnCellEnter += MouseOnCellEnter;
        EventMaster.current.SelectedObject += MouseOnActiveUnit;
        EventMaster.current.CreatedPreview += CreatePreview;
        EventMaster.current.DeletedPreview += DeletePreview;

        activeUnit = null;
        idle = playerTurn = existPreview = false;
        turnCounter = 0;

        EventMaster.current.TurnChanging(playerTurn);
    }


    public void StartFight()
    {
        Debug.Log("Fight is started!");
        gridTable.turnOffCells();

        EventMaster.current.StartFight();

        idle = true;

        existPreview = false;
        nextTurn();
    }

    private void CreatePreview(int id)
    {
        existPreview = true;
        previewId = id;
    }

    private void DeletePreview()
    {
        existPreview = false;
    }

    private void MouseOnActiveUnit(Vector3 pose, Material material, bool render)
    {
        if (render && activeUnit != null && idle)
        {
            if (activeUnit.transform.position == pose)
            {
                ClearRoute();
            }
        }
    }

    private void MouseOnCellEnter(Cell cell, bool render)
    {
        if (activeUnit != null && idle)
        {
            if (render)
            {
                int countRouteCells = GetRouteLength(moveRoute);
                Cell lastRouteCell = getRouteLast(moveRoute);

                if (countRouteCells < 1)
                {
                    Cell[] firstStep = gridTable.GetRange(activeUnit.transform.position, 1, false);
                    if (canMove(cell, firstStep))
                    {
                        moveRoute[0] = cell;
                        Cell previousRouteCell = gridTable.getCellInfoByPose(activeUnit.transform.position);

                        countRouteCells = GetRouteLength(moveRoute);

                        EventMaster.current.SelectCellForRoute(cell, previousRouteCell, activeUnit.mobility == countRouteCells);
                        return;
                    }
                }
                else
                {
                    if (moveRoute.Contains(cell) && lastRouteCell != cell)
                    {
                        DeleteAllRouteAfterValue(cell);
                        return;
                    }

                    else
                    {

                        bool overRoute = (activeUnit.mobility > countRouteCells);

                        if (overRoute)
                        {
                            Cell[] nextStep = gridTable.GetRange(lastRouteCell.cellPose, 1, false);

                            if (canMove(cell, nextStep))
                            {
                                moveRoute[countRouteCells] = cell;

                                overRoute = activeUnit.mobility == GetRouteLength(moveRoute);
                                EventMaster.current.SelectCellForRoute(cell, moveRoute[countRouteCells - 1], overRoute);
                            }
                        }
                    }
                }
            }

            else 
            {
                ClearRoute();
            }
        }
    }

    private void ClearRoute()
    {
        if (moveRoute != null)
        {
            EventMaster.current.ClearingRoute();
            moveRoute = new Cell[activeUnit.mobility];
        }
        
    }


    private void DeleteAllRouteAfterValue(Cell cell)
    {
        Cell[] newRoute = new Cell[activeUnit.mobility];

        bool cut = false;

        for (int index = 0; index < moveRoute.Length; index++)
        {
            Cell currentCell = moveRoute[index];

            if (currentCell != null)
            {
                if (!cut)
                {
                    newRoute[index] = currentCell;
                    if (cell == currentCell)
                    {
                        cut = true;
                    }

                }
                else
                {
                    EventMaster.current.ClearingRouteCell(currentCell, moveRoute[index - 1]);
                }
            }
        }

        moveRoute = newRoute;
    }

    private int GetRouteLength(Cell [] route)
    {
        int massLength = route.Length;
        for (int index = 0; index < massLength; index++)
        {
            if (route[index] == null)
            {
                return index;
            }
        }
        return massLength;
    }

    private Cell getRouteLast(Cell[] mass)
    {
        Cell lastCell = null;
        for (int index = 0; index < mass.Length; index++)
        {
            Cell currentCell = mass[index];
            if (currentCell == null)
            {
                return lastCell;
            }
            lastCell = currentCell;

        }
        return lastCell;
    }

    public void Pass()
    {
        Debug.Log("pass!");
        if (idle && playerTurn) nextTurn();
    }

    private void nextTurn()
    {
        if (!fightIsOver)
        {
            turnCounter++;

            Debug.Log("Следующий ход: " + turnCounter);
            playerTurn = !playerTurn;

            EventMaster.current.TurnChanging(playerTurn);


            Debug.Log("Очередь игрока? - " + playerTurn);

            if (playerTurn)
            {
                if (activeUnit != null)
                {
                    updateActiveCells(activeUnit.transform.position, activeUnit.mobility, activeUnit.distance);

                    showActiveCells();
                }
            }
            else
            {
                AISolution();
            }
        }

        
    }

    
    public void AISolution()
    {
        Debug.Log("Решение ИИ");

        TurnData AIMove = AI.Action("Enemy", 2);

        int AIAction = AIMove.action;

        switch (AIAction)
        {
            case 0:
                nextTurn();
                break;
            case 1:
                AttackTarget(AIMove.target.id);
                break;
            case 2:
                moveUnit(AIMove.route, AIMove.activeUnit.id);
                break;
        }
    }

    private void MoveComplete(GameObject unit, int unitId, Vector3[] unitPoses)
    {
        idle = true;
        EventMaster.current.StatusTurnChanging(idle);

        nextTurn();
    }

    private void SetActiveUnit(Unit unit)
    {
        gridTable.turnOffCells();
        if (unit == activeUnit)
        {
            ClearRoute();
            activeUnit = null;
            return;
        }
        activeUnit = unit;

        moveRoute = new Cell[activeUnit.mobility];

        updateActiveCells(activeUnit.transform.position, activeUnit.mobility, activeUnit.distance);

        showActiveCells();

        ClearRoute();

        return;
        }


    private void updateActiveCells(Vector3 center, int moveRadius, int attackRadius)
    {
        possibleMoveCells = gridTable.GetRange(center, moveRadius, false);
        realMoveCells = gridTable.GetRealMoveCells(center, moveRadius);


    }

    public void showActiveCells()
    {
        if (realMoveCells != null) gridTable.turnOnSomeCells(realMoveCells);
    }

    public void hideActiveCells()
    {
        if (realMoveCells != null) gridTable.turnOffSomeCells(realMoveCells);
    }

    private bool canMove(Cell cell, Cell[] moveRange)
    {
        if (moveRange.Contains(cell))
        {
            return true;
        }
        return false;
    }

    private void moveUnit(Cell[] route, int objId)
    {
        idle = false;
        EventMaster.current.StatusTurnChanging(idle);

        EventMaster.current.UnitMovingOnRoute(objId, route);
        hideActiveCells();

        ClearRoute();
    }


    private void AttackTarget(int targetId)
    {
        AttackersData targetData = currentBattlePosition.attackersInfo[targetId];
        BattleSlot target = targetData.obj;

        Vector3 attackPoint = target.center;

        foreach (KeyValuePair<BattleSlot, int> item in targetData.possibleAttackers)
        {
            BattleSlot attacker = item.Key;
            EventMaster.current.UnitAttacking(attacker, target, attackPoint, attacker.damage);
        }

        if (activeUnit != null)
        {
            hideActiveCells();

            ClearRoute();
        }

        nextTurn();
    }

    private void ClickedOnEnemy(GameObject obj)
    {
        AttackersData objData = currentBattlePosition.attackersInfo[obj.GetComponent<Destructible>().id];

        Dictionary<BattleSlot, int> attackers = objData.possibleAttackers;
        if (attackers.Count > 0)
        {

            Debug.Log("Атакующие объекта +" + obj.name);

            foreach (KeyValuePair<BattleSlot, int> item in attackers)
            {
                Debug.Log(item.Key.id);
            }

            AttackTarget(objData.obj.id);
            return;
        }

        Debug.Log("Аткующих 0!");
    }

    private void ObjectDestroyed(GameObject obj, Vector3[] occypiedPoses)
    {
        foreach (Vector3 pose in occypiedPoses)
        {
            Cell currentCell = gridTable.getCellInfoByPose(pose);
            if (currentCell != null &&  possibleMoveCells != null)
            {
                currentCell.renderSwitch(canMove(currentCell, possibleMoveCells));   
            }
        }
    }


    private void AllUnitsDown(bool EnemyDown)
    {
        EventMaster.current.FightFinishing(EnemyDown);
        FightIsOver(EnemyDown);
    }


    public void FightIsOver(bool playerWon)
    {
        if (playerWon)
        {
            Debug.Log("Победа!");
            return;
        }
        Debug.Log("Поражение!");

        fightIsOver = true;
    }


    private void ObjectInterract(GameObject obj, string[] enemyTags)
    {
        if (!obj.tag.Contains("Friendly"))
        {
            ClickedOnEnemy(obj);
            return;
        }
        if (obj.tag == currentBattlePosition.friendlyUnitTag) SetActiveUnit(obj.GetComponent<Unit>());

    }

    public void clickedOnCell(Cell cell, GameObject occypier)
    {
        Debug.Log("Клетка принята");

        if (existPreview)
        {
            if (cell.occypier == null)
            {
                EventMaster.current.SpawnUnit(cell, previewId);

            }
            return;
        }

        if (playerTurn && idle)
        {
            if (occypier != null)
            {
                ObjectInterract(occypier, currentBattlePosition.GetEnemyTagsByTag(occypier.tag));
            }

            else
            {
                if (activeUnit != null && moveRoute.Contains(cell))
                {
                    moveUnit(moveRoute, activeUnit.GetComponent<Destructible>().id);
                }
            }
        }
        }
    }


