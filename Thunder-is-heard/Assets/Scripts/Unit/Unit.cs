using UnityEngine;
using UnityEngine.EventSystems;

public class Unit : Destructible
{
    public int damage, mobility, distance;
    public float realSpeed;
    public int unitId;

    private Cell point;
    private Cell[] route;
    private bool mustMove;

    [SerializeField] public Transform model;

    public Animator animator;

    public Unit(string UnitName, int Id, int MaxHealth, int Damage, int Distance, int Mobility, float RealSpeed, Transform Model = null)
    {
        this.elementName = UnitName;
        this.unitId = Id;
        this.maxHealth = MaxHealth;
        this.damage = Damage;
        this.distance = Distance;
        this.mobility = Mobility;
        this.realSpeed = RealSpeed;
        this.model = Model;
        
    }

    private void Awake()
    {
        type = 1;
        center = transform.position;
        sizeX = sizeZ = 1;

        EventMaster.current.UnitAttacks += SomeBodyAttacks;

        EventMaster.current.UnitMoveOnRoute += moveOnRoute;
    }

    private void Start()
    {
        health = maxHealth;

        animator = model.GetComponent<Animator>();

        UpdateOccypiedPoses();
        EventMaster.current.SceneAddObject(this.gameObject, occypiedPoses);
    }

    private void SomeBodyAttacks(BattleSlot attacker, BattleSlot defender, Vector3 attackPoint, int damage)
    {

        if (attacker.id == id)
        {

            rotateToTarget(attackPoint);
        }
        if (defender.id == id)
        {
            if (DamageLethality(damage))
            {
                EventMaster.current.UnitMoveOnRoute -= moveOnRoute;
                EventMaster.current.UnitAttacks -= SomeBodyAttacks;
            }
            ObjectHasBeenAttack(attacker, defender, attackPoint, damage);
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

    private void moveOnRoute(int unitId, Cell[] newRoute)
    {

        if (unitId == id)
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
        point = cell;
        rotateToTarget(point.transform.position);

        EventMaster.current.StartUnitMove(GetComponent<Unit>(), unitId, this.transform.position);

        mustMove = true;
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
        UpdateOccypiedPoses();

        Cell nextPoint = setNextPoint();
        if (nextPoint == null)
        {
            mustMove = false;

            EventMaster.current.UnitMovingComplete(this.gameObject, id, this.occypiedPoses);
            return;
        }
        
        occypyCell(nextPoint);
    }
}
