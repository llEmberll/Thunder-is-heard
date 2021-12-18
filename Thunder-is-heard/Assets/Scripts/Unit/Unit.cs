using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Unit : MonoBehaviour
{
    public int health, maxHealth,  damage, mobility, distance;
    public string unitName;
    public float realSpeed;
    public int type, unitId;

    private Cell point;
    private Cell[] route;
    private bool mustMove;

    [SerializeField] public Transform model;

    public Animator animator;

    

    public Unit(string UnitName, int Id, int MaxHealth, int Damage, int Distance, int Mobility, float RealSpeed, Transform Model)
    {
        this.unitName = UnitName;
        this.unitId = Id;
        this.maxHealth = MaxHealth;
        this.damage = Damage;
        this.distance = Distance;
        this.mobility = Mobility;
        this.realSpeed = RealSpeed;
        this.model = Model;
        this.type = 0;

    }

    private void Awake()
    {
        EventMaster.current.UnitMoveOnRoute += moveOnRoute;
        EventMaster.current.UnitAttacksUnit += SomebodyAttacks;
        EventMaster.current.UnitAttacksBuild += BuildHasBeenAttack;

        
    }

    private void Start()
    {
        health = maxHealth;

        animator = model.GetComponent<Animator>();

        Debug.Log("unit посылает ивент о его появлении");

        EventMaster.current.SceneAddUnit(GetComponent<Unit>(), this.CompareTag("EnemyUnit"));
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

    private Cell setNextPoint()
    {
        bool setInNext = false;
        for (int index = 0; index < route.Length; index++)
        {
            Cell currentPoint = route[index];
            if (setInNext)
            {
                if (currentPoint != null)
                {
                    return currentPoint;
                }
                return null;

            }
            
            if (point == currentPoint)
            {
                setInNext = true;
            }
        }
        return null;
    }

    private void moveOnRoute(Vector3 unitPose, Cell[] newRoute)
    {

        if (transform.position == unitPose)
        {
            route = newRoute;
            occypyCell(route[0]);
        }
    }

    private void rotateToTarget(Vector3 target)
    {
        model.LookAt(new Vector3(target.x, model.position.y, target.z));
    }

    private void occypyCell(Cell cell)
    {
        Debug.Log("Приказ о передвижении принят юнитом!");
        point = cell;
        rotateToTarget(point.transform.position);

        EventMaster.current.StartUnitMove(GetComponent<Unit>(), unitId, this.transform.position);

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

        health -= Damage;
        if (health <= 0)
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
        EventMaster.current.UnitAttacksBuild -= BuildHasBeenAttack;

        EventMaster.current.UnitDying(this.GetComponent<Unit>());

        Destroy(this.gameObject);
        
    }

    private void FixedUpdate()
    {
        if (mustMove == true)
        {
            Vector3 pointPosition = point.cellPose;
            if ((Vector3.Distance(pointPosition, transform.position)) > 0)
            {
                transform.position = Vector3.MoveTowards(transform.position, pointPosition, realSpeed * Time.fixedDeltaTime);
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

            EventMaster.current.UnitmovingComplete(GetComponent<Unit>(), unitId, this.transform.position);

            Debug.Log("must move: " + mustMove);
            return;
        }
        
        occypyCell(nextPoint);
    }
}
