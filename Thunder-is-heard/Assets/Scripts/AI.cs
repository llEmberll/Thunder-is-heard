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
    private TurnData bestMove, currentParentMove;
    private float evaluationByBestMove, evaluationByCurrentParentMove;


    private void Start()
    {
        gridTable = GameObject.FindWithTag("GridTable").GetComponent<GridTable>();
        battleManager = GameObject.FindWithTag("BattleManager").GetComponent<BattleManager>();

    }


    public TurnData Action(string AITag, int depth)
    {
        if (AITag.Contains("Friendly")) AISide = 1;
        evaluationByBestMove = -9999 * AISide;

        Dictionary<TurnData, UnitTable> AllTurns = battleManager.GetAllPositionsFromPosition(battleManager.currentPosition);

        Debug.Log("Все ходы получены!");

        foreach (KeyValuePair<TurnData, UnitTable> item in AllTurns)
        {
            if (item.Key.action == 1)
            {
                Debug.Log("Атака объекта " + item.Key.target.id);
            }
            else if (item.Key.action == 2)
            {
                Debug.Log("Передвижения объекта " + item.Key.activeUnit.id + " в точку " + item.Key.point.cellPose);
            }

            Debug.Log("Оценка: " + item.Value.positionEvaluation);
        }

        bestMove = GetBestTurn(AllTurns);

        Debug.Log("Лучший ход:");
        
        if (bestMove.action == 0) Debug.Log("Ожидание");
        else
        {
            if (bestMove.action == 1)
            {
                Debug.Log("Атаковать цель с id: " + bestMove.target.id);
            }
            else
            {
                Debug.Log("Передвигаться юнитом  с id " + bestMove.activeUnit.id + " в точку " + bestMove.point.cellPose);
            }
        }

        bestMove = BuildRoute(bestMove, bestMove.activeUnit);

        //bestMove = FindBestMoveMain(AllTurns, 0, depth);

        //return BuildRoute(bestMove, bestMove.activeUnit);

        return bestMove;
    }


    private void UpgradePositionsEvaluation(Dictionary<TurnData, UnitTable> positions)
    {

    }


    private TurnData BuildRoute(TurnData move, BattleSlot activeObj = null)
    {
        if (activeObj == null || move.action != 2 || move.point == null) return move;

        Debug.Log("Build route. unit center now: " + activeObj.center);


        Debug.Log("unit mobility: " + activeObj.mobility);


        Dictionary<Cell, int> realMoveCells = gridTable.GetRealMoveCells(activeObj.center, activeObj.mobility);

        Cell[] route = new Cell[activeObj.mobility];
        Cell lastCell = move.point;

        route[realMoveCells[lastCell] - 1] = lastCell;

        for (int index = realMoveCells[move.point]; index > 1; index--)
        {
            route[index-2] = lastCell = FindPreviousCell(index, lastCell, realMoveCells);
        }

        move.route = route;

        return move;
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


    private TurnData FindBestMoveMain(Dictionary<TurnData, UnitTable> turns, int currentDepth, int depth)
    {
            foreach (KeyValuePair<TurnData, UnitTable> item in turns)
            {
                currentParentMove = item.Key;


                bestMove = FindBestMove(battleManager.GetAllPositionsFromPosition(item.Value), currentDepth, depth);
            }


        Debug.Log("Найден лучший ход");

        return bestMove;
    }


    private TurnData FindBestMove(Dictionary<TurnData, UnitTable> turns, int currentDepth, int depth)
    {
        if (currentDepth > depth)
        {
            if (evaluationByCurrentParentMove >= evaluationByBestMove)
            {
                evaluationByBestMove = evaluationByCurrentParentMove;
                bestMove = currentParentMove;
            }
        }

        else
        {

                foreach (KeyValuePair<TurnData, UnitTable> item in turns)
                {
                    evaluationByCurrentParentMove = item.Value.positionEvaluation;
                    FindBestMove(battleManager.GetAllPositionsFromPosition(item.Value), currentDepth++, depth);
                }
            
                
        }

        Debug.Log("Возможный лучший ход возвращается");

        return bestMove;
    }


    private TurnData GetBestTurn(Dictionary<TurnData, UnitTable> turns)
    {
        float bestEvaluation = 9999 * AISide;
        float nearestDistanceToEnemy = battleManager.currentPosition.nearestDistanceToEnemy;

        bestMove = new TurnData(0);

        foreach (KeyValuePair<TurnData, UnitTable> item in turns)
        {
            if (item.Key.action == 1)
            {
                if (bestMove.action == 1 && item.Value.positionEvaluation * AISide < bestEvaluation)
                {

                }
                else
                {
                    AttackersData attackersData = item.Value.attackersInfo[item.Key.target.id];
                    if (attackersData.inputDamage >= attackersData.obj.health)
                    {
                        bestMove = item.Key;
                        bestEvaluation = item.Value.positionEvaluation * AISide;
                    }
                    else
                    {
                        int AITurnsToKill = Mathf.CeilToInt(attackersData.obj.health / attackersData.inputDamage);
                        int enemyTurnsToKill = 9999;
                        foreach (KeyValuePair<BattleSlot, int> targetData in attackersData.possibleTargets)
                        {
                            int currentTurnsToKill = Mathf.CeilToInt(targetData.Key.health / targetData.Value);
                            if (currentTurnsToKill < enemyTurnsToKill) enemyTurnsToKill = currentTurnsToKill;
                            if (enemyTurnsToKill < AITurnsToKill) break;
                        }
                        if (enemyTurnsToKill > AITurnsToKill)
                        {
                            bestMove = item.Key;
                            bestEvaluation = item.Value.positionEvaluation * AISide;
                        }
                    }
                }
                
            }
            else if (item.Value.positionEvaluation * AISide > bestEvaluation)
            {
                if (bestMove.action != 1)
                {
                    bestMove = item.Key;
                    bestEvaluation = item.Value.positionEvaluation * AISide;
                    if (bestMove.action == 2) nearestDistanceToEnemy = item.Value.attackersInfo[item.Key.activeUnit.id].distanceToNearestEnemyUnit;
                }

                

            }

            else if (item.Value.positionEvaluation * AISide == bestEvaluation)
            {
                if (item.Key.action == 2 && bestMove.action == 2)
                {
                    float newNearestDistanceToEnemy = item.Value.attackersInfo[item.Key.activeUnit.id].distanceToNearestEnemyUnit;
                    if (newNearestDistanceToEnemy < nearestDistanceToEnemy)
                    {
                        bestMove = item.Key;
                        bestEvaluation = item.Value.positionEvaluation * AISide;
                        nearestDistanceToEnemy = newNearestDistanceToEnemy;
                    }
                    
                }
            }
        }
        return bestMove;
    }

    private TurnData GetFirstTurn(Dictionary<TurnData, UnitTable> turns)
    {
        TurnData turn = new TurnData(0);
        foreach (KeyValuePair<TurnData, UnitTable> item in turns)
        {
            return item.Key;
        }
        return turn;
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

