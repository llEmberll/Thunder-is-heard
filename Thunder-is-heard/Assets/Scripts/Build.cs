using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Build : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] public int Health, id;
    [SerializeField] private string buildName;

    [SerializeField] private Text Name;
    [SerializeField] private Canvas UI;
    [SerializeField] private Slider HealthBar;
    [SerializeField] private Text HealthCount;

    [SerializeField] private SpriteRenderer selector;

    [SerializeField] private int sizeX;
    [SerializeField] private int sizeZ;

    private Cell[] occypiedCells;

    public Build(int healthCount, int Id, int SizeX, int SizeZ, string Name)
    {
        this.Health = healthCount;
        this.id = Id;
        this.sizeX = SizeX;
        this.sizeZ = SizeZ;
        this.name = Name;
        
    }

    private void Start()
    {
        EventMaster.current.SelectedObject += ObjectSelected;
        EventMaster.current.FindObjectOnCell += ObjectFinded;
        EventMaster.current.UnitAttacksBuild += BuildHasBeenAttack;

        ClearOccypiedCells();
        UI.enabled = selector.enabled = false;
        Name.text = buildName;
        HealthBar.minValue = 0;
        HealthBar.maxValue = Health;
        UpdateHealthBar(Health);
        UpdateAttributes(Health);
    }

    private void UpdateHealthBar(int newValue)
    {
        if (newValue < 1)
        {
            HealthBar.value = HealthBar.minValue;
            return;
        }
        HealthBar.value = newValue;

    }

    private void UpdateAttributes(int newHealth)
    {
        HealthCount.text = newHealth.ToString();
    }

    private void BuildHasBeenAttack(Unit attacker, Build build, int damage)
    {
        Debug.Log("Ивент об атаке принят зданием");

        if (build == GetComponent<Build>())
        {
            Debug.Log("Здание совпадает");

            getDamage(damage);
        }
    }


    private void ClearOccypiedCells()
    {
        occypiedCells = new Cell[sizeX * sizeZ];
    }

    private bool IsExistInCells(Cell cell)
    {
        foreach (Cell existCell in occypiedCells)
        {
            if (existCell == cell)
            {
                return true;
            }
        }
        return false;
    }

    private void ObjectFinded(int objId, Cell cell)
    {
        if (id == objId)
        {
            if (!IsExistInCells(cell))
            {
                AddCellToOccypied(cell);
            }
        }
    }


    private void AddCellToOccypied(Cell cell)
    {
        for (int index = 0; index < occypiedCells.Length; index++)
        {

            if (occypiedCells[index] == null)
            {
                occypiedCells[index] = cell;
                break;

            } 
        }
    }


    private void ObjectSelected(Vector3 pose, Material material, bool nowSelect)
    {
        if (transform.position == pose) UI.enabled = selector.enabled = nowSelect;

    }

    private void getDamage(int Damage)
    {
        Debug.Log("Постройка получила урон");

        Debug.Log("урон - " + Damage);

        Health -= Damage;
        UpdateHealthBar(Health);
        if (Health <= 0)
        {
            Debug.Log("Постройка больше не имеет здоровья");
            Die();

        }
    }

    private void Die()
    {
        Debug.Log("Постройка разрушается!");

        EventMaster.current.BuildDestroing(this.GetComponent<Build>(), occypiedCells);

        Destroy(this.gameObject);

        EventMaster.current.SelectedObject -= ObjectSelected;
        EventMaster.current.FindObjectOnCell -= ObjectFinded;
        EventMaster.current.UnitAttacksBuild -= BuildHasBeenAttack;

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        EventMaster.current.BuildClick(this.GetComponent<Build>(), occypiedCells);
    }

}
