using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI : MonoBehaviour
{
    private int AISide = -1;
    private int mood;
    private GridTable gridTable;
    private BattleManager battleManager;
    private TurnData bestMove;
    private bool stalemate;


    private void Start()
    {
        gridTable = GameObject.FindWithTag("GridTable").GetComponent<GridTable>();
        battleManager = GameObject.FindWithTag("BattleManager").GetComponent<BattleManager>();

    }


    public TurnData Action(string AITag, int depth)
    {
        if (AITag.Contains("Friendly")) AISide = 1;

        Dictionary<TurnData, UnitTable> AllTurns = battleManager.GetAllPositionsFromPosition(battleManager.currentPosition);

        //foreach (KeyValuePair<TurnData, UnitTable> item in AllTurns)
        //{
        //    if (item.Key.action == 1)
        //    {
        //        Debug.Log("Атака объекта " + item.Key.target.id);
        //    }
        //    else if (item.Key.action == 2)
        //    {
        //        Debug.Log("Передвижения объекта " + item.Key.activeUnit.id + " в точку " + item.Key.point.cellPose);
        //    }

        //    Debug.Log("Оценка: " + item.Value.positionEvaluation);
        //}

        bestMove = FindBestMove(AllTurns);

        if (bestMove.action == 2 && bestMove.point != null && bestMove.activeUnit != null) bestMove.route = BuildRoute(bestMove.point, bestMove.activeUnit.center, bestMove.activeUnit.mobility);

        //bestMove = FindBestMoveMain(AllTurns, 0, depth);

        //return BuildRoute(bestMove, bestMove.activeUnit);

        return bestMove;
    }



    private Cell[] BuildRoute(Cell point, Vector3 center, int mobility, string excTag = null)
    {
        Dictionary<Cell, int> realMoveCells = gridTable.GetRealMoveCells(center, mobility, excTag);

        Cell[] route = new Cell[mobility];
        Cell lastCell = point;

        route[realMoveCells[lastCell] - 1] = lastCell;

        for (int index = realMoveCells[point]; index > 1; index--)
        {
            route[index-2] = lastCell = FindPreviousCell(index, lastCell, realMoveCells);
        }
        return route;
    }


    private Cell FindPreviousCell(int cellCost, Cell cell, Dictionary<Cell, int> cells)
    {
        List<Cell> previousCells = new List<Cell>();
        foreach (KeyValuePair<Cell, int> item in cells)
        {
            if (item.Key != null &&  item.Value == cellCost - 1 && gridTable.NeighbourCell(cell.cellPose, item.Key.cellPose)) previousCells.Add(item.Key);
        }
        if (previousCells.Count == 1) return previousCells.First();
        foreach (Cell currentCell in previousCells)
        {
            if (currentCell.cellPose.z == cell.cellPose.z || currentCell.cellPose.x == cell.cellPose.x) return currentCell;
        }
        return previousCells.First();
    }



    private TurnData FindBestMove(Dictionary<TurnData, UnitTable> turns)
    {
        Dictionary<TurnData, UnitTable> attackTurns = new Dictionary<TurnData, UnitTable>();

        Dictionary<TurnData, UnitTable> accurateOffensiveTurns = new Dictionary<TurnData, UnitTable>();

        Dictionary<TurnData, UnitTable> insaneOffensiveTurns = new Dictionary<TurnData, UnitTable>();

        Dictionary<TurnData, UnitTable> moveTurns = new Dictionary<TurnData, UnitTable>();

        foreach (KeyValuePair<TurnData, UnitTable> item in turns)
        {
            if (item.Key.action == 1) attackTurns.Add(item.Key, item.Value);
            else
            {
                TurnData currentTurn = item.Key;
                UnitTable currentPosition = item.Value;

                AttackersData activeUnitDataCurrent = currentPosition.attackersInfo[currentTurn.activeUnit.id];
                AttackersData activeUnitDataInRealPosition = battleManager.currentPosition.attackersInfo[currentTurn.activeUnit.id];

                if (activeUnitDataCurrent.possibleUnitTargets.Count > activeUnitDataInRealPosition.possibleUnitTargets.Count)
                {
                    if (activeUnitDataCurrent.inputDamage == 0 || activeUnitDataCurrent.obj.health > activeUnitDataCurrent.inputDamage)
                    {
                        accurateOffensiveTurns.Add(currentTurn, currentPosition);
                    }
                    else
                    {
                        insaneOffensiveTurns.Add(currentTurn, currentPosition);
                    }
                }
                else
                {
                    moveTurns.Add(currentTurn, currentPosition);
                }

            }
        }

        if (attackTurns.Count > 0) return ChooseBestAttack(attackTurns);

        if (accurateOffensiveTurns.Count > 0) return ChooseBestOffensive(accurateOffensiveTurns);

        stalemate = CheckForStalemate(battleManager.currentPosition, battleManager.currentPosition.enemyUnitTag);
        if (insaneOffensiveTurns.Count > 0)
        {
            if (stalemate || moveTurns.Count == 0) return ChooseBestOffensive(insaneOffensiveTurns);
        }
        if (moveTurns.Count > 0) return ChooseBestMoveTurn(moveTurns);
        else { return new TurnData(0); }
    }

    
    private TurnData ChooseBestMoveTurn(Dictionary<TurnData, UnitTable> turns)
    {
        Dictionary<int, TurnData> turnTable = new Dictionary<int, TurnData>();
        Dictionary<int, float> distanceTable = new Dictionary<int, float>();

        foreach (KeyValuePair<TurnData, UnitTable> item in turns)
        {
            BattleSlot activeUnit = item.Key.activeUnit;
            if (!distanceTable.ContainsKey(activeUnit.id))
            {

                float currentActiveUnitDistance = battleManager.currentPosition.attackersInfo[activeUnit.id].distanceToNearestEnemyUnit;

                distanceTable.Add(activeUnit.id, currentActiveUnitDistance);

                turnTable.Add(activeUnit.id, FindBestConvergenceTurn(activeUnit, turns));
            }
        }


        int unitCount = distanceTable.Count;
        for (int index = 0; index < unitCount; index++)
        {
            int nearestUnitId = FindNearestDistance(distanceTable);
            TurnData currentTurn = turnTable[nearestUnitId];
            if (currentTurn != null) return currentTurn;
            distanceTable.Remove(nearestUnitId);
        }
        return new TurnData(0);
    }



    private int FindNearestDistance(Dictionary<int, float> table)
    {
        int nearestKey = 9999;
        float nearestDistance = 9999;

        foreach (KeyValuePair<int, float> item in table)
        {
            if (nearestDistance > item.Value)
            {
                nearestKey = item.Key;
                nearestDistance = item.Value;
            }
        }
        return nearestKey;
    }



    private TurnData FindBestConvergenceTurn(BattleSlot activeUnit, Dictionary<TurnData, UnitTable> turns)
    {
        TurnData bestTurn = null;

        AttackersData attackersData = battleManager.currentPosition.attackersInfo[activeUnit.id];

        float bestDistance = attackersData.distanceToNearestEnemyUnit;

        foreach (KeyValuePair<TurnData, UnitTable> item in turns)
        {
            if (item.Key.activeUnit.id == activeUnit.id)
            {
                float currentDistance = item.Value.attackersInfo[activeUnit.id].distanceToNearestEnemyUnit;
                if (bestDistance > currentDistance)
                {
                    bestTurn = item.Key;
                    bestDistance = currentDistance;
                }
            }
        }

        if (bestTurn == null)
        {
            GameObject obstacle = IsObstacleOnForward(attackersData.nearestUnit.center, attackersData.obj.center);
            if (obstacle != null)
            {
                Cell[] shortestRoute = BuildRoute(gridTable.getCellInfoByPose(attackersData.nearestUnit.center), attackersData.obj.center, Mathf.Max(gridTable._gridSize.x, gridTable._gridSize.y), attackersData.nearestUnit.tag);

                foreach (KeyValuePair<TurnData, UnitTable> item in turns)
                {
                    if (item.Key.activeUnit.id == activeUnit.id && item.Key.point.cellPose == shortestRoute[activeUnit.mobility - 1].cellPose) return item.Key;
                }
            }
        }
        return bestTurn;
    }


    private TurnData ChooseBestOffensive(Dictionary<TurnData, UnitTable> turns)
    {
        TurnData bestOffensive = null;
        int bestInputDamage = 9999;

        foreach (KeyValuePair<TurnData, UnitTable> item in turns)
        {
            TurnData currentTurn = item.Key;
            UnitTable currentPosition = item.Value;

            AttackersData activeUnitDataCurrent = currentPosition.attackersInfo[currentTurn.activeUnit.id];
            

            if (bestInputDamage > activeUnitDataCurrent.inputDamage)
            {
                bestOffensive = currentTurn;
                bestInputDamage = activeUnitDataCurrent.inputDamage;
            }

        }
        return bestOffensive;

    }

    private TurnData ChooseBestAttack(Dictionary<TurnData, UnitTable> turns)
    {
        TurnData bestAttack = null;
        float turnEvaluation = 0;
        bool isLethalAttack = false;

        foreach (KeyValuePair<TurnData, UnitTable> item in turns)
        {
            TurnData currentTurn = item.Key;
            UnitTable currentPosition = item.Value;

            float targetEvaluation = battleManager.currentPosition.attackersInfo[currentTurn.target.id].evaluation;

            if (!currentPosition.attackersInfo.ContainsKey(currentTurn.target.id))
            {
                if (!isLethalAttack)
                {
                    isLethalAttack = true;
                    bestAttack = currentTurn;
                    turnEvaluation = targetEvaluation;
                }
                else
                {
                    if (turnEvaluation == 0)
                    {
                        bestAttack = currentTurn;
                        turnEvaluation = targetEvaluation;
                    }
                    else
                    {
                        if (targetEvaluation > turnEvaluation)
                        {
                            bestAttack = currentTurn;
                            turnEvaluation = targetEvaluation;
                        }
                    }
                }
            }
            else
            {
                if (!isLethalAttack)
                {
                    if (turnEvaluation == 0)
                    {
                        bestAttack = currentTurn;
                        turnEvaluation = targetEvaluation;
                    }
                    else
                    {
                        if (targetEvaluation > turnEvaluation)
                        {
                            bestAttack = currentTurn;
                            turnEvaluation = targetEvaluation;
                        }
                    }
                }
            }
        }
        return bestAttack;
    }


    private GameObject IsObstacleOnForward(Vector3 target, Vector3 currentPoint)
    {
        int stepByX = SetComingStep((int)target.x, (int)currentPoint.x);
        int stepByZ = SetComingStep((int)target.z, (int)currentPoint.z);

        Cell nextStepToTarget = gridTable.getCellInfoByPose(new Vector3(currentPoint.x + stepByX, currentPoint.y, currentPoint.z + stepByZ));
        if (nextStepToTarget.occypier != null)
        {
            if (nextStepToTarget.occypier.tag.Contains("Build") || nextStepToTarget.occypier.tag.Contains("Indestructibles")) return nextStepToTarget.occypier;
        }
        return null;
    }


    private int SetComingStep(int target, int currentPoint)
    {
        int step = -1;
        if (target > currentPoint) step = 1;
        if (target == currentPoint) step = 0;
        return step;
    }
 

    private void ShowTurnInfo(TurnData turn)
    {
        int turnAction = turn.action;
        switch (turnAction)
        {
            case 0:
                Debug.Log("Ожидание");
                break;
            case 1:
                Debug.Log("Атака цели " + turn.target.id);
                break;
            case 2:
                Debug.Log("Перемещение юнита " + turn.activeUnit.id + " на точку " + turn.point.cellPose);
                break;
        }
    }


    private bool CheckForStalemate(UnitTable position, string unitTag)
    {
        if (position.collections[unitTag].Count() < 2) return true;
        float maxDistanceDifferenceBetweenUnits = 0;
        float minDistanceDifferenceBetweenUnits = 9999;
        foreach (BattleSlot unit in position.collections[unitTag])
        {
                float currentDifference = position.attackersInfo[unit.id].distanceToNearestEnemyUnit;

                if (currentDifference > maxDistanceDifferenceBetweenUnits) maxDistanceDifferenceBetweenUnits = currentDifference;
                if (currentDifference < minDistanceDifferenceBetweenUnits) minDistanceDifferenceBetweenUnits = currentDifference;
                if (Mathf.Abs(maxDistanceDifferenceBetweenUnits - minDistanceDifferenceBetweenUnits) > 1) return false;
        }
        return true;
    }


}
public class TurnData
{
    public int action = 0; //0 - ожидание; 1 - атака; 2 - передвижение

    public BattleSlot target = null;

    public BattleSlot activeUnit = null;

    public Cell point;

    public Cell[] route;

    public TurnData(int Action, BattleSlot activeObject = null, Cell Point = null, Cell[] moveRoute = null)
    {
        action = Action;
        activeUnit = activeObject;
        point = Point;
        route = moveRoute;
    }
}

