using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Unit : MonoBehaviour, IPointerClickHandler
{
    public int Health, Damage, Mobility, Range;
    public string UnitName;
    public float RealSpeed;
    public int type;

    private Cell Point;
    private Cell[] Route;
    private bool mustMove;

    [SerializeField] private int id;

    [SerializeField] private Transform model;

    [SerializeField] private Text Name;
    [SerializeField] private Canvas UI;
    [SerializeField] private Slider HealthBar;
    [SerializeField] private Text DamageCount;
    [SerializeField] private Text HealthCount;
    [SerializeField] private Canvas DistanceLine;
    [SerializeField] private SpriteRenderer selector;

    private Animator animator;

    

    public Unit(string unitName, int health, int damage, int mobility, int range, float realSpeed)
    {
        this.UnitName = unitName;
        this.Health = health;
        this.Damage = damage;
        this.Mobility = mobility;
        this.Range = range;
        this.RealSpeed = realSpeed;
        this.type = 0;
    }


    private void Start()
    {
        
        animator = model.GetComponent<Animator>();
        EventMaster.current.UnitMoveOnRoute += moveOnRoute;
        EventMaster.current.UnitAttacksUnit += SomebodyAttacks;
        EventMaster.current.SelectedObject += ObjectSelected;
        EventMaster.current.UnitAttacksBuild += BuildHasBeenAttack;

        UI.enabled = selector.enabled = false;

        Name.text = UnitName;
        HealthBar.minValue = 0;
        HealthBar.maxValue = Health;
        UpdateHealthBar(Health);
        UpdateAttributes(Health, Damage);


        float RangeSize = 800 + (1600 * Range);
        DistanceLine.GetComponent<RectTransform>().sizeDelta = new Vector2(RangeSize, RangeSize);
        DistanceLine.enabled = false;
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

    private void UpdateAttributes(int newHealth, int newDamage)
    {
        HealthCount.text = newHealth.ToString();
        DamageCount.text = newDamage.ToString();
    }

    private void ObjectSelected(Vector3 pose, Material material, bool nowSelect)
    {
        if (transform.position == pose) UI.enabled = DistanceLine.enabled = selector.enabled = nowSelect;
    }

    private void BuildHasBeenAttack(Unit attacker, Build build, int damage)
    {
        if (attacker == GetComponent<Unit>())
        {
            rotateToTarget(build.transform.position);
        }
    }

    private void SomebodyAttacks(Unit attacker, Unit defender, int damage)
    {
        Unit thisUnit = this.GetComponent<Unit>();

        if (attacker == thisUnit)
        {
            attackUnit(defender);
        }
        else
        {
            if (defender == thisUnit)
            {
                getDamage(damage);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        EventMaster.current.UnitClick(this.GetComponent<Unit>());
    }

    private Cell setNextPoint()
    {
        bool setInNext = false;
        for (int index = 0; index < Route.Length; index++)
        {
            Cell point = Route[index];
            if (setInNext)
            {
                if (point != null)
                {
                    return point;
                }
                return null;

            }
            
            if (Point == point)
            {
                setInNext = true;
            }
        }
        return null;
    }

    private void moveOnRoute(Vector3 unitPose, Cell[] route)
    {
        if (transform.position == unitPose)
        {
            Route = route;
            occypyCell(Route[0]);
        }
    }

    private void rotateToTarget(Vector3 target)
    {
        model.LookAt(new Vector3(target.x, model.position.y, target.z));
    }

    private void occypyCell(Cell cell)
    {
        Debug.Log("Приказ о передвижении принят юнитом!");
        Point = cell;
        rotateToTarget(Point.transform.position);
        mustMove = true;
        Debug.Log("Началось движение");
    }


    private void attackUnit(Unit unit)
    {
        rotateToTarget(unit.transform.position);
        Debug.Log("Приказ атаковать принят юнитом!");
    }

    private void getDamage(int Damage)
    {
        Debug.Log("Юнит получил урон");

        Health -= Damage;
        UpdateHealthBar(Health);
        if (Health <= 0)
        {
            Debug.Log("Юнит больше не имеет здоровья");
            Die();

        } 
    }

    private void Die()
    {
        Debug.Log("Юнит выходит из боя!");

        EventMaster.current.UnitMoveOnRoute -= moveOnRoute;
        EventMaster.current.UnitAttacksUnit -= SomebodyAttacks;
        EventMaster.current.SelectedObject -= ObjectSelected;
        EventMaster.current.UnitAttacksBuild -= BuildHasBeenAttack;

        EventMaster.current.UnitDying(this.GetComponent<Unit>());

        Destroy(this.gameObject);
        
    }

    private void FixedUpdate()
    {
        if (mustMove == true)
        {
            Vector3 pointPosition = Point.cellPose;
            if ((Vector3.Distance(pointPosition, transform.position)) > 0)
            {
                transform.position = Vector3.MoveTowards(transform.position, pointPosition, RealSpeed * Time.fixedDeltaTime);
            } 
            else
            {
                moveComplete();
            }
        }
    }

    private void moveComplete()
    {
        Debug.Log("Юнит прибыл на точку");

        Cell nextPoint = setNextPoint();
        if (nextPoint == null)
        {
            mustMove = false;
            
            EventMaster.current.movingComplete();

            Debug.Log("must move: " + mustMove);
            return;
        }
        
        occypyCell(nextPoint);
    }
}
