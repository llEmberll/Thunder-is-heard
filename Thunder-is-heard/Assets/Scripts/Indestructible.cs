using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indestructible : MonoBehaviour
{
    public Vector3[] occypiedPoses;
    public int sizeX;
    public int sizeZ;
    public Vector3 center;

    private void Awake()
    {
        UpdateOccypied();
        EventMaster.current.ObjectDestroyed += DestroyIndestructible;
    }

    private void Start()
    {
        EventMaster.current.SceneAddObject(this.gameObject, occypiedPoses);
    }

    private void DestroyIndestructible(GameObject element, Vector3[] poses)
    {
        if (this.transform.position == element.transform.position)
        {
            Destroy(this.gameObject);
            EventMaster.current.ObjectDestroyed -= DestroyIndestructible;
        }

    }

    private void UpdateOccypied()
    {
        Vector3 startPose = transform.position;
        if (sizeX < 2 && sizeZ < 2)
        {
            occypiedPoses = new Vector3[] { transform.position }; 
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
}
