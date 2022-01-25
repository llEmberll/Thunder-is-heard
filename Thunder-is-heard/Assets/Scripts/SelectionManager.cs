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
    [SerializeField] private Text elementName, damageCount, healthCount;
    [SerializeField] private Canvas UI, distanceLine;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Color friendlyHealthColor, enemyHealthColor;
    [SerializeField] private SpriteRenderer friendlySelector, enemySelector, enemyTargetSelector;

    [SerializeField] private SpriteRenderer[] attackerSelectors;

    [SerializeField] private Vector2 unitHealthBarSize, buildHealthBarSize;
    [SerializeField] private float UIUnitPoseByY, UIBuildPoseByY;

    private Dictionary<string, ObjectUI> UIconfigs;

    [SerializeField] private GameObject overRouteImage;
    [SerializeField] private GameObject routeImage;

    [SerializeField] private Button pass;

    [SerializeField] private Image fightButtons;
    [SerializeField] private Image fightPrepareButtons;

    [SerializeField] private Image preparePanel;

    private List<GameObject> allRoute;

    private bool playerTurn;
    private bool idle = true;

    private GameObject _preview;
    private int previewId;
    private float previewOffsetByY;

    private Transform _selection;

    private Prefabs dataBase;
    private UnitTable currentBattlePosition;


    private class ObjectUI
    {
        public Color color;
        public Vector2 healthBarSize;
        public float UIposeByY;
        public SpriteRenderer selector;
        public bool distance;
        public bool health, damage;

        public ObjectUI(Color Color, Vector2 HealthBarSize, float UIPoseByY, SpriteRenderer Selector, bool Distance, bool Health, bool Damage)
        {
            color = Color;
            healthBarSize = HealthBarSize;
            UIposeByY = UIPoseByY;
            selector = Selector;
            distance = Distance;
            health = Health;
            damage = Damage;
        }
    }


    private void Awake()
    {
        EventMaster.current.ChangeTurn += ChangeTurn;
        EventMaster.current.ChangeStatusTurn += ChangeStatusTurn;
        EventMaster.current.ObjectDestroyed += ObjectDestroyed;
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
        currentBattlePosition = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>().currentPosition;

        UIconfigs = new Dictionary<string, ObjectUI>() {
            { currentBattlePosition.friendlyUnitTag, new ObjectUI(friendlyHealthColor, unitHealthBarSize, UIUnitPoseByY, friendlySelector, true, true, true) },
            { currentBattlePosition.enemyUnitTag, new ObjectUI(enemyHealthColor, unitHealthBarSize, UIUnitPoseByY, enemySelector, true, true, true) },
            { currentBattlePosition.friendlyBuildTag, new ObjectUI(friendlyHealthColor, buildHealthBarSize, UIBuildPoseByY, friendlySelector, false, true, false) },
            { currentBattlePosition.enemyBuildTag, new ObjectUI(enemyHealthColor, buildHealthBarSize, UIBuildPoseByY, enemySelector, false, true, false) }
        };

        fightPrepareButtons.enabled = preparePanel.enabled = true;
        fightPrepareButtons.gameObject.SetActive(true);
        preparePanel.gameObject.SetActive(true);

        fightButtons.enabled = false;
        fightButtons.gameObject.SetActive(false);


        allRoute = new List<GameObject>();

        UI.enabled = distanceLine.enabled = enemySelector.enabled = friendlySelector.enabled = enemyTargetSelector.enabled = false;
        damage.gameObject.SetActive(false); health.gameObject.SetActive(false);
    }

    private void StartFight()
    {
        Destroy(fightPrepareButtons.gameObject);
        Destroy(preparePanel.gameObject);

        fightButtons.gameObject.SetActive(true);
        fightButtons.enabled = true;
        DeletePreview();

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

        unit.elementName = data.name;
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
        if (_preview != null)
        {
            Destroy(_preview.gameObject);
            _preview = null;
        }
        
    }

    private void CreatePreview(int id)
    {
        UnitData unitData = dataBase.GetUnitData(id);
        previewId = id;
        previewOffsetByY = unitData.meshPose.y;

        DeletePreview();

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
            if (allRoute[index] != null) Destroy(allRoute[index]);
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

    private void ObjectDestroyed(GameObject obj, Vector3[] poses)
    {
        if (_selection != null)
        {
            if (poses.Contains(_selection.transform.position))
            {
                TurnOffUI();
            }
        }
        hideAttackers();
    }

    private void ChangeTurn(bool turn)
    {
        playerTurn = pass.interactable = turn;
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
        SelectionHandle();
    }

    private void showAttackers(GameObject target)
    {
        Dictionary<BattleSlot, int> targetAttackers = GetAttackers(target);
        if (targetAttackers != null)
        {
            attackerSelectors = new SpriteRenderer[targetAttackers.Count];
            int index = 0;

            foreach (KeyValuePair<BattleSlot, int> item in targetAttackers)
            {
                BattleSlot attacker = item.Key;
                if (attacker != null)
                {
                    var selector = Instantiate(friendlySelector, new Vector3(attacker.center.x, 0.047f, attacker.center.z), Quaternion.identity);
                    selector.enabled = true;

                    selector.name = $"selector-{index}";

                    attackerSelectors[index] = selector;
                    index++;
                }
            }
        }
    }

    private void hideAttackers()
    {
        foreach (SpriteRenderer selector in attackerSelectors)
        {
            if (selector != null)
            {
                Destroy(selector.gameObject);
            }
            
        }
    }

    private Dictionary<BattleSlot, int> GetAttackers(GameObject element)
    {
        int elementId = element.GetComponent<Destructible>().id;
        if (currentBattlePosition.GetEnemyTagsByTag(currentBattlePosition.friendlyUnitTag).Contains(element.tag) && currentBattlePosition.attackersInfo.ContainsKey(elementId))
        {
            return currentBattlePosition.attackersInfo[elementId].possibleAttackers;
        }
        return null;
    }

    private void SetUI(Transform selection, Destructible obj, ObjectUI UIconfig)
    {
        UI.enabled = true;
        UI.transform.position = new Vector3(obj.center.x, UIconfig.UIposeByY, obj.center.z);

        elementName.text = obj.elementName;
        healthBar.GetComponent<RectTransform>().sizeDelta = UIconfig.healthBarSize;
        healthBar.minValue = 0;
        healthBar.maxValue = obj.maxHealth;
        healthBar.value = obj.health;
        healthBarFill.color = UIconfig.color;

        enemySelector.enabled = friendlySelector.enabled = false;

        SpriteRenderer selector = UIconfig.selector;
        selector.enabled = true;

        selector.transform.position = new Vector3(obj.center.x, 0.047f, obj.center.z);

        float maxBuildSize = 0.14f * (Mathf.Max(obj.sizeX, obj.sizeZ));

        selector.transform.localScale = new Vector3(maxBuildSize, maxBuildSize, maxBuildSize);

        SetDistanceAndDamage(UIconfig.distance && UIconfig.damage, selection.transform.position, obj.gameObject);

        health.enabled = true;
        health.gameObject.SetActive(true);
        healthCount.text = $"{obj.maxHealth}";
    }


    private void SetDistanceAndDamage(bool needDraw, Vector3 selectionPose, GameObject obj)
    {
        if (!needDraw)
        {
            distanceLine.enabled = false;
            damage.gameObject.SetActive(false);
            damage.enabled = false;
            return;
        }
           
        Unit unit = obj.GetComponent<Unit>();

        distanceLine.enabled = true;
        distanceLine.transform.position = new Vector3(selectionPose.x, 0.045f, selectionPose.z);
        float RangeSize = 800 + (1600 * unit.distance);
        distanceLine.GetComponent<RectTransform>().sizeDelta = new Vector2(RangeSize, RangeSize);

        damage.gameObject.SetActive(true);
        damage.enabled = true;
        damageCount.text = $"{unit.damage}";
    }

    private void SelectionHandle()
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
                        selectAction(selection);
                    }
                    break;
            }
        }
    }

    private void selectAction(Transform selection)
    {
        Cell cell = selection.GetComponent<Cell>();
        GameObject selectionObject = cell.occypier;

        if (selectionObject != null && idle && UIconfigs.ContainsKey(selectionObject.tag))
        {
            SetUI(selection, selectionObject.GetComponent<Destructible>(), UIconfigs[selectionObject.tag]);
            showAttackers(selectionObject);
        }
        _selection = selection;
    }

    private void Deselection()
    {
        if (_selection.CompareTag("Cell"))
        {
            Cell cell = _selection.GetComponent<Cell>();
            GameObject obj = _selection.GetComponent<Cell>().occypier;

            if (obj != null)
            {
                if (obj.CompareTag("FriendlyUnit") || obj.CompareTag("EnemyUnit") || obj.CompareTag("EnemyBuild") || obj.CompareTag("FriendlyBuild"))
                {
                    hideAttackers();
                    TurnOffUI();
                }
            }
        }
        _selection = null;
    }
}