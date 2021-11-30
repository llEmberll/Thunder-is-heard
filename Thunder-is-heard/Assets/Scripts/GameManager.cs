using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] public Unit activeUnit;

    private GridTable gridTable;

    private Cell[] possibleMoveCells;
    private Cell[] blastCells;
    private Cell[] moveRoute;

    
    private bool idle;

    private int turnCounter;
    public bool playerTurn;

    private Dictionary<Cell, int> realMoveCells;


    // Start is called before the first frame update
    void Start()
    {
        gridTable = GameObject.FindWithTag("GridGenerator").GetComponent<GridTable>();
        gridTable.turnOffCells();

        EventMaster.current.FightIsOver += FightIsOver;
        EventMaster.current.UnitSelected += ClickedOnUnit;
        EventMaster.current.CompleteMove += MoveComplete;
        EventMaster.current.ClickedOnCell += clickedOnCell;
        EventMaster.current.UnitDies += UnitTakedDown;
        EventMaster.current.MouseOnCellEnter += MouseOnCellEnter;
        EventMaster.current.SelectedCellUnderUnit += MouseOnUnit;


        activeUnit = null;
        idle = true;
        turnCounter = 0;
        playerTurn = true;

        EventMaster.current.TurnChanging(playerTurn);
        EventMaster.current.StatusTurnChanging(idle);
        
    }

    private void MouseOnUnit(Vector3 pose, Material material, bool render)
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
                int countRouteCells = getRouteLength(moveRoute);
                Cell lastRouteCell = getRouteLast(moveRoute);

                if (countRouteCells < 1)
                {
                    Cell[] firstStep = gridTable.getRange(activeUnit.transform.position, 1, false);
                    if (canMove(cell, firstStep))
                    {
                        moveRoute[0] = cell;
                        Cell previousRouteCell = gridTable.getCellInfoByPose(activeUnit.transform.position);

                        EventMaster.current.SelectCellForRoute(cell, previousRouteCell, activeUnit.Mobility == countRouteCells++);
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

                        bool overRoute = (activeUnit.Mobility > countRouteCells);

                        if (overRoute)
                        {
                            Cell[] nextStep = gridTable.getRange(lastRouteCell.cellPose, 1, false);

                            if (canMove(cell, nextStep))
                            {
                                moveRoute[countRouteCells] = cell;

                                overRoute = activeUnit.Mobility == getRouteLength(moveRoute);
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
        EventMaster.current.ClearingRoute();
        moveRoute = new Cell[activeUnit.Mobility];
    }

    private void updatePossibleMoveCells(Vector3 center, int range, int currentRange = 1)
    {
        Cell[] newCells = new Cell[possibleMoveCells.Length];
        if (currentRange > range)
        {
            return;
        }

        for (int index = 0; index < possibleMoveCells.Length; index++)
        {
            Cell currentCell = possibleMoveCells[index];

            if (currentCell != null)
            {
                int Xdiff = Mathf.Abs((int)(center.x - currentCell.cellPose.x));
                int Zdiff = Mathf.Abs((int)(center.z - currentCell.cellPose.z));
                int maxDiff = Mathf.Max(Xdiff, Zdiff);

                if (maxDiff == 1)
                {
                    if (!realMoveCells.ContainsKey(currentCell))
                    {
                        realMoveCells.Add(currentCell, currentRange);
                        newCells[index] = currentCell;

                        possibleMoveCells[index] = null;
                        
                    }
                }
            }
        }

        for (int index = 0; index < newCells.Length; index++)
        {
            Cell currentCell = newCells[index];
            if (currentCell != null)
            {
                updatePossibleMoveCells(currentCell.cellPose, range, currentRange + 1);
            }
        }
        

    }

    private void DeleteAllRouteAfterValue(Cell cell)
    {
        Cell[] newRoute = new Cell[activeUnit.Mobility];

        bool cut = false;

        for (int index = 0; index < moveRoute.Length; index++)
        {
            Cell currentCell = moveRoute[index];
            if (!cut)
            {
                if (currentCell != null)
                {
                    newRoute[index] = currentCell;
                    if (cell == currentCell)
                    {
                        cut = true;
                    }
                }
            }
            else
            {
                EventMaster.current.ClearingRouteCell(currentCell, moveRoute[index - 1]);
            }
        }

        moveRoute = newRoute;
    }

    private int getRouteLength(Cell [] route)
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
        if (playerTurn) nextTurn();
    }

    private void nextTurn()
    {
        turnCounter++;

        Debug.Log("Следующий ход: " + turnCounter);
        playerTurn = !playerTurn;

        EventMaster.current.TurnChanging(playerTurn);

        Debug.Log("Очередь игрока? - " + playerTurn);

        if (playerTurn)
        {
            Debug.Log("Обновляем состояния клеток");


            gridTable.updateStatuses();

            if (activeUnit != null)
            {
                Debug.Log("Обновляем активные клетки");

                updateActiveCells(activeUnit.transform.position, activeUnit.Mobility, activeUnit.Range);

                showActiveCells();
            }
                
        }
        else
        {
            AIsolution();
        }
        
    }

    

    public void AIsolution()
    {
        Debug.Log("Решение ИИ");

        nextTurn();
    }

    private void MoveComplete()
    {
        Debug.Log("Движение окончено");

        idle = true;
        EventMaster.current.StatusTurnChanging(idle);

        nextTurn();
    }

    private void setActiveUnit(Unit unit)
    {
        gridTable.turnOffCells();
        if (unit == activeUnit)
        {
            ClearRoute();
            activeUnit = null;
            Debug.Log("Now null active!");
            return;
        }
        activeUnit = unit;

        Debug.Log("Now new active!");

        moveRoute = new Cell[activeUnit.Mobility];

        updateActiveCells(activeUnit.transform.position, activeUnit.Mobility, activeUnit.Range);

        showActiveCells();

        ClearRoute();

        return;
        }


    private void updateActiveCells(Vector3 center, int moveRadius, int attackRadius)
    {
        possibleMoveCells = gridTable.getRange(center, moveRadius, false);

        realMoveCells = new Dictionary<Cell, int>(possibleMoveCells.Length);

        updatePossibleMoveCells(center, moveRadius);

        blastCells = gridTable.getRange(center, attackRadius, true);
        
    }

    public void showActiveCells()
    {
        gridTable.turnOnSomeCells(realMoveCells);
    }

    public void hideActiveCells()
    {
        gridTable.turnOffSomeCells(realMoveCells);
    }

    private bool canMove(Cell cell, Cell[] moveRange)
    {
        if (moveRange.Contains(cell))
        {
            return true;
        }
        return false;
    }

    private bool canAttack(Cell cell)
    {
        return blastCells.Contains(cell);
    }

    private void moveUnit(Cell[] route)
    {
        idle = false;
        EventMaster.current.StatusTurnChanging(idle);

        Debug.Log("Приказ о передвижении отправлен!");

        EventMaster.current.UnitMovingOnRoute(activeUnit.id, route);
        hideActiveCells();

        ClearRoute();

    }

    private void tryAttack(Unit unit)
    {
        Debug.Log("Активный юнит есть");

        Cell unitCell = gridTable.getCellInfoByPose(unit.transform.position);

        if (canAttack(unitCell))
        {
            Debug.Log("Можно атаковать");

            EventMaster.current.UnitAttacking(activeUnit, unit, activeUnit.Damage);
            return;
        }
        
        Debug.Log("Юниту не хватает дистанции для атаки");
    }

    public void ClickedOnUnit(Unit unit)
    {
        Debug.Log("Юнит принят");

        if (playerTurn && idle)
        {
            Debug.Log("Движения нет");
            string unitTag = unit.tag;

            if (unitTag == "FriendlyUnit")
            {
                setActiveUnit(unit);
            }

            else if (unitTag == "EnemyUnit")
            {

                Debug.Log("Выбран вражеский юнит");

                if (activeUnit != null)
                {
                    tryAttack(unit);
                    return;

                }

                Debug.Log("Нет активного юнита для атаки");
            }
        }

        else
        {
            Debug.Log("Сейчас ходит не игрок!");
        }
    }


    public void UnitTakedDown(Unit unit)
    {
        Cell cell = gridTable.getCellInfoByPose(unit.transform.position);
        if (canMove(cell, possibleMoveCells))
        {
            cell.renderOn();
        }
        else
        {
            cell.renderOff();
        }
    }


    public void FightIsOver(bool playerWon)
    {
        if (playerWon)
        {
            Debug.Log("Победа!");
            return;
        }
        Debug.Log("Поражение!");
    }

    public void clickedOnCell(Cell cell)
    {
        Debug.Log("Клетка принята");

        if (playerTurn && activeUnit != null)
        {
            Debug.Log("Активный юнит есть");

            if (moveRoute.Contains(cell))
            {
                moveUnit(moveRoute);
            }
        }
        }
    }


