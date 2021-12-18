using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indestructible : MonoBehaviour
{

    private void Awake()
    {
        EventMaster.current.IndestructibleDestroyed += DestroyIndestructible;
    }

    private void Start()
    {
        EventMaster.current.SceneAddIndestructible(GetComponent<Indestructible>());
    }

    private void DestroyIndestructible(Indestructible element)
    {
        if (this.transform.position == element.transform.position)
        {
            Destroy(this.gameObject);
            EventMaster.current.IndestructibleDestroyed -= DestroyIndestructible;
        }
        
    }
}
