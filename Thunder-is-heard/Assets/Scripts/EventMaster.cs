using System;
using UnityEngine;


public class EventMaster : MonoBehaviour
{
    public static EventMaster current;

    private void Awake()
    {
        current = this;
    }


    public event Action<GameObject, Vector3[]> ObjectDestroyed;
    public void ObjectDestroy(GameObject obj, Vector3[] occypiedPoses)
    {
        ObjectDestroyed?.Invoke(obj, occypiedPoses);
    }


    public event Action<bool> AllPartyUnitDowned;
    public void AllPartyUnitDown(bool enemyUnitsDown)
    {
        AllPartyUnitDowned?.Invoke(enemyUnitsDown);
    }


    public event Action<bool> FightIsOver;
    public void FightFinishing(bool playerWon)
    {
        FightIsOver?.Invoke(playerWon);
    }

    public event Action<bool> ChangeTurn;
    public void TurnChanging(bool playerTurn)
    {
        ChangeTurn?.Invoke(playerTurn);
    }


    public event Action<bool> ChangeStatusTurn;
    public void StatusTurnChanging(bool idle)
    {
        ChangeStatusTurn?.Invoke(idle);
    }

    public event Action<Vector3, Material, bool> SelectedObject;
    public void ObjectSelected(Vector3 pose, Material material, bool render)
    {
        SelectedObject?.Invoke(pose, material, render);
    }

    public event Action<Vector3> CellRequestByPose;
    public void CellRequestionByPose(Vector3 pose)
    {
        CellRequestByPose?.Invoke(pose);
    }

    public event Action<Cell> CellSend;
    public void CellSending(Cell cell)
    {
        CellSend?.Invoke(cell);
    }

    public event Action<Cell, GameObject> ClickedOnCell;
    public void ClickOnCell(Cell cell, GameObject occypier)
    {
        ClickedOnCell?.Invoke(cell, occypier);

    }


    public event Action<int, Cell[]> UnitMoveOnRoute;
    public void UnitMovingOnRoute(int unitId, Cell[] route)
    {
        UnitMoveOnRoute?.Invoke(unitId, route);
    }

    public event Action<Cell, bool> MouseOnCellEnter;
    public void MouseOnCellEntered(Cell cell, bool render)
    {
        MouseOnCellEnter?.Invoke(cell, render);
    }


    public event Action<Cell, bool> MouseOnCellExit;
    public void MouseOnCellExited(Cell cell, bool render)
    {
        MouseOnCellExit?.Invoke(cell, render);
    }

    public event Action<Cell, Cell, bool> SelectedCellForRoute;
    public void SelectCellForRoute(Cell cell, Cell previousCell, bool overPoint)
    {
        SelectedCellForRoute?.Invoke(cell, previousCell, overPoint);
    }

    public event Action ClearRoute;
    public void ClearingRoute()
    {
        ClearRoute?.Invoke();
    }

    public event Action<Cell, Cell> ClearRouteCell;
    public void ClearingRouteCell(Cell clearedCell, Cell previousCell)
    {
        ClearRouteCell?.Invoke(clearedCell, previousCell);
    }


    public event Action<BattleSlot, BattleSlot, Vector3, int> UnitAttacks;
    public void UnitAttacking(BattleSlot attacker, BattleSlot defender, Vector3 attackPoint, int damage)
    {
        UnitAttacks?.Invoke(attacker, defender, attackPoint, damage);
    }



    public event Action<GameObject, int, Vector3[]> CompleteUnitMove;
    public void UnitMovingComplete(GameObject unit, int unitId, Vector3[] unitPoses)
    {
        CompleteUnitMove?.Invoke(unit, unitId, unitPoses);
    }


    public event Action FightIsStarted;
    public void StartFight()
    {
        FightIsStarted?.Invoke();
    }


    public event Action<int, int> UnitAddedToPlayer;
    public void AddUnitToPlayer(int unitId, int count)
    {
        UnitAddedToPlayer?.Invoke(unitId, count);
    }


    public event Action<int, int> UnitDeletedFromPlayer;
    public void DeleteUnitFromPlayer(int unitId, int count)
    {
        UnitDeletedFromPlayer?.Invoke(unitId, count);
    }


    public event Action<int, bool> UnitStatusChanged;
    public void ChangeUnitStatus(int unitId, bool status)
    {
        UnitStatusChanged?.Invoke(unitId, status);
    }


    public event Action<Transform> turnOnSetObjectMode;
    public void turningOnSetObjectMode(Transform obj)
    {
        turnOnSetObjectMode?.Invoke(obj);  
    }

    public event Action<int> CreatedPreview;
    public void CreatePreview(int previewId)
    {
        CreatedPreview?.Invoke(previewId);
    }

    public event Action DeletedPreview;
    public void DeletePreview()
    {
        DeletedPreview?.Invoke();
    }


    public event Action<Cell, int> SpawnedUnit;
    public void SpawnUnit(Cell cell, int buttonId)
    {
        SpawnedUnit?.Invoke(cell, buttonId);
    }


    public event Action<GameObject, Vector3[]> AddedObjectToScene;
    public void SceneAddObject(GameObject obj, Vector3[] occypiedPoses)
    {
        AddedObjectToScene?.Invoke(obj, occypiedPoses);
    }

    public event Action<Unit, int, Vector3> MoveUnitStarts;
    public void StartUnitMove(Unit unit, int unitId, Vector3 unitPose)
    {
        MoveUnitStarts?.Invoke(unit, unitId, unitPose);
    }


    public event Action<GameObject, Vector3, Vector3> ChangePreviewPose;
    public void PreviewPoseChanging(GameObject preview, Vector3 oldPose, Vector3 newPose)
    {
        ChangePreviewPose?.Invoke(preview, oldPose, newPose);
    }

    public event Action<GameObject, int> ObjectChangedHealth;
    public void ObjectHealthChange(GameObject obj, int newHealth)
    {
        ObjectChangedHealth?.Invoke(obj, newHealth);
    }

}
