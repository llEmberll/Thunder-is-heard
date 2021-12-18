using System;
using UnityEngine;


public class EventMaster : MonoBehaviour
{
    public static EventMaster current;

    private void Awake()
    {
        current = this;
    }

    public event Action<Unit> UnitDies;
    public void UnitDying(Unit unit)
    {
        UnitDies?.Invoke(unit);
    }

    public event Action<Build, Vector3[]> BuildDestroed;
    public void BuildDestroing(Build build, Vector3[] occypiedPoses)
    {
        BuildDestroed?.Invoke(build, occypiedPoses);
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


    public event Action<int, Cell> FindObjectOnCell;
    public void ObjectFindedOnCell(int objId, Cell cell)
    {
        FindObjectOnCell?.Invoke(objId, cell);
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


    public event Action<Vector3, Cell[]> UnitMoveOnRoute;
    public void UnitMovingOnRoute(Vector3 UnitPose, Cell[] route)
    {
        UnitMoveOnRoute?.Invoke(UnitPose, route);
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


    public event Action<Unit, Unit, int> UnitAttacksUnit;
    public void UnitAttackingUnit(Unit Attacker, Unit Defender, int Damage)
    {
        UnitAttacksUnit?.Invoke(Attacker, Defender, Damage);
    }

    public event Action<Unit, Build, int> UnitAttacksBuild;
    public void UnitAttackingBuild(Unit Attacker, Build Defender, int Damage)
    {
        UnitAttacksBuild?.Invoke(Attacker, Defender, Damage);
    }



    public event Action<Unit, int, Vector3> CompleteUnitMove;
    public void UnitmovingComplete(Unit unit, int unitId, Vector3 unitPose)
    {
        CompleteUnitMove?.Invoke(unit, unitId, unitPose);
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

    public event Action<Unit, bool> AddedUnitToScene; 
    public void SceneAddUnit(Unit unit, bool enemy)
    {
        AddedUnitToScene?.Invoke(unit, enemy);
    }

    public event Action<Build, bool> AddedBuildToScene;
    public void SceneAddBuild(Build build, bool enemy)
    {
        AddedBuildToScene?.Invoke(build, enemy);
    }

    public event Action<Unit, int, Vector3> MoveUnitStarts;
    public void StartUnitMove(Unit unit, int unitId, Vector3 unitPose)
    {
        MoveUnitStarts?.Invoke(unit, unitId, unitPose);
    }

    public event Action<GameObject, Vector3> NewBuildOccypyPose;
    public void SendNewBuildOccypyPose(GameObject build, Vector3 pose)
    {
        NewBuildOccypyPose?.Invoke(build, pose);
    }

    public event Action<Indestructible> AddedIndestructibleToScene;
    public void SceneAddIndestructible(Indestructible element)
    {
        AddedIndestructibleToScene?.Invoke(element);
    }

    public event Action<Indestructible> IndestructibleDestroyed;
    public void BuildDestroing(Indestructible element)
    {
        IndestructibleDestroyed?.Invoke(element);
    }

    public event Action<GameObject, Vector3, Vector3> ChangePreviewPose;
    public void PreviewPoseChanging(GameObject preview, Vector3 oldPose, Vector3 newPose)
    {
        ChangePreviewPose?.Invoke(preview, oldPose, newPose);
    }
}
