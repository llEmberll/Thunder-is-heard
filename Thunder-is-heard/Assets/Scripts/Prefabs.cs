using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prefabs : MonoBehaviour
{

    [SerializeField] private GameObject CT;
    [SerializeField] private GameObject lis;

    private Dictionary<int, UnitData> units = new Dictionary<int, UnitData>();

    private int currentIdKey;


    private void Awake()
    {
        currentIdKey = 0;
        AddUnit(lis, currentIdKey);
        AddUnit(CT, currentIdKey);

        Debug.Log("������� ���������");
    }

    

    public UnitData GetUnitData(int id)
    {
        if (units.ContainsKey(id))
        {
            return units[id];
        }
        return null;

    }

    private void AddUnit(GameObject element, int elementId)
    {
        if (element != null)
        {

            Debug.Log("������� �� null");


            Debug.Log("id = " + elementId);

            Unit unitClass = element.GetComponent<Unit>();

            GameObject prefab = element.transform.FindChildByTag("Model");
            GameObject preview = element.transform.FindChildByTag("Preview");

            if (prefab == null && preview != null)
            {
                Debug.Log("� ���������� ��� ����, ������ �� ��������� � ����"); 
                return;
            }

            Vector3 meshScale = new Vector3((prefab.transform.localScale.x * element.transform.localScale.x), (prefab.transform.localScale.y * element.transform.localScale.y), (prefab.transform.localScale.z * element.transform.localScale.z));

            UnitData newUnitRecord = new UnitData(unitClass.unitName, elementId, unitClass.maxHealth, unitClass.damage, unitClass.distance, unitClass.mobility, unitClass.realSpeed, prefab.transform.position, meshScale, prefab, preview);

            if (!units.ContainsKey(elementId))
            {

                Debug.Log("� ���� ��� ������ ����");



                units.Add(elementId, newUnitRecord); 
                
                currentIdKey++;

                Debug.Log("���������");
            }
        }    
    }
}
public class UnitData
{
    public string name;
    public int id, maxHealth, damage, distance, mobility;
    public float realSpeed;
    public Vector3 meshPose;
    public Vector3 meshScale;
    public GameObject prefab;
    public GameObject preview;


    public UnitData(string Name, int Id, int MaxHealth, int Damage, int Distance, int Mobility, float speed, Vector3 MeshPose, Vector3 MeshScale, GameObject Prefab, GameObject Preview)
    {
        this.name = Name;
        this.id = Id;
        this.maxHealth = MaxHealth;
        this.damage = Damage;
        this.distance = Distance;
        this.mobility = Mobility;
        this.realSpeed = speed;
        this.meshPose = MeshPose;
        this.meshScale = MeshScale;
        this.prefab = Prefab;
        this.preview = Preview;
    }
}