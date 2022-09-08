using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prefabs : MonoBehaviour
{
    //Units
    [SerializeField] private GameObject CT;
    [SerializeField] private GameObject lis;
    [SerializeField] private GameObject newCT;
    [SerializeField] private GameObject humveee;

    [SerializeField] private Sprite CTImage;
    [SerializeField] private Sprite lisImage;
    [SerializeField] private Sprite newCTImage;
    [SerializeField] private Sprite humveeImage;


    //builds
    [SerializeField] private GameObject tent;
    [SerializeField] private GameObject wareHouse;

    [SerializeField] private Sprite tentImage;
    [SerializeField] private Sprite wareHouseImage;

    private Dictionary<int, UnitData> units = new Dictionary<int, UnitData>();
    private Dictionary<int, BuildData> builds = new Dictionary<int, BuildData>();


    private int currenUnitIdKey, currentBuildIdKey;


    private void Awake()
    {
        currenUnitIdKey = currentBuildIdKey = 0;
        AddUnit(lis, currenUnitIdKey, lisImage);
        AddUnit(CT, currenUnitIdKey, CTImage);
        AddUnit(newCT, currenUnitIdKey, newCTImage);
        AddUnit(humveee, currenUnitIdKey, humveeImage);

        AddBuild(tent, currentBuildIdKey, tentImage);
        AddBuild(wareHouse, currentBuildIdKey, wareHouseImage);
    }

    

    public UnitData GetUnitData(int id)
    {
        if (units.ContainsKey(id))
        {
            return units[id];
        }
        return null;

    }


    public BuildData GetBuildData(int id)
    {
        if (builds.ContainsKey(id))
        {
            return builds[id];
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

                currenUnitIdKey++;
            }
        }    
    }


    private void AddBuild(GameObject element, int elementId, Sprite image = null)
    {
        if (element != null)
        {
            Build buildClass = element.GetComponent<Build>();

            GameObject prefab = element.transform.FindChildByTag("Model");
            GameObject preview = element.transform.FindChildByTag("Preview");

            if (prefab == null && preview == null)
            {
                Debug.Log("В экземпляре нет меша, запись не добавлена в базу");
                return;
            }

            Vector3 meshScale = new Vector3((prefab.transform.localScale.x * element.transform.localScale.x), (prefab.transform.localScale.y * element.transform.localScale.y), (prefab.transform.localScale.z * element.transform.localScale.z));

            BuildData newBuildRecord = new BuildData(buildClass.elementName, elementId, buildClass.maxHealth, new Vector2(buildClass.sizeX, buildClass.sizeZ), prefab.transform.position, meshScale, prefab, preview, image);

            if (!builds.ContainsKey(elementId))
            {
                builds.Add(elementId, newBuildRecord);

                currentBuildIdKey++;
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

public class BuildData
{
    public string name;
    public int id, maxHealth;
    public Vector3 meshPose;
    public Vector3 meshScale;
    public Vector2 size;
    public Sprite previewImage;
    public GameObject prefab;
    public GameObject preview;


    public BuildData(string Name, int Id, int MaxHealth, Vector3 MeshPose, Vector3 MeshScale, Vector2 Size, GameObject Prefab, GameObject Preview, Sprite image = null)
    {
        this.name = Name;
        this.id = Id;
        this.maxHealth = MaxHealth;
        this.meshPose = MeshPose;
        this.meshScale = MeshScale;
        this.size = Size;
        this.prefab = Prefab;
        this.preview = Preview;
        this.previewImage = image;
    }
}


