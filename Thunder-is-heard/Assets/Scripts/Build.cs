using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Build : MonoBehaviour
{
    [SerializeField] public int maxHealth, health, id, typeId;
    [SerializeField] public string buildName;

    [SerializeField] public int sizeX;
    [SerializeField] public int sizeZ;
    public Vector3 center;

    public Vector3[] occypiedPoses;


    public Build(int TypeId, int healthCount, int Id, int SizeX, int SizeZ, string Name)
    {
        this.typeId = TypeId;
        this.maxHealth = healthCount;
        this.id = Id;
        this.sizeX = SizeX;
        this.sizeZ = SizeZ;
        this.name = Name;
        
    }

    private void Awake()
    {
        EventMaster.current.UnitAttacksBuild += BuildHasBeenAttack;

        health = maxHealth;

        
    }

    private void Start()
    {
        UpdateOccypiedCells();

        SendNewOccypyPoses();

        EventMaster.current.SceneAddBuild(GetComponent<Build>(), this.CompareTag("EnemyBuild"));
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

    private void SendNewOccypyPoses()
    {
        foreach (Vector3 Pose in occypiedPoses)
        {
            EventMaster.current.SendNewBuildOccypyPose(this.gameObject, Pose);
        }
    }

    private void UpdateOccypiedCells()
    {
        occypiedPoses = new Vector3[sizeX * sizeZ];

        Vector3 buildStartPose = transform.position;

        int index = 0;

        Vector3 bounds = buildStartPose + transform.right * sizeX + transform.forward * sizeZ;
        int maxX = (int)bounds.x;
        int maxZ = (int)bounds.z;

        center = new Vector3((transform.position.x + maxX - 1) /2, 0, (transform.position.z + maxZ - 1) /2);

        for (int x = (int)buildStartPose.x; x < maxX; x++)
        {
            for (int z = (int)buildStartPose.z; z < maxZ; z++)
            {
                occypiedPoses[index] = transform.right * x + transform.forward * z;
                index++;
            }
        }
    }


    private void getDamage(int Damage)
    {
        Debug.Log("Постройка получила урон");

        Debug.Log("урон - " + Damage);

        health -= Damage;
        if (health <= 0)
        {
            Debug.Log("Постройка больше не имеет здоровья");
            Die();

        }
    }

    private void Die()
    {
        Debug.Log("Постройка разрушается!");

        EventMaster.current.BuildDestroing(this.GetComponent<Build>(), occypiedPoses);

        Destroy(this.gameObject);
        EventMaster.current.UnitAttacksBuild -= BuildHasBeenAttack;


    }
}
