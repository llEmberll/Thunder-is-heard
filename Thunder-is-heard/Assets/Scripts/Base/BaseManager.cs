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
        Debug.Log("������ ������");
    }


    private void MouseOnCellEnter(Cell cell, bool render)
    {
        Debug.Log("�� ������ ��������");
    }

    private void CreatePreview(int id)
    {
        Debug.Log("������ " + id + " �������");
    }

    private void DeletePreview()
    {
        Debug.Log("������ �������");
    }

}
