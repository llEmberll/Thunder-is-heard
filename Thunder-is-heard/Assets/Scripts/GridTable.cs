using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class GridTable : MonoBehaviour
{
    [SerializeField] public Vector2Int _gridSize;
    [SerializeField] private Cell _prefab;
    [SerializeField] private float _offset;
    [SerializeField] private Transform _parent;

    [SerializeField] private Dictionary<Vector3, Cell> cellsData;

    private Dictionary<Cell, int> realMoveCells;

    private Cell[] possibleMoveCells;

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

    private void Awake()
    {
        GenerateGrid();

        EventMaster.current.CellRequestByPose += AnswerCellRequest;
    }


    private void AnswerCellRequest(Vector3 pose)
    {
        Cell cell = getCellInfoByPose(pose);
        EventMaster.current.CellSending(cell);
    }

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
    
    public void changeTypeOfCell(Vector3 cellPose, int newtype)
    {
        cellsData[cellPose].changeType(newtype);
    }

    public Cell getCellInfoByPose(Vector3 cellPose)
    {
        return cellsData[cellPose];
    }

    public void turnOffCells()
    {

        foreach(KeyValuePair <Vector3, Cell> item in cellsData)
        {

            item.Value.renderSwitch(false);
        }
    }

    public void turnOffSomeCells(Dictionary<Cell, int> cells)
    {
        foreach (KeyValuePair<Cell, int> item in cells)
        {
            item.Key.renderSwitch(false);

        }
    }

    public void turnOnSomeCells(Dictionary<Cell, int> cells)
    {
        foreach (KeyValuePair<Cell, int> item in cells)
        {
            item.Key.renderSwitch(true);
           
        }
    }


    public void turnOnSomeCellsByMass(Cell[] cells)
    {
        for (int index = 0; index < cells.Length; index++)
        {
            if (cells[index] != null)
            {
                cells[index].renderSwitch(true);
            }
        }
    }


    public bool NeighbourCell(Vector3 pose, Vector3 cellPose)
    {
        if (Mathf.Max(Mathf.Abs((int)(pose.x - cellPose.x)), Mathf.Abs((int)(pose.z - cellPose.z))) == 1) return true;
        return false;
    }

    public Dictionary<Cell, int> GetRealMoveCells(Vector3 unitPose, int unitRange, string excTag = null)
    {
        possibleMoveCells = GetRange(unitPose, unitRange, false, excTag);
        realMoveCells = new Dictionary<Cell, int>();

        if (unitRange == 1)
        {
            foreach (Cell cell in possibleMoveCells)
            {
                if (cell != null) realMoveCells.Add(cell, 1);
            }
            return realMoveCells;
        }
        else
        {
            Cell[] firstCells = GetRange(unitPose, 1, false, excTag);

            foreach (Cell firstStep in firstCells)
            {
                if (firstStep != null) realMoveCells.Add(firstStep, 1);
            }
            UpdateLevelsOfCells(unitRange, firstCells, 2);

            return realMoveCells;
        }
    }


    public void UpdateLevelsOfCells(int range, Cell[] previousCells, int currentRange)
    {
        Cell[] newCells = new Cell[8 * currentRange];
        int newCellsIndex = 0;

        for (int index = 0; index < previousCells.Length; index++)
        {
            Cell currentPreviousCell = previousCells[index];
            if (currentPreviousCell != null)
            {
                for (int index2 = 0; index2 < possibleMoveCells.Length; index2++)
                {
                    Cell currentPossibleCell = possibleMoveCells[index2];
                    if (currentPossibleCell != null)
                    {
                        if (NeighbourCell(currentPreviousCell.cellPose, currentPossibleCell.cellPose))
                        {
                            if (!realMoveCells.ContainsKey(currentPossibleCell))
                            {
                                realMoveCells.Add(currentPossibleCell, currentRange);
                                newCells[newCellsIndex] = currentPossibleCell;
                                newCellsIndex++;
                            }
                            possibleMoveCells[index2] = null;
                        }
                    }
                }
            }
        }
        if (currentRange < range) UpdateLevelsOfCells(range, newCells, currentRange + 1);
        return;
    }



    private bool ISExistOnCellsByTag(Dictionary<Cell, int> cells, string objTag)
    {
        Debug.Log("��� ����� - " + objTag);

        foreach (KeyValuePair<Cell, int> item in cells)
        {
            GameObject currentOccypier = item.Key.occypier;

            Debug.Log("������ " + item.Key.cellPose);

            if (currentOccypier != null) {

                Debug.Log("�������� �� null, ��� - " + currentOccypier.name + " , tag = " + currentOccypier.tag);
                if (currentOccypier.tag == objTag)
                {
                    Debug.Log("��������� � ����� enemy!");
                    return true;
                }
                }

        }
        return false;
    }


    public Cell[] GetRange(Vector3 Center, int radius, bool ignoreOccypy, string excTag = null)
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
                    if (currentCell.occypier == null || ignoreOccypy)
                    {
                        if (currentCell.cellPose != Center)
                        {
                            cells[cellSelector] = currentCell;
                            cellSelector++;
                        }
                    }

                    else
                    {
                        if (excTag != null && currentCell.occypier.tag.Contains(excTag))
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
