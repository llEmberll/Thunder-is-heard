using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Build : Destructible
{
    [SerializeField] public int typeId;

    public Build(int TypeId, int healthCount, int Id, int SizeX, int SizeZ, string BuildName)
    {
        this.typeId = TypeId;
        this.maxHealth = healthCount;
        this.id = Id;
        this.sizeX = SizeX;
        this.sizeZ = SizeZ;
        this.elementName = BuildName;
    }

    private void Awake()
    {
        type = 2;
        EventMaster.current.UnitAttacks += ObjectHasBeenAttack;
    }

    private void Start()
    {
        health = maxHealth;

        UpdateOccypiedPoses();
        EventMaster.current.SceneAddObject(this.gameObject, occypiedPoses);
    }
}
