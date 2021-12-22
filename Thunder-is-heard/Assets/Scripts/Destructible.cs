using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField] public int maxHealth, health, id;
    public string elementName;
    public Vector3[] occypiedPoses;
    public Vector3 center;
    public int sizeX, sizeZ;


    protected void GetDamage(int damage)
    {
        Debug.Log("Get damaged! " + this.name);

        Debug.Log("damage = " + damage);

        Debug.Log("health = " + health);

        health -= damage;

        if (health < 1) Die();
        return;
    }

    protected void UpdateOccypiedPoses()
    {
        Vector3 startPose = center = transform.position;
        if (sizeX < 2 && sizeZ < 2)
        {
            occypiedPoses = new Vector3[] { startPose };
            return;
        }
        occypiedPoses = new Vector3[sizeX * sizeZ];

        Vector3 bounds = startPose + transform.right * sizeX + transform.forward * sizeZ;
        int maxX = (int)bounds.x;
        int maxZ = (int)bounds.z;

        center = new Vector3((transform.position.x + maxX - 1) / 2, 0, (transform.position.z + maxZ - 1) / 2);

        int index = 0;
        for (int x = (int)startPose.x; x < maxX; x++)
        {
            for (int z = (int)startPose.z; z < maxZ; z++)
            {
                occypiedPoses[index] = transform.right * x + transform.forward * z;
                index++;
            }
        }
    }

    protected void ObjectHasBeenAttack(GameObject attacker, GameObject defender, Vector3 attackPoint, int damage)
    {
        Debug.Log("object" + this.name + " = " + "has been attacked");

        if (this.gameObject == defender)
        {
            GetDamage(damage);
            return;
        }
    }

    protected bool DamageLethality(int damage)
    {
        if (damage >= health)
        {
            Debug.Log(this.name + " || damage = " + damage + " || health = " + health);

            return true;
        }
        return false;
    }
    protected void Die()
    {

        Debug.Log(this.name + " die");

        EventMaster.current.UnitAttacks -= ObjectHasBeenAttack;
        EventMaster.current.ObjectDestroy(this.gameObject, occypiedPoses);
        Destroy(this.gameObject);
    }
}