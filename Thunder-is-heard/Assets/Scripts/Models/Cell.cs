using System.Collections;
using System.Collections.Generic;
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
        EventMaster.current.UnitDies += UnitDown;
        EventMaster.current.BuildDestroed += BuildDestroyed;
        EventMaster.current.AddedUnitToScene += NewUnit;
        EventMaster.current.AddedIndestructibleToScene += NewIndestructible;
        EventMaster.current.NewBuildOccypyPose += NewBuild;
        EventMaster.current.MoveUnitStarts += UnitStartsMove;
        EventMaster.current.CompleteUnitMove += UnitCompleteMove;
        EventMaster.current.IndestructibleDestroyed += DestroyIndestructible;
        EventMaster.current.ChangePreviewPose += PreviewChangePose;

        this.cellPose = transform.position;

        Debug.Log("Cellpose " + cellPose.x + " || " + cellPose.z);

        this.type = 0;

    }

    public void Start()
    { 
        
        
    }

    private void UnitCompleteMove(Unit unit, int unitId, Vector3 unitPose)
    {
        if (unitPose == cellPose)
        {
            occypyCell(unit.gameObject);
        }
    }

    private void PreviewChangePose(GameObject preview, Vector3 oldPose, Vector3 newPose)
    {
        Debug.Log("Cell event");

        if (cellPose == oldPose)
        {
            Debug.Log("cellpose == oldPose => freeCell");

            freeCell();
            renderOn();
            return;
        }
        if (cellPose == newPose && occypier == null)
        {
            Debug.Log("cellpose == newPose && occ == null => occypCell");

            occypyCell(preview);
        }
    }

    private void UnitStartsMove(Unit unit, int unitId, Vector3 unitPose)
    {
        if (unitPose == cellPose)
        {
            freeCell();
        }
    }

    private void NewIndestructible(Indestructible element)
    {
        if (cellPose == element.transform.position)
        {
            occypyCell(element.gameObject);
        }
    }

    private void DestroyIndestructible(Indestructible element)
    {
        if (cellPose == element.transform.position)
        {
            freeCell();
        }
    }
    private void NewUnit(Unit unit, bool enemy)
    {
        if (unit.transform.position == cellPose)
        {
            occypyCell(unit.gameObject);
        }
    }


    private void NewBuild(GameObject build, Vector3 pose)
    {
        if (pose == cellPose)
        {
            occypyCell(build);
        }
    }

    private void BuildDestroyed(Build build, Vector3[] occypyPoses)
    {
        foreach (Vector3 pose in occypyPoses)
        {
            if (pose == cellPose)
            {
                freeCell();
            }
        }
    }

    private void UnitDown(Unit unit)
    {
        if (cellPose == unit.transform.position)
        {
            freeCell();
        }
    }

    public void occypyCell(GameObject obj)
    {
        occypier = obj;
        renderOff();
        EventMaster.current.AddedUnitToScene -= NewUnit;
        EventMaster.current.NewBuildOccypyPose -= NewBuild;
        EventMaster.current.BuildDestroed -= BuildDestroyed;
        EventMaster.current.IndestructibleDestroyed -= DestroyIndestructible;
        EventMaster.current.AddedIndestructibleToScene -= NewIndestructible;
    }

    public void freeCell()
    {
        occypier = null;
        EventMaster.current.AddedUnitToScene += NewUnit;
        EventMaster.current.NewBuildOccypyPose += NewBuild;
        EventMaster.current.BuildDestroed += BuildDestroyed;
        EventMaster.current.IndestructibleDestroyed += DestroyIndestructible;
        EventMaster.current.AddedIndestructibleToScene += NewIndestructible;
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

    public void renderOff()
    {
        if (_meshRenderer != null)
        {
            _meshRenderer.enabled = false;
        }
        
    }

    public void renderOn()
    {
        if (_meshRenderer != null)
        {
            _meshRenderer.enabled = true;
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
