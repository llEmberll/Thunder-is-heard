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

                    Debug.Log("точка ЛЕЖИТ на векторе, утверждено главной проверкой");

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

        Debug.Log("equalPose === точка: x " + pointPose.x + "|| z " + pointPose.z + "|| y " + pointPose.y);
        Debug.Log("equalPose === клетка: x " + cellPose.x + "|| z " + cellPose.z + "|| y " + cellPose.y);

        if (pointPose == cellPose)
        {

            Debug.Log("точка ЛЕЖИТ на клетке, позиции совпадают");

            return true;
        }

        Debug.Log("точка НЕ лежит на клетке, позиции не совпадают");

        return false;
    }

    private bool PointOnLine(Vector3 point, Vector3 startLine, Vector3 endLine)
    {
        Vector3 pointPose = point;
        pointPose.y = 0;

        Vector3 newRoutePart = endLine - startLine;
        Vector3 routeImageOffset = newRoutePart / 3;

        Vector3 startLineAndOffset = endLine - routeImageOffset;

        Vector3 startLineAndOffset2 = endLine - routeImageOffset - routeImageOffset;


        Debug.Log("ПЕРЕД точка: x " + pointPose.x + "|| z " + pointPose.z + "|| y " + pointPose.y);

        Debug.Log("ПЕРЕД startline: x " + endLine.x + "|| z " + endLine.z + "|| y " + endLine.y);

        Debug.Log("ПЕРЕД сдвиг: x " + routeImageOffset.x + "|| z " + routeImageOffset.z + "|| y " + routeImageOffset.y);

        Debug.Log("ПЕРЕД startline+СДВИГ: x " + startLineAndOffset.x + "|| z " + startLineAndOffset.z + "|| y " + startLineAndOffset.y);

        Debug.Log("startline+СДВИГx2: x " + startLineAndOffset2.x + "|| z " + startLineAndOffset2.z + "|| y " + startLineAndOffset2.y);

        

        if (Vector3.Distance(pointPose, startLineAndOffset) <= 0.1 || Vector3.Distance(pointPose, startLineAndOffset2) <= 0.1)
        {

            Debug.Log("точка ЛЕЖИТ на векторе");
            return true;
        }


        else
        {
            Debug.Log("точка НЕ лежит на векторе");


            Debug.Log("Расстояние 1: " + Vector3.Distance(pointPose, startLineAndOffset));
            Debug.Log("Расстояние 2: " + Vector3.Distance(pointPose, startLineAndOffset2));

            return false;
        }
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
        Debug.Log("В selection Manager принята клетка для маршрута");

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

        Vector3 newRoutePart = previousCell.cellPose - cell.cellPose;
        Vector3 routeImageOffset = newRoutePart / 3;

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

            if (_selection.CompareTag("FriendlyUnit") || _selection.CompareTag("EnemyUnit"))
            {


                EventMaster.current.CellUnderUnitSelected(_selection.transform.position, defaultMaterial, false);
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
                    if (idle)
                    {
                        if (playerTurn)
                        {
                            EventMaster.current.CellUnderUnitSelected(selection.transform.position, selectionMaterial, true);
                            _selection = selection;
                        }
                        
                    }

                    break;
                case "EnemyUnit":

                    EventMaster.current.CellUnderUnitSelected(selection.transform.position, selectionEnemyMaterial, true);
                    _selection = selection;

                    break;
                case "FriendlyBuild":
                    break;
                case "EnemyBuild":
                    break;
            }
            

        }
    }
}
