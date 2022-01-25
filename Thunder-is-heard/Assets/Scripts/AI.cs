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
    private UnitTable positionInBestMove;
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

        bestMove = GetBestTurn(AllTurns);

        bestMove = BuildRoute(bestMove, bestMove.activeUnit);

        //bestMove = FindBestMoveMain(AllTurns, 0, depth);

        //return BuildRoute(bestMove, bestMove.activeUnit);

        return bestMove;
    }



    private TurnData BuildRoute(TurnData move, BattleSlot activeObj = null)
    {
        if (activeObj == null || move.action != 2 || move.point == null) return move;

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
        positionInBestMove = battleManager.currentPosition;

        bool stalemate = false;


        foreach (KeyValuePair<TurnData, UnitTable> item in turns)
        {

            Debug.Log("Начало поиска хода, stalemate = " + stalemate);


            if (item.Key.action == 1)
            {
                if (!item.Value.attackersInfo.ContainsKey(item.Key.target.id))
                {
                    positionInBestMove = item.Value;
                    bestMove = item.Key;
                    bestEvaluation = positionInBestMove.positionEvaluation * AISide;
                }
                else
                {
                    if (bestMove.action == 1 && positionInBestMove.attackersInfo[bestMove.target.id].possibleUnitTargets.Count > item.Value.attackersInfo[item.Key.target.id].possibleUnitTargets.Count)
                    {

                    }
                    else
                    {
                        positionInBestMove = item.Value;
                        bestMove = item.Key;
                        bestEvaluation = positionInBestMove.positionEvaluation * AISide;
                    }
                }
                
                
            }
            else if (item.Key.action == 2)
            {
                if (bestMove.action != 1)
                {
                    AttackersData activeUnitDataCurrent = item.Value.attackersInfo[item.Key.activeUnit.id];
                    AttackersData activeUnitDataInBestMove = positionInBestMove.attackersInfo[item.Key.activeUnit.id];

                    if (activeUnitDataCurrent.possibleUnitTargets.Count > activeUnitDataInBestMove.possibleUnitTargets.Count)
                    {
                        if (activeUnitDataCurrent.inputDamage == 0)
                        {
                            positionInBestMove = item.Value;
                            bestMove = item.Key;
                            bestEvaluation = item.Value.positionEvaluation * AISide;
                            nearestDistanceToEnemy = item.Value.attackersInfo[item.Key.activeUnit.id].distanceToNearestEnemyUnit;
                        }
                        else
                        {
                            if (activeUnitDataInBestMove.inputDamage != 0)
                            {
                                if (Mathf.CeilToInt(activeUnitDataCurrent.obj.health / activeUnitDataCurrent.inputDamage) > Mathf.CeilToInt(activeUnitDataInBestMove.obj.health / activeUnitDataInBestMove.inputDamage))
                                {
                                    positionInBestMove = item.Value;
                                    bestMove = item.Key;
                                    bestEvaluation = item.Value.positionEvaluation * AISide;
                                    nearestDistanceToEnemy = item.Value.attackersInfo[item.Key.activeUnit.id].distanceToNearestEnemyUnit;
                                }
                            }
                            else
                            {
                                if (activeUnitDataCurrent.obj.health > activeUnitDataCurrent.inputDamage)
                                {
                                    positionInBestMove = item.Value;
                                    bestMove = item.Key;
                                    bestEvaluation = item.Value.positionEvaluation * AISide;
                                    nearestDistanceToEnemy = item.Value.attackersInfo[item.Key.activeUnit.id].distanceToNearestEnemyUnit;
                                }
                                else
                                {
                                    if (battleManager.currentPosition.positionEvaluation > 0)
                                    {
                                        positionInBestMove = item.Value;
                                        bestMove = item.Key;
                                        bestEvaluation = item.Value.positionEvaluation * AISide;
                                        nearestDistanceToEnemy = item.Value.attackersInfo[item.Key.activeUnit.id].distanceToNearestEnemyUnit;
                                    }
                                    else
                                    {
                                        if (stalemate)
                                        {
                                            positionInBestMove = item.Value;
                                            bestMove = item.Key;
                                            bestEvaluation = item.Value.positionEvaluation * AISide;
                                            nearestDistanceToEnemy = item.Value.attackersInfo[item.Key.activeUnit.id].distanceToNearestEnemyUnit;
                                        }
                                    }
                                }
                            }
                            
                        }
                        
                        
                    }
                    else if (activeUnitDataCurrent.possibleUnitTargets.Count == activeUnitDataInBestMove.possibleUnitTargets.Count)
                    {
                        if (activeUnitDataCurrent.inputDamage < activeUnitDataInBestMove.inputDamage)
                        {
                            positionInBestMove = item.Value;
                            bestMove = item.Key;
                            bestEvaluation = item.Value.positionEvaluation * AISide;
                            nearestDistanceToEnemy = item.Value.attackersInfo[item.Key.activeUnit.id].distanceToNearestEnemyUnit;
                        }
                        else if (activeUnitDataCurrent.inputDamage == activeUnitDataInBestMove.inputDamage)
                        {
                            float newNearestDistanceToEnemy = item.Value.attackersInfo[item.Key.activeUnit.id].distanceToNearestEnemyUnit;
                            if (newNearestDistanceToEnemy < positionInBestMove.attackersInfo[item.Key.activeUnit.id].distanceToNearestEnemyUnit)
                            {

                                positionInBestMove = item.Value;
                                bestMove = item.Key;
                                bestEvaluation = item.Value.positionEvaluation * AISide;
                                nearestDistanceToEnemy = item.Value.attackersInfo[item.Key.activeUnit.id].distanceToNearestEnemyUnit;
                            }
                            
                        }

                    }
                    
                }

                stalemate = CheckForStalemate(positionInBestMove);
                Debug.Log("Завершение поиска хода, stalemate = " + stalemate);
                

            }
        }
        return bestMove;
    }


    private bool CheckForStalemate(UnitTable position)
    {
        float maxDistanceDifferenceBetweenUnits = 0;
        float minDistanceDifferenceBetweenUnits = 9999;
        foreach (KeyValuePair<int, AttackersData> item in position.attackersInfo)
        {
            if (item.Value.obj.tag.Contains("Unit"))
            {
                float currentDifference = item.Value.distanceToNearestEnemyUnit;

                Debug.Log("расстояние до  ближайшего юнита, объект " + item.Value.obj.id + " :" + currentDifference);

                if (currentDifference > maxDistanceDifferenceBetweenUnits) maxDistanceDifferenceBetweenUnits = currentDifference;
                if (currentDifference < minDistanceDifferenceBetweenUnits) minDistanceDifferenceBetweenUnits = currentDifference;
                if (Mathf.Abs(maxDistanceDifferenceBetweenUnits - minDistanceDifferenceBetweenUnits) > 1) return false;
            }
            
        }
        return true;
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

