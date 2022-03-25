using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prefabs : MonoBehaviour
{

    [SerializeField] private GameObject CT;
    [SerializeField] private GameObject lis;
    [SerializeField] private GameObject newCT;
    [SerializeField] private GameObject humveee;
    [SerializeField] private GameObject HT;

    [SerializeField] private Sprite CTImage;
    [SerializeField] private Sprite lisImage;
    [SerializeField] private Sprite newCTImage;
    [SerializeField] private Sprite humveeImage;
    [SerializeField] private GameObject HTImage;

    private Dictionary<int, UnitData> units = new Dictionary<int, UnitData>();

    private int currentIdKey;


    private void Awake()
    {
        currentIdKey = 0;
        AddUnit(lis, currentIdKey, lisImage);
        AddUnit(CT, currentIdKey, CTImage);
        AddUnit(newCT, currentIdKey, newCTImage);
        AddUnit(humveee, currentIdKey, humveeImage);
    }

    

    public UnitData GetUnitData(int id)
    {
        if (units.ContainsKey(id))
        {
            return units[id];
        }
        return null;

    }

    private void AddUnit(GameObject element, int elementId, Sprite image = null)
    {
        if (element != null)
        {
            Unit unitClass = element.GetComponent<Unit>();

            GameObject prefab = element.transform.FindChildByTag("Model");
            GameObject preview = element.transform.FindChildByTag("Preview");

            if (prefab == null && preview == null)
            {
                Debug.Log("В экземпляре нет меша, запись не добавлена в базу"); 
                return;
            }

            Vector3 meshScale = new Vector3((prefab.transform.localScale.x * element.transform.localScale.x), (prefab.transform.localScale.y * element.transform.localScale.y), (prefab.transform.localScale.z * element.transform.localScale.z));

            UnitData newUnitRecord = new UnitData(unitClass.elementName, elementId, unitClass.maxHealth, unitClass.damage, unitClass.distance, unitClass.mobility, unitClass.realSpeed, prefab.transform.position, meshScale, prefab, preview, image);

            if (!units.ContainsKey(elementId))
            {
                units.Add(elementId, newUnitRecord); 
                
                currentIdKey++;
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
    public Sprite previewImage;
    public GameObject prefab;
    public GameObject preview;


    public UnitData(string Name, int Id, int MaxHealth, int Damage, int Distance, int Mobility, float speed, Vector3 MeshPose, Vector3 MeshScale, GameObject Prefab, GameObject Preview, Sprite image = null)
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
        this.previewImage = image;
    }
}
