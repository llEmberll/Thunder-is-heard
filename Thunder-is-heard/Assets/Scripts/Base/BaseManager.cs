using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseManager : MonoBehaviour
{
    public int baseMode;

    private void Awake()
    {
        EventMaster.current.ClickedOnCell += clickedOnCell;
        EventMaster.current.MouseOnCellEnter += MouseOnCellEnter;
        EventMaster.current.CreatedPreview += CreatePreview;
        EventMaster.current.DeletedPreview += DeletePreview;
    }

    private void clickedOnCell(Cell cell, GameObject occypier)
    {
        Debug.Log("Клетка нажата");
    }


    private void MouseOnCellEnter(Cell cell, bool render)
    {
        Debug.Log("На клетку навелись");
    }

    private void CreatePreview(int id)
    {
        Debug.Log("Превью " + id + " создано");
    }

    private void DeletePreview()
    {
        Debug.Log("Превью удалено");
    }

}
