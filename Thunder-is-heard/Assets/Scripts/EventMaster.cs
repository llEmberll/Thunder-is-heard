using System;
using System.Collections;
using System.Collections.Generic;
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
        if (UnitDies != null)
        {
            UnitDies(unit);
        }
    }

    public event Action<Build, Cell[]> BuildDestroed;
    public void BuildDestroing(Build build, Cell[] occypiedCells)
    {
        if (BuildDestroed != null)
        {
            BuildDestroed(build, occypiedCells);
        }
    }


    public event Action<bool> FightIsOver;
    public void FightFinishing(bool playerWon)
    {
        if (FightIsOver != null)
        {
            FightIsOver(playerWon);
        }
    }

    public event Action<bool> ChangeTurn;
    public void TurnChanging(bool playerTurn)
    {
        if (ChangeTurn != null)
        {
            ChangeTurn(playerTurn);
        }
    }

    public event Action<bool> ChangeStatusTurn;
    public void StatusTurnChanging(bool idle)
    {
        if (ChangeStatusTurn != null)
        {
            ChangeStatusTurn(idle);
        }
    }


    public event Action<Unit> UnitClicked;
    public void UnitClick(Unit unit)
    {
        if (UnitClicked != null)
        {
            UnitClicked(unit);
        }
    }

    public event Action<Build, Cell[]> BuildClicked;
    public void BuildClick(Build build, Cell[] occypiedCells)
    {
        if (BuildClicked != null)
        {
            BuildClicked(build, occypiedCells);
        }
    }

    public event Action<int, Cell> FindObjectOnCell;
    public void ObjectFindedOnCell(int objId, Cell cell)
    {
        if (FindObjectOnCell != null)
        {
            FindObjectOnCell(objId, cell);
        }
    }


    public event Action<Vector3, Material, bool> SelectedObject;
    public void ObjectSelected(Vector3 pose, Material material, bool render)
    {
        if (SelectedObject != null)
        {
            SelectedObject(pose, material, render);
        }
    }

    public event Action<Vector3> CellRequestByPose;
    public void CellRequestionByPose(Vector3 pose)
    {
        if (CellRequestByPose != null)
        {
            CellRequestByPose(pose);
        }
    }

    public event Action<Cell> CellSend;
    public void CellSending(Cell cell)
    {
        if (CellSend != null)
        {
            CellSend(cell);
        }
    }

    public event Action<Cell> ClickedOnCell;
    public void ClickOnCell(Cell cell)
    {
        if (ClickedOnCell != null)
        {
            ClickedOnCell(cell);
        }

    }


    public event Action<Vector3, Cell[]> UnitMoveOnRoute;
    public void UnitMovingOnRoute (Vector3 UnitPose, Cell[] route)
    {
        if (UnitMoveOnRoute != null)
        {
            UnitMoveOnRoute(UnitPose, route);
        }
    }

    public event Action<Cell, bool> MouseOnCellEnter;
    public void MouseOnCellEntered (Cell cell, bool render)
    {
        if (MouseOnCellEnter != null)
        {
            MouseOnCellEnter(cell, render);
        }
    }


    public event Action<Cell, bool> MouseOnCellExit;
    public void MouseOnCellExited(Cell cell, bool render)
    {
        if (MouseOnCellExit != null)
        {
            MouseOnCellExit(cell, render);
        }
    }

    public event Action<Cell, Cell, bool> SelectedCellForRoute;
    public void SelectCellForRoute(Cell cell, Cell previousCell, bool overPoint)
    {
        if (SelectedCellForRoute != null)
        {
            SelectedCellForRoute(cell, previousCell, overPoint);
        }
    }

    public event Action ClearRoute;
    public void ClearingRoute()
    {
        if (ClearRoute != null)
        {
            ClearRoute();
        }
    }

    public event Action<Cell, Cell> ClearRouteCell;
    public void ClearingRouteCell(Cell clearedCell, Cell previousCell)
    {
        if (ClearRouteCell != null)
        {
            ClearRouteCell(clearedCell, previousCell);
        }
    }


    public event Action<Unit, Unit, int> UnitAttacksUnit;
    public void UnitAttackingUnit(Unit Attacker, Unit Defender, int Damage)
    {
        if (UnitAttacksUnit != null)
        {
            UnitAttacksUnit(Attacker, Defender, Damage);
        }
    }

    public event Action<Unit, Build, int> UnitAttacksBuild;
    public void UnitAttackingBuild(Unit Attacker, Build Defender, int Damage)
    {
        Debug.Log("� �������");

        if (UnitAttacksBuild != null)
        {
            Debug.Log("� �������: ����� �� null!");

            Debug.Log("���� ����� - " + Damage);

            UnitAttacksBuild(Attacker, Defender, Damage);
        }
    }



    public event Action CompleteMove;
    public void movingComplete()
    {
        if (CompleteMove != null)
        {
            CompleteMove();
        }
    }
}
