using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTable : MonoBehaviour
{
    [SerializeField] private Vector2Int _gridSize;
    [SerializeField] private Cell _prefab;
    [SerializeField] private float _offset;
    [SerializeField] private Transform _parent;

    [SerializeField] private Dictionary<Vector3, Cell> cellsData;



    [ContextMenu("Generate grid")]
    private void GenerateGrid()
    {
        DeleteCells();

        var cellsize = _prefab.GetComponent<MeshRenderer>().bounds.size;

        cellsData = new Dictionary<Vector3, Cell>(_gridSize.x * _gridSize.y);

        for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                var position = new Vector3(x * (cellsize.x + _offset), 0, y * (cellsize.z + _offset));

                var cell = Instantiate(_prefab, position, Quaternion.identity, _parent);

                cell.name = $"|X:{x}||Y:{y}|";
                cell.tag = "Cell";
                cellsData.Add(position, cell);
            }
        }
    }

    private void Start()
    {
        GenerateGrid();
        //EventMaster.current.SelectedObject += GridSelected;
        EventMaster.current.CellRequestByPose += AnswerCellRequest;
        EventMaster.current.UnitDies += UnitDead;
        EventMaster.current.BuildDestroed += BuildDestroy;
    }

    private void BuildDestroy(Build build, Cell[] occypiedCells)
    {
        updateStatuses();
    }

    private void UnitDead(Unit unit)
    {
        updateStatuses();
    }

    private void AnswerCellRequest(Vector3 pose)
    {
        Cell cell = getCellInfoByPose(pose);
        EventMaster.current.CellSending(cell);
    }

    //private void GridSelected(Vector3 pose, Material material, bool render)
    //{
    //    Cell cell = cellsData[pose];

    //    if (render)
    //    {
    //        cell.renderOn();
    //    }
    //    else
    //    {
    //        cell.renderOff();
    //    }

    //    cell.ChangeMaterial(material);
    //}


    [ContextMenu("Delete Cells")]
    private void DeleteCells()
    {
        GameObject[] cells = GameObject.FindGameObjectsWithTag("Cell");
        foreach (GameObject obj in cells)
        {
            DestroyImmediate(obj);
        }
    }

    private void updateCellTable(Dictionary<Vector3, Cell> newCellsData)
    {
        cellsData = newCellsData;
    }

    private void clearTable()
    {
        cellsData = new Dictionary<Vector3, Cell>(900);

    }

    private void addToTable(Vector3 cellPose, Cell cell)
    {
        cellsData.Add(cellPose, cell);

    }

    public void updateStatuses()
    {
        foreach (KeyValuePair<Vector3, Cell> item in cellsData)
        {

            item.Value.updateCellStatus();
        }
    }
    
    public void changeTypeOfCell(Vector3 cellPose, int newtype)
    {
        cellsData[cellPose].changeType(newtype);
    }

    public void changeStatusOfCell(Vector3 cellPose, bool status)
    {
        if (status)
        {
            cellsData[cellPose].OccypyCell();
        }
        else
        {
            cellsData[cellPose].freeCell();

        }
    }

    public Cell getCellInfoByPose(Vector3 cellPose)
    {
        return cellsData[cellPose];
    }

    public void turnOffCells()
    {

        foreach(KeyValuePair <Vector3, Cell> item in cellsData)
        {

            item.Value.renderOff();
        }
    }

    public void turnOffSomeCells(Dictionary<Cell, int> cells)
    {
        foreach (KeyValuePair<Cell, int> item in cells)
        {
            item.Key.renderOff();

        }
    }

    public void turnOnSomeCells(Dictionary<Cell, int> cells)
    {
        foreach (KeyValuePair<Cell, int> item in cells)
        {
            item.Key.renderOn();
           
        }
    }


    public Cell[] getRange(Vector3 Center, int radius, bool ignoreOccypy)
    {
        int minX = (int)Center.x - radius;

        int maxX = (int)Center.x + radius;

        int minZ = (int)Center.z - radius;

        int maxZ = (int)Center.z + radius;

        int maxCells = (int)Mathf.Pow((Mathf.Abs(maxX - minX) + 1), 2);

        Cell[] cells = new Cell[maxCells];
        Vector3 currentPose;

        int dynamicZ = minZ;

        int cellSelector = 0;
        for (int X = minX; X <= maxX; X ++)
        {
            for (; dynamicZ <= maxZ; dynamicZ++)
            {
                currentPose.x = X;
                currentPose.z = dynamicZ;
                currentPose.y = 0;

                if (cellsData.ContainsKey(currentPose))
                {
                    Cell currentCell = cellsData[currentPose];
                    if (currentCell.IsOccypy == false)
                    {
                        if (currentCell.cellPose != Center)
                        {
                            cells[cellSelector] = currentCell;
                            cellSelector++;
                        }
                    }

                    else
                    {
                        if (ignoreOccypy)
                        {
                            cells[cellSelector] = currentCell;
                            cellSelector++;
                        }
                    }

                }
            }
            dynamicZ = minZ;
        }
        return cells;
    }
}
