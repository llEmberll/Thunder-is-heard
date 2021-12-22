using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour, IPointerClickHandler
{

    [SerializeField] private Color _standartColor;
    [SerializeField] private Color _hoverColor;

    [SerializeField] private MeshRenderer _meshRenderer;

    [SerializeField] public Vector3 cellPose;
    public GameObject occypier;
    public int type;


    private void Awake()
    {
        occypier = null;

        EventMaster.current.ObjectDestroyed += ObjectDestroyed;
        EventMaster.current.AddedObjectToScene += NewObject;
        EventMaster.current.MoveUnitStarts += UnitStartsMove;
        EventMaster.current.CompleteUnitMove += UnitCompleteMove;

        this.cellPose = transform.position;

        this.type = 0;

    }


    private void NewObject(GameObject obj, Vector3[] occypiedPoses)
    {
        foreach (Vector3 pose in occypiedPoses)
        {
            if (cellPose == pose)
            {
                occypyCell(obj);
                break;
            }
        }
    }

    private void ObjectDestroyed(GameObject obj, Vector3[] occypiedPoses)
    {
        foreach (Vector3 pose in occypiedPoses)
        {
            if (cellPose == pose)
            {
                freeCell();
                break;
            }
        }
    }

    private void UnitCompleteMove(GameObject unit, int unitId, Vector3[] unitPoses)
    {
        if (unitPoses.Contains(cellPose))
        {
            occypyCell(unit);
        }
    }

    private void UnitStartsMove(Unit unit, int unitId, Vector3 unitPose)
    {
        if (unitPose == cellPose)
        {
            freeCell();
        }
    }


    public void occypyCell(GameObject obj)
    {
        occypier = obj;
        renderSwitch(false);
        EventMaster.current.AddedObjectToScene -= NewObject;
        EventMaster.current.CompleteUnitMove -= UnitCompleteMove;
    }

    public void freeCell()
    {
        occypier = null;
        EventMaster.current.AddedObjectToScene += NewObject;
        EventMaster.current.CompleteUnitMove += UnitCompleteMove;
    }

    public void changeType(int newType)
    {
        type = newType;
    }

    public void ChangeColor(Color color)
    {
        if (_meshRenderer != null)
        {
            _meshRenderer.material.color = color;
        }
        
    }

    public void StandartColor()
    {
        if (_meshRenderer != null)
        {
            _meshRenderer.material.color = _standartColor;
        }
        
    }

    public void ChangeMaterial(Material mat)
    {
        if (_meshRenderer != null)
        {
            _meshRenderer.material = mat;
        }
        
    }

    public void renderSwitch(bool render)
    {
        if (_meshRenderer != null)
        {
            _meshRenderer.enabled = render;
        }
    }

   
    private void OnMouseEnter()
    {
        ChangeColor(_hoverColor);
        EventMaster.current.MouseOnCellEntered(this.GetComponent<Cell>(), _meshRenderer.enabled);
    }

    private void OnMouseExit()
    {
        ChangeColor(_standartColor);
        EventMaster.current.MouseOnCellExited(this.GetComponent<Cell>(), _meshRenderer.enabled);
    }

    public void OnPointerClick(PointerEventData eventData)
    {

        EventMaster.current.ClickOnCell(this.gameObject.GetComponent<Cell>(), occypier);
        

    }

}
