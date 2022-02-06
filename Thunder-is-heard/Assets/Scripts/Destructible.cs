using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField] public int maxHealth, health, id, type;
    public string elementName;
    public Vector3[] occypiedPoses;
    public Vector3 center;
    public int sizeX, sizeZ;

    private void Awake()
    {
        
    }

    protected void GetDamage(int damage)
    {
        health -= damage;

        if (health < 1) Die();
        else EventMaster.current.ObjectHealthChange(this.gameObject, health);
        return;
    }

    public void UpdateOccypiedPoses()
    {
        Vector3 startPose = center = transform.position;
        if (sizeX < 2 && sizeZ < 2)
        {
            occypiedPoses = new Vector3[] { startPose };
            return;
        }

        occypiedPoses = new Vector3[sizeX * sizeZ];

        BringLocalByRotation();

        Vector3 stepByX = FindStepsForFillOcypyByX();
        Vector3 stepByZ = FindStepsForFillOcypyByZ();

        int maxX = (int)startPose.x + ((int)stepByX.x * sizeX);
        int maxZ = (int)startPose.z + ((int)stepByZ.z * sizeZ);

        center = new Vector3(((transform.position.x + (maxX - stepByX.x))) / 2, 0, ((transform.position.z + (maxZ - stepByZ.z))) / 2);

        Vector3 currentPose = startPose;

        int index = 0;
        for (int x = (int)startPose.x; ;)
        {
            x = (int)currentPose.x;
            if (x == maxX)
            {
                break;
            }
            for (int z = (int)startPose.z; ;)
            {
                z = (int)currentPose.z;
                if (z == maxZ)
                {
                    break;
                }

                occypiedPoses[index] = new Vector3(currentPose.x, 0, currentPose.z);
                index++;
                currentPose = currentPose + stepByZ;

            }
            currentPose = currentPose + stepByX;
            currentPose.z = startPose.z;
        }
    }


    private void BringLocalByRotation()
    {
        if (transform.eulerAngles.y == 90 || transform.eulerAngles.y == 270)
        {
            int oldSizeX = sizeX;
            sizeX = sizeZ;
            sizeZ = oldSizeX;
        }
    }


    private Vector3 FindStepsForFillOcypyByX()
    {
        if (transform.forward.x != 0) return transform.forward;
        if (transform.right.x != 0) return transform.right;
        return new Vector3(0, 0, 0);
    }


    private Vector3 FindStepsForFillOcypyByZ()
    {
        if (transform.right.z != 0) return transform.right;
        if (transform.forward.z != 0) return transform.forward;
        return new Vector3(0, 0, 0);
    }


    protected void ObjectHasBeenAttack(BattleSlot attacker, BattleSlot defender, Vector3 attackPoint, int damage)
    {
        

        if (defender.id == id)
        {
            GetDamage(damage);
            return;
        }
    }

    protected bool DamageLethality(int damage)
    {
        if (damage >= health)
        {
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
