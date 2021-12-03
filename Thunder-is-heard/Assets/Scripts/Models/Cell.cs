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
    public bool IsOccypy;
    public int type;

    public void Start()
    {
        EventMaster.current.UnitDies += ObjectDestroyed;
        //EventMaster.current.SelectedObject += ObjectOnCellSelected;
        

        this.cellPose = transform.position;
        updateCellStatus();
        this.type = 0;
    }

    //private void ObjectOnCellSelected(Vector3 pose, Material material, bool nowSelect)
    //{
    //    if (cellPose == pose)
    //    {
    //        if (nowSelect)
    //        {
    //            renderOn();
    //            ChangeMaterial(material);
    //        }
    //        else
    //        {
    //            if(_meshRenderer != null)
    //            {
    //                ChangeMaterial(material);
    //                renderOff();
    //                return;
    //            }

    //        }
            
    //    }
    //}

    private void ObjectDestroyed(Unit unit)
    {
        if (cellPose == unit.transform.position)
        {
            freeCell();
            StandartColor();
        }
    }

    private bool findObjectOnCell(Collider[] cols)
    {
        foreach (Collider col in cols)
        {
            if (col.tag != "Cell" && col.tag != "Terrain")
            {
                if (col.tag == "EnemyBuild" || col.tag == "FriendlyBuild")
                {
                    EventMaster.current.ObjectFindedOnCell(col.GetComponent<Build>().id, GetComponent<Cell>());
                }

                return true;
            }
        }
        return false;
    }

    public void updateCellStatus()
    {
        IsOccypy = findObjectOnCell(Physics.OverlapSphere(cellPose, 0.2f));
    }


    public int GetOccypyInfo()
    {
        Collider[] occypyers = Physics.OverlapSphere(cellPose, 0.2f);
        foreach (Collider obj in occypyers)
        {
            if (obj.tag != "Cell" && obj.tag != "Terrain")
            {
                Debug.Log("Найден оккупант клетки");
                switch (obj.tag)
                {
                    case "FriendlyUnit":
                        return 1;
                    case "Enemyunit":
                        return 2;
                    case "FriedlyBuild":
                        return 3;
                    case "EnemyBuild":
                        return 4;
                    default:
                        return 0;
                }
            }
        }
        return 0;
    }

    public void OccypyCell()
    {
        IsOccypy = true;
    }

    public void freeCell()
    {
        IsOccypy = false;
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

        EventMaster.current.ClickOnCell(this.gameObject.GetComponent<Cell>());
        

    }

}
