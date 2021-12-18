using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private Material defaultCellMaterial;
    [SerializeField] private Material previewMaterial;

    [SerializeField] private Image damage, health;
    [SerializeField] private Text name, damageCount, healthCount;
    [SerializeField] private Canvas UI, distanceLine;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Color friendlyHealthColor, enemyHealthColor;
    [SerializeField] private SpriteRenderer friendlySelector, enemySelector;

    [SerializeField] private Vector2 unitHealthBarSize, buildHealthBarSize;
    [SerializeField] private float UIUnitPoseByY, UIBuildPoseByY;



    [SerializeField] private GameObject overRouteImage;
    [SerializeField] private GameObject routeImage;

    [SerializeField] private Button pass;

    [SerializeField] private Image fightButtons;
    [SerializeField] private Image fightPrepareButtons;

    [SerializeField] private Image preparePanel;

    private List<GameObject> allRoute;

    private bool playerTurn;
    private bool idle;

    private GameObject _preview;
    private int previewId;
    private float previewOffsetByY;



    private Transform _selection;

    private Prefabs dataBase;

    private void Awake()
    {
        EventMaster.current.ChangeTurn += ChangeTurn;
        EventMaster.current.ChangeStatusTurn += ChangeStatusTurn;
        EventMaster.current.UnitDies += UnitDead;
        EventMaster.current.BuildDestroed += BuildDestroy;
        EventMaster.current.CellSend += GetCell;
        EventMaster.current.SelectedCellForRoute += CellForRoute;
        EventMaster.current.ClearRoute += ClearRouteCells;
        EventMaster.current.ClearRouteCell += ClearRouteCell;
        EventMaster.current.FightIsStarted += StartFight;
        EventMaster.current.CreatedPreview += CreatePreview;
        EventMaster.current.SpawnedUnit += SpawnUnit;
        EventMaster.current.DeletedPreview += DeletePreview;

        dataBase = GameObject.FindGameObjectWithTag("Prefabs").GetComponent<Prefabs>();
    }

    private void Start()
    {
        
        
        fightPrepareButtons.enabled = preparePanel.enabled = true;
        fightButtons.enabled = false;

        allRoute = new List<GameObject>();

        UI.enabled = distanceLine.enabled = enemySelector.enabled = friendlySelector.enabled = false;
        damage.gameObject.SetActive(false); health.gameObject.SetActive(false);
    }


    private void StartFight()
    {
        Debug.Log("Start fight â SelMan");

        Destroy(fightPrepareButtons.gameObject);
        Destroy(preparePanel.gameObject);
        fightButtons.enabled = true;
        _preview = null;

    }

    private void SpawnUnit(Cell cell, int id)
    {
        UnitData unitData = dataBase.GetUnitData(previewId);
        GameObject unit = new GameObject();

        unit.transform.position = cell.cellPose;

        Unit unitClass = unit.AddComponent<Unit>();
       
        GameObject unitMesh = Instantiate(unitData.prefab, (unit.transform.position + unitData.meshPose), Quaternion.identity, parent: unit.transform);

        FillUnitParams(unitClass, unitData, unitMesh);
    }

    private void FillUnitParams(Unit unit, UnitData data, GameObject mesh)
    {
        mesh.transform.localScale = data.meshScale;
        unit.model = mesh.transform;

        unit.unitName = data.name;
        unit.unitId = data.id;
        unit.maxHealth = data.maxHealth;
        unit.damage = data.damage;
        unit.distance = data.distance;
        unit.mobility = data.mobility;
        unit.realSpeed = data.realSpeed;

        unit.tag = "FriendlyUnit";
    }

    private void DeletePreview()
    {
        Destroy(_preview.gameObject);
        _preview = null;
    }

    private void CreatePreview(int id)
    {
        UnitData unitData = dataBase.GetUnitData(id);
        previewId = id;
        previewOffsetByY = unitData.meshPose.y;

        _preview = Instantiate(unitData.preview, new Vector3(0, previewOffsetByY, 0), Quaternion.identity);
        _preview.transform.localScale = unitData.meshScale;

    }

    private void UpdatePreview(Transform selection)
    {
        Vector3 newPose = selection.transform.position;
        _preview.transform.position = new Vector3(newPose.x, previewOffsetByY, newPose.z);

        if (selection.GetComponent<Cell>().occypier != null)
        {
            ChangePreviewColor(0.3f);
        }
        else
        {
            ChangePreviewColor(0);
        }

    }

    private void ChangePreviewColor(float alpha)
    {
        Color previewColor = previewMaterial.color;

        previewColor.a = alpha;

        previewMaterial.color = previewColor;
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
        imagePose.y = 0.047f;

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

    private void GetCell(Cell cell)
    {
        cell.ChangeMaterial(defaultCellMaterial);
    }

    private void BuildDestroy(Build build, Vector3[] occypyPoses)
    {
        if (_selection != null)
        {
            Vector3 selectionPose = _selection.transform.position;
            if (occypyPoses.Contains(selectionPose))
            {
                TurnOffUI();
            }
        }
    }

    private void UnitDead(Unit unit)
    {
        if (_selection != null)
        {
            if (unit.transform.position == _selection.transform.position)
            {
                TurnOffUI();
            }
        }
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

    private void TurnOffUI()
    {
        UI.enabled = distanceLine.enabled = enemySelector.enabled = friendlySelector.enabled = false;
    }

    void FixedUpdate()
    {
        if (_selection != null)
        {
            Deselection();
        }
        RaycastAction();
        
    }

    private void SetUnitUI(Transform selection, Unit unit, Color healthColor, SpriteRenderer selector)
    {
        Vector3 selectionPose = selection.transform.position;
        name.text = unit.unitName;

        healthBar.GetComponent<RectTransform>().sizeDelta = unitHealthBarSize;
        healthBar.minValue = 0;
        healthBar.maxValue = unit.maxHealth;
        healthBar.value = unit.health;
        healthBarFill.color = healthColor;

        enemySelector.enabled = friendlySelector.enabled = false;
        selector.enabled = true;
        selector.transform.position = new Vector3(selectionPose.x, 0.047f, selectionPose.z);
        selector.transform.localScale = new Vector3(0.14f, 0.14f, 0.14f);

        distanceLine.enabled = true;
        distanceLine.transform.position = new Vector3(selectionPose.x, 0.045f, selectionPose.z);
        float RangeSize = 800 + (1600 * unit.distance);
        distanceLine.GetComponent<RectTransform>().sizeDelta = new Vector2(RangeSize, RangeSize);

        health.gameObject.SetActive(true);
        health.enabled = true;

        damage.gameObject.SetActive(true);
        damage.enabled = true;

        healthCount.text = $"{unit.maxHealth}";
        damageCount.text = $"{unit.damage}";
    }

    private void SetBuildUI(Transform selection, Build build, Color healthColor, SpriteRenderer selector)
    {
        name.text = build.buildName;
        healthBar.GetComponent<RectTransform>().sizeDelta = buildHealthBarSize;
        healthBar.minValue = 0;
        healthBar.maxValue = build.maxHealth;
        healthBar.value = build.health;
        healthBarFill.color = healthColor;

        enemySelector.enabled = friendlySelector.enabled = false;
        selector.enabled = true;

        selector.transform.position = new Vector3(build.center.x, 0.047f, build.center.z);

        float maxBuildSize = 0.14f * (Mathf.Max(build.sizeX, build.sizeZ));

        selector.transform.localScale = new Vector3(maxBuildSize, maxBuildSize, maxBuildSize);

        distanceLine.enabled = false;

        damage.enabled = false;
        damage.gameObject.SetActive(false);

        health.enabled = true;
        health.gameObject.SetActive(true);

        
    }

    private void SetUIPose(Vector3 pose)
    {
        UI.enabled = true;
        UI.transform.position = pose;
    }

    private void RaycastAction()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var selection = hit.transform;
            switch (selection.tag)
            {
                case "Cell":
                    if (_preview != null)
                    {
                        UpdatePreview(selection);
                    }

                    else
                    {
                        Cell cell = selection.GetComponent<Cell>();
                        GameObject selectionObject = cell.occypier;

                        if (selectionObject != null)
                        {
                            switch (selectionObject.tag)
                            {
                                case "FriendlyUnit":
                                    SetUIPose(new Vector3(selectionObject.transform.position.x, UIUnitPoseByY, selectionObject.transform.position.z));
                                    SetUnitUI(selection, selectionObject.GetComponent<Unit>(), friendlyHealthColor, friendlySelector);
                                    _selection = selection;
                                    break;
                                case "EnemyUnit":
                                    SetUIPose(new Vector3(selectionObject.transform.position.x, UIUnitPoseByY, selectionObject.transform.position.z));
                                    SetUnitUI(selection, selectionObject.GetComponent<Unit>(), enemyHealthColor, enemySelector);
                                    _selection = selection;
                                    break;
                                case "FriendlyBuild":
                                    Build friendlyBuild = selectionObject.GetComponent<Build>();
                                    SetUIPose(new Vector3(friendlyBuild.center.x, UIBuildPoseByY, friendlyBuild.center.z));
                                    SetBuildUI(selection, selectionObject.GetComponent<Build>(), friendlyHealthColor, friendlySelector);
                                    _selection = selection;
                                    break;
                                case "EnemyBuild":
                                    Build enemyBuild = selectionObject.GetComponent<Build>();
                                    SetUIPose(new Vector3(enemyBuild.center.x, UIBuildPoseByY, enemyBuild.center.z));
                                    SetBuildUI(selection, selectionObject.GetComponent<Build>(), enemyHealthColor, enemySelector);
                                    _selection = selection;
                                    break;
                                case "Destructible":
                                    break;
                            }
                        }
                    }
                    break;
            }
        }
    }

    private void Deselection()
    {
        if (_selection.CompareTag("Cell"))
        {
            GameObject obj = _selection.GetComponent<Cell>().occypier;
            if (obj != null)
            {
                if (obj.CompareTag("FriendlyUnit") || obj.CompareTag("EnemyUnit") || obj.CompareTag("EnemyBuild") || obj.CompareTag("FriendlyBuild"))
                {
                    TurnOffUI();
                }
            }
            
            else
            {
                var selectionRenderer = _selection.GetComponent<Renderer>();
                selectionRenderer.material = defaultCellMaterial;
            }  
        }
        _selection = null;
    }
}
