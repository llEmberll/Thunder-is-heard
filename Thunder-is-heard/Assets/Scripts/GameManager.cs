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
        EventMaster.current.UnitClicked += ClickedOnUnit;
        EventMaster.current.BuildClicked += ClickedOnBuild;
        EventMaster.current.CompleteMove += MoveComplete;
        EventMaster.current.ClickedOnCell += clickedOnCell;
        EventMaster.current.UnitDies += UnitTakedDown;
        EventMaster.current.MouseOnCellEnter += MouseOnCellEnter;
        EventMaster.current.SelectedObject += MouseOnActiveUnit;

        activeUnit = null;
        idle = true;
        turnCounter = 0;
        playerTurn = true;

        EventMaster.current.TurnChanging(playerTurn);
        EventMaster.current.StatusTurnChanging(idle);
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
                int countRouteCells = getRouteLength(moveRoute);
                Cell lastRouteCell = getRouteLast(moveRoute);

                if (countRouteCells < 1)
                {
                    Cell[] firstStep = gridTable.getRange(activeUnit.transform.position, 1, false);
                    if (canMove(cell, firstStep))
                    {
                        moveRoute[0] = cell;
                        Cell previousRouteCell = gridTable.getCellInfoByPose(activeUnit.transform.position);

                        countRouteCells = getRouteLength(moveRoute);

                        EventMaster.current.SelectCellForRoute(cell, previousRouteCell, activeUnit.Mobility == countRouteCells);
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


    private bool NeighbourCell(Vector3 pose, Vector3 cellPose)
    {
        if (Mathf.Max(Mathf.Abs((int)(pose.x - cellPose.x)), Mathf.Abs((int)(pose.z - cellPose.z))) == 1) return true;
        return false;
    }

    private void updateMoveRange(Vector3 unitPose, int unitRange)
    {
        if (unitRange == 1)
        {
            foreach (Cell cell in possibleMoveCells)
            {
                if (cell != null)
                {
                    realMoveCells.Add(cell, 1);
                }
            }
            return;
        }
        else
        {
            Cell[] firstCells = gridTable.getRange(unitPose, 1, false);

            foreach (Cell firstStep in firstCells)
            {
                if (firstStep != null)
                {
                    realMoveCells.Add(firstStep, 1);
                }
            }
            updateLevelsOfCells(unitRange, firstCells, 2);
        }
    }


    private void updateLevelsOfCells(int range, Cell[] previousCells, int currentRange)
    {
        Cell[] newCells = new Cell[8 * currentRange];
        int newCellsIndex = 0;

        for (int index = 0; index < previousCells.Length; index++)
        {
            Cell currentPreviousCell = previousCells[index];
            if (currentPreviousCell != null)
            {
                for (int index2 = 0; index2 < possibleMoveCells.Length; index2++)
                {
                    Cell currentPossibleCell = possibleMoveCells[index2];
                    if (currentPossibleCell != null)
                    {
                        if (NeighbourCell(currentPreviousCell.cellPose, currentPossibleCell.cellPose))
                        {
                            if (!realMoveCells.ContainsKey(currentPossibleCell))
                            {
                                realMoveCells.Add(currentPossibleCell, currentRange);
                                newCells[newCellsIndex] = currentPossibleCell;
                                newCellsIndex++;
                            }
                            possibleMoveCells[index2] = null;
                        }
                    }
                }
            }
        }
        if (currentRange < range) updateLevelsOfCells(range, newCells, currentRange + 1);
        return;
    }

    private void DeleteAllRouteAfterValue(Cell cell)
    {
        Cell[] newRoute = new Cell[activeUnit.Mobility];

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
        blastCells = gridTable.getRange(center, attackRadius, true);
        realMoveCells = new Dictionary<Cell, int>(possibleMoveCells.Length);


        foreach (Cell posCell in possibleMoveCells)
        {
            if (posCell != null)
            {
                Debug.Log("Теоретически Возможная клетка: " + posCell.cellPose.x + "|| " + posCell.cellPose.z);
            }
            else
            {
                Debug.Log("Теоретически Возможная клетка равна null");
            }

        }

        updateMoveRange(center, moveRadius);

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

    private bool canAttack(params Cell[] cells)
    {
        foreach (Cell cell in cells)
        {
            if (blastCells.Contains(cell))
            {
                Debug.Log("True!");

                Debug.Log("Занятая зданием клетка: " + cell.cellPose.x + "|| " + cell.cellPose.z);



                return true;
            }
        }
        return false;
    }

    private void moveUnit(Cell[] route)
    {
        idle = false;
        EventMaster.current.StatusTurnChanging(idle);

        Debug.Log("Приказ о передвижении отправлен!");

        EventMaster.current.UnitMovingOnRoute(activeUnit.transform.position, route);
        hideActiveCells();

        ClearRoute();

    }

    private void tryAttackUnit(Unit unit)
    {
        Debug.Log("Активный юнит есть");

        Cell unitCell = gridTable.getCellInfoByPose(unit.transform.position);

        if (canAttack(unitCell))
        {
            Debug.Log("Можно атаковать");

            EventMaster.current.UnitAttackingUnit(activeUnit, unit, activeUnit.Damage);

            hideActiveCells();

            ClearRoute();

            nextTurn();

            return;
        }
        
        Debug.Log("Юниту не хватает дистанции для атаки");
    }


    private void ClickedOnBuild(Build build, Cell[] occypiedCells)
    {

        foreach (Cell ocCell in occypiedCells)
        {
            Debug.Log("Занятая зданием клетка: " + ocCell.cellPose.x + "|| " + ocCell.cellPose.z);
        }

        Debug.Log("Здание принято");
        if (idle && activeUnit != null && build.CompareTag("EnemyBuild"))
        {
            Debug.Log("Здание может являться целью");
            if (canAttack(occypiedCells))
            {
                Debug.Log("Здание может быть атаковано");

                EventMaster.current.UnitAttackingBuild(activeUnit, build, activeUnit.Damage);

                hideActiveCells();

                ClearRoute();

                nextTurn();

                Debug.Log("Ивент об атаке создан");
            }
        }
    }

    private void ClickedOnUnit(Unit unit)
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
                    tryAttackUnit(unit);
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


