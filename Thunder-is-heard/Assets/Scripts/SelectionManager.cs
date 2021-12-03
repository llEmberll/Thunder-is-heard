using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private Material CellForOverRouteMaterial;
    [SerializeField] private Material CellForRouteMaterial;
    [SerializeField] private Material selectionMaterial;
    [SerializeField] private Material selectionEnemyMaterial;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private GameObject overRouteImage;
    [SerializeField] private GameObject routeImage;

    [SerializeField] private Button pass;

    private List<GameObject> allRoute;

    private bool playerTurn;
    private bool idle;

    private Transform _selection;

    private void Start()
    {
        EventMaster.current.ChangeTurn += ChangeTurn;
        EventMaster.current.ChangeStatusTurn += ChangeStatusTurn;
        EventMaster.current.UnitDies += UnitDead;
        EventMaster.current.CellSend += getCell;
        EventMaster.current.SelectedCellForRoute += CellForRoute;
        EventMaster.current.ClearRoute += ClearRouteCells;
        EventMaster.current.ClearRouteCell += ClearRouteCell;

        allRoute = new List<GameObject>();
    }

    private void ClearRouteCell(Cell clearedCell, Cell previousCell)
    {
        GameObject[] Bucket = new GameObject[allRoute.Count];
        int countForDelete = 0;

        foreach (GameObject currentElement in allRoute)
        {
            if (currentElement != null)
            {
                if (equalPose(currentElement.transform.position, clearedCell.cellPose) || PointOnLine(currentElement.transform.position, previousCell.cellPose, clearedCell.cellPose))
                {
                    Bucket[countForDelete] = currentElement;
                    countForDelete++;
                }
            }
        }

        DeleteFromRoute(Bucket);

    }

    private bool equalPose(Vector3 imagePose, Vector3 cellPose)
    {
        Vector3 pointPose = imagePose;
        pointPose.y = 0;

        if (pointPose == cellPose) return true;
        return false;
    }

    private bool PointOnLine(Vector3 point, Vector3 startLine, Vector3 endLine)
    {
        Vector3 pointPose = point;
        Vector3 routeImageOffset = (endLine - startLine) / 3;

        pointPose.y = 0;

        if (Vector3.Distance(pointPose, endLine - routeImageOffset) <= 0.1 || Vector3.Distance(pointPose, endLine - routeImageOffset - routeImageOffset) <= 0.1)
        {
            return true;
        }
        return false;
    }
    

    private void ClearRouteCells()
    {
        for (int index = 0; index < allRoute.Count; index++)
        {
            Destroy(allRoute[index]);
        }
        allRoute.Clear();
    }
    private void CellForRoute(Cell cell, Cell previousCell, bool overPoint)
    {
        Vector3 imagePose = cell.cellPose;
        imagePose.y = 0.01f;

        if (!overPoint)
        {
            addToRoute(Instantiate(routeImage, imagePose, Quaternion.Euler(new Vector3(90f, 0f, 0f))));
        }
        else
        {
            addToRoute(Instantiate(overRouteImage, imagePose, Quaternion.Euler(new Vector3(90f, 0f, 0f))));
        }

        Vector3 routeImageOffset = (previousCell.cellPose - cell.cellPose) / 3;

        addToRoute(
            Instantiate(routeImage, imagePose + routeImageOffset, Quaternion.Euler(new Vector3(90f, 0f, 0f))), 
            Instantiate(routeImage, imagePose + (routeImageOffset * 2), Quaternion.Euler(new Vector3(90f, 0f, 0f)))
            );
    }

    private void addToRoute(params GameObject[] newRouteParts)
    {
        foreach(GameObject part in newRouteParts)
        {
            allRoute.Add(part);
        }
    }

    private void DeleteFromRoute(params GameObject[] newRouteParts)
    {
        foreach (GameObject part in newRouteParts)
        {
            allRoute.Remove(part);
            Destroy(part);
        }
    }

    private void getCell(Cell cell)
    {
        cell.ChangeMaterial(defaultMaterial);
    }

    private void UnitDead(Unit unit)
    {
        EventMaster.current.CellRequestionByPose(unit.transform.position);
    }

    private void ChangeTurn(bool turn)
    {
        playerTurn = pass.enabled = turn;
    }

    private void ChangeStatusTurn(bool status)
    {
        idle = status;
    }


    void FixedUpdate()
    {
        if (_selection != null)
        {

            if (_selection.CompareTag("FriendlyUnit") || _selection.CompareTag("EnemyUnit") || _selection.CompareTag("EnemyBuild"))
            {
                EventMaster.current.ObjectSelected(_selection.transform.position, defaultMaterial, false);
            }
            else
            {
                var selectionRenderer = _selection.GetComponent<Renderer>();
                selectionRenderer.material = defaultMaterial;
            }
            
            _selection = null;
        }
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var selection = hit.transform;

            switch (selection.tag)
            {
                case "Cell":
                    break;

                case "FriendlyUnit":
                    if (idle && playerTurn)
                    {
                        EventMaster.current.ObjectSelected(selection.transform.position, selectionMaterial, true);
                        _selection = selection;
                    }

                    break;
                case "EnemyUnit":

                    EventMaster.current.ObjectSelected(selection.transform.position, selectionEnemyMaterial, true);
                    _selection = selection;

                    break;
                case "FriendlyBuild":
                    break;
                case "EnemyBuild":

                    EventMaster.current.ObjectSelected(selection.transform.position, selectionEnemyMaterial, true);
                    _selection = selection;

                    break;
            }
        }
    }
}
