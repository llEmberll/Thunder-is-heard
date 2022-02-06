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
    private float bestEvaluation, evaluationByCurrentParentMove;
    private float nearestDistanceToEnemy;
    private bool stalemate, bestMoveIsAttackingMoving;


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

        bestMove = GetBestTurn(AllTurns);

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
            if (evaluationByCurrentParentMove >= bestEvaluation)
            {
                bestEvaluation = evaluationByCurrentParentMove;
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
        bestEvaluation = 9999 * AISide;
        nearestDistanceToEnemy = 9999;

        bestMove = new TurnData(0);
        positionInBestMove = battleManager.currentPosition;

        stalemate = bestMoveIsAttackingMoving = false;

        foreach (KeyValuePair<TurnData, UnitTable> item in turns)
        {

            TurnData currentTurn = item.Key;
            UnitTable currentPosition = item.Value;

            if (currentTurn.action == 1)
            {
                AttackTurnHandle(currentTurn, currentPosition);
            }
            else if (currentTurn.action == 2)
            {
                if (bestMove.action != 1)
                {
                    MoveTurnHandle(currentTurn, currentPosition);
                }

                stalemate = CheckForStalemate(positionInBestMove, positionInBestMove.enemyUnitTag);
            }
        }
        return bestMove;
    }


    private void AttackTurnHandle(TurnData turn, UnitTable position)
    {
        if (!position.attackersInfo.ContainsKey(turn.target.id))
        {
            SetTurnAsBest(turn, position, true);
        }
        else
        {
            if (bestMove.action == 1)
            {
                if (!positionInBestMove.attackersInfo.ContainsKey(bestMove.target.id))
                {
                }
                else if (positionInBestMove.attackersInfo[bestMove.target.id].possibleUnitTargets.Count > position.attackersInfo[turn.target.id].possibleUnitTargets.Count || positionInBestMove.attackersInfo[bestMove.target.id].outputDamage > position.attackersInfo[turn.target.id].outputDamage)
                {
                }
                else
                {
                    SetTurnAsBest(turn, position, true);
                }
            }
            else
            {
                SetTurnAsBest(turn, position, true);
            }
        }
    }


    private void MoveTurnHandle(TurnData turn, UnitTable position)
    {

        AttackersData activeUnitDataCurrent = position.attackersInfo[turn.activeUnit.id];
        AttackersData activeUnitDataInBestMove = positionInBestMove.attackersInfo[turn.activeUnit.id];

        if (activeUnitDataCurrent.possibleUnitTargets.Count > activeUnitDataInBestMove.possibleUnitTargets.Count)
        {

            if (activeUnitDataCurrent.inputDamage == 0 || activeUnitDataCurrent.obj.health > activeUnitDataCurrent.inputDamage || stalemate)
            {

                if (bestMove.action != 1 || !bestMoveIsAttackingMoving)
                {

                        SetTurnAsBest(turn, position, true);
                    
                    
                }

            }
   
            else
            {
                int currentPositionInputDamage = battleManager.currentPosition.attackersInfo[activeUnitDataCurrent.obj.id].inputDamage;
                if (currentPositionInputDamage > 0)
                {
                    if (positionInBestMove.attackersInfo[activeUnitDataCurrent.obj.id].inputDamage >= currentPositionInputDamage)
                    {
                        SetTurnAsBest(turn, position, true);
                    }
                }
            }


        }
        else if (activeUnitDataCurrent.possibleUnitTargets.Count == activeUnitDataInBestMove.possibleUnitTargets.Count)
        {
            Debug.Log("В этом ходу цели активного юнита равны целям этого же юнита в лучшем ходе");

            if (!bestMoveIsAttackingMoving)
            {
                Debug.Log("Лучший ход не атакующий");

                if (activeUnitDataCurrent.inputDamage < activeUnitDataInBestMove.inputDamage)
                {

                    Debug.Log("Входной урон в текущем ходу у активного юнита меньше, чем у этого же юнита в лучшем ходе, поэтому текущий ход теперь лучший");

                    SetTurnAsBest(turn, position);
                }
                else
                {
                    AttackersData currentActiveAttackersData = position.attackersInfo[turn.activeUnit.id];


                    Debug.Log("Входной урон в текущем ходу у активного юнита такой же, как у этого же юнита в лучшем ходе");

                    if (bestMove.action == 0 && currentActiveAttackersData.distanceToNearestEnemyUnit < battleManager.currentPosition.attackersInfo[turn.activeUnit.id].distanceToNearestEnemyUnit)
                    {
                        Debug.Log("Лучший ход - ожидание + активный юнит становится ближе к цели, значит текущий лучше!");



                        SetTurnAsBest(turn, position);
                    }
                    else
                    {

                        Debug.Log("Лучший ход не ожидающий, либо дистанция к цели не уменьшается!");


                        float newNearestDistanceToEnemy = currentActiveAttackersData.distanceToNearestEnemyUnit;

                        Debug.Log("Дистанция до юнита в текущем ходе: " + newNearestDistanceToEnemy);
                        Debug.Log("Ближайшая дистанция: " + nearestDistanceToEnemy);

                        if (newNearestDistanceToEnemy < nearestDistanceToEnemy)
                        {
                            Debug.Log("Эта дистанция меньше чем лучшая дистанция в лучшем ходу");
                            
                            

                            if (newNearestDistanceToEnemy < positionInBestMove.attackersInfo[turn.activeUnit.id].distanceToNearestEnemyUnit)
                            {
                                nearestDistanceToEnemy = newNearestDistanceToEnemy;

                                Debug.Log("Дистанция в текущем ходе меньше чем этого же юнита в лучшем ходе: " + positionInBestMove.attackersInfo[turn.activeUnit.id].distanceToNearestEnemyUnit);
                                Debug.Log("Этот ход лучший!");
                                Debug.Log("Предыдущий лучший ход:");

                                ShowTurnInfo(bestMove);

                                SetTurnAsBest(turn, position);




                            }
                            else
                            {
                                Debug.Log("Else!!!!");

                                GameObject obstacle = IsObstacleOnForward(currentActiveAttackersData.nearestUnit.center, turn.activeUnit.center);
                                if (obstacle != null)
                                {
                                    Debug.Log("Point = " + currentActiveAttackersData.nearestUnit.center);

                                    Cell point = gridTable.getCellInfoByPose(currentActiveAttackersData.nearestUnit.center);

                                    Debug.Log("pointCELL = " + point.cellPose);

                                    Cell[] shortestRoute = BuildRoute(gridTable.getCellInfoByPose(currentActiveAttackersData.nearestUnit.center), battleManager.currentPosition.attackersInfo[turn.activeUnit.id].obj.center, Mathf.Max(gridTable._gridSize.x, gridTable._gridSize.y), currentActiveAttackersData.nearestUnit.tag);


                                    Debug.Log("Лучшая клетка согласно мобильности: " + shortestRoute[turn.activeUnit.mobility - 1].cellPose);
                                    Debug.Log("А в текущем ходу клетка для передвижения " + turn.point.cellPose);


                                    if (turn.point.cellPose == shortestRoute[turn.activeUnit.mobility - 1].cellPose && !bestMoveIsAttackingMoving)
                                    {
                                        Debug.Log("Клетка в ходу является лучшей клеткой согласно кротчайшему маршруту!!! Ход лучший");

                                        nearestDistanceToEnemy = newNearestDistanceToEnemy;
                                        SetTurnAsBest(turn, position);
                                    }
                                }
                            }


                        }
                        else
                        {
                            Debug.Log("Новая дистанция не меньше чем установленна ближайшая: " + nearestDistanceToEnemy + " поэтому ход не будет избран!");
                        }

                        
                    }

                }
            }
            
        }
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


    private void FindWayOutOfObstacle(Vector3 target, Vector3 currentPoint, GameObject obstacle)
    {
        Cell shortestWayOut;
        float shortestDistance = 9999;


    }


    private void FindNeighbourObstacle(GameObject obstacle)
    {
        Vector3[] occypiedCells;
        Vector3 center;
        int sizeX, sizeZ;
        if (obstacle.tag.Contains("Build"))
        {
            Destructible obstacleClass = obstacle.GetComponent<Destructible>();
            occypiedCells = obstacleClass.occypiedPoses;
            center = obstacleClass.center;
            sizeX = obstacleClass.sizeX;
            sizeZ = obstacleClass.sizeZ;
        }
        else
        {
            Indestructible obstacleClass = obstacle.GetComponent<Indestructible>();
            occypiedCells = obstacleClass.occypiedPoses;
            center = obstacleClass.center;
            sizeX = obstacleClass.sizeX;
            sizeZ = obstacleClass.sizeZ;
        }

        Vector3[] aroundCells = GetAroundArea(obstacle.transform, occypiedCells, center, sizeX, sizeZ);

        for (int index = 0; index < aroundCells.Length; index++)
        {
            Debug.Log(aroundCells[index]);
        }
    }


    
    private Vector3[] GetAroundArea(Transform obj, Vector3[] cells, Vector3 center, int sizeX, int sizeZ)
    {
        Vector3[] aroundCells = new Vector3[((sizeX * 2) + (sizeZ * 2)) + 4];
        int stepByX = 0;
        int stepByZ = 0;
        if (transform.forward.x != 0) stepByX = (int)transform.forward.x;
        else if (transform.right.x != 0) stepByX = (int)transform.right.x;

        if (transform.right.z != 0) stepByZ = (int)transform.right.z;
        else if (transform.forward.z != 0) stepByZ = (int)transform.forward.z;

        int newMaxX = (int)obj.transform.position.x + (stepByX * (sizeX + 1));
        int newMaxZ = (int)obj.transform.position.z + (stepByZ * (sizeZ + 1));

        Vector3 newStartPoint = new Vector3(obj.transform.position.x - stepByX, obj.transform.position.y, obj.transform.position.z - stepByZ);

        int aroundCellsIndex = 0;


            for (int x = (int)newStartPoint.x; x != newMaxX; x += stepByX)
            {
                Debug.Log("Первый цикл");
                aroundCells[aroundCellsIndex] = new Vector3(x, newStartPoint.y, newStartPoint.z);

                Debug.Log("Добавлена клетка " + aroundCells[aroundCellsIndex]);

                aroundCellsIndex++;

                Debug.Log("новый индекс = " + aroundCellsIndex);

            }
            for (int z = (int)newStartPoint.z + stepByZ; z != newMaxZ; z += stepByZ)
            {
                Debug.Log("Второй цикл");

                aroundCells[aroundCellsIndex] = new Vector3(newStartPoint.x, newStartPoint.y, z);

                Debug.Log("Добавлена клетка " + aroundCells[aroundCellsIndex]);


                aroundCellsIndex++;


                Debug.Log("новый индекс = " + aroundCellsIndex);

            }
            for (int z = (int)newStartPoint.z + stepByZ; z != newMaxZ; z += stepByZ)
            {
                Debug.Log("Третий цикл");

                aroundCells[aroundCellsIndex] = new Vector3(newMaxX - stepByX, newStartPoint.y, z);

                Debug.Log("Добавлена клетка " + aroundCells[aroundCellsIndex]);

                aroundCellsIndex++;

                Debug.Log("новый индекс = " + aroundCellsIndex);

            }
            for (int x = (int)newStartPoint.x + stepByX; x != newMaxX - stepByX; x += stepByX)
            {
                Debug.Log("Четвертый цикл");

                aroundCells[aroundCellsIndex] = new Vector3(x, newStartPoint.y, newMaxZ - stepByZ);


                Debug.Log("Добавлена клетка " + aroundCells[aroundCellsIndex]);

                aroundCellsIndex++;

                Debug.Log("новый индекс = " + aroundCellsIndex);

            }


        return aroundCells;

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


    private bool IsUnitOnEdge(BattleSlot unit, UnitTable position)
    {
        Debug.Log("Проверяется на грани юнит " + unit.id);

        Debug.Log("Его центр " + unit.center);

        Cell[] unitEdgeCells = gridTable.GetRange(unit.center, 1, false);

        for (int index = 0; index < unitEdgeCells.Length; index++)
        {
            if (unitEdgeCells[index] != null) Debug.Log("Крайние клетки: " + unitEdgeCells[index].cellPose);

        }

        foreach (BattleSlot enemy in position.collections[position.GetEnemyUnitTagByTag(unit.tag)])
        {

            Debug.Log("Возможный атакующий: " + enemy.id);
            Debug.Log("Позиция атакующего: " + enemy.center);

            Cell[] enemyBlustCells = gridTable.GetRange(enemy.center, enemy.distance, true);

            for (int index = 0; index < enemyBlustCells.Length; index++)
            {
                if (enemyBlustCells[index] != null) Debug.Log("Атакующая клетка: " + enemyBlustCells[index].cellPose);

            }


            IEnumerable<Cell> intersectRes = enemyBlustCells.Intersect(unitEdgeCells);

            foreach (Cell cell in intersectRes)
            {
                if (cell != null)
                {
                    Debug.Log("пересечение в клетке " + cell.cellPose);
                    return true;
                }
                    
            }
        }
        return false;
    }

    private void SetTurnAsBest(TurnData turn, UnitTable turnPosition, bool attackingMove = false)
    {
        positionInBestMove = turnPosition;
        bestMove = turn;
        bestEvaluation = positionInBestMove.positionEvaluation * AISide;
        bestMoveIsAttackingMoving = attackingMove;
        if (turn.action == 2) nearestDistanceToEnemy = positionInBestMove.attackersInfo[turn.activeUnit.id].distanceToNearestEnemyUnit;
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

