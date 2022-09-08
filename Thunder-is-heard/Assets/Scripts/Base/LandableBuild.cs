using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LandableBuild : MonoBehaviour
{
    public int buttonId, previewId, count;

    public string health;

    public Sprite image;

    public Text countText;

    private void Awake()
    {
        EventMaster.current.ItemsDeleted += Delete;
        EventMaster.current.SpawnedUnit += ChangeCount;
    }


    private void ChangeCount(Cell cell, int id)
    {
        if (buttonId == id)
        {
            count--;
            countText.text = $"x{count}";
            if (count < 1)
            {
                EventMaster.current.SpawnedUnit -= ChangeCount;
                EventMaster.current.DeletePreview();
            }
        }

    }

    public void OnClick()
    {
        if (count > 0)
        {
            EventMaster.current.CreatePreview(previewId);
        }

    }



    private void Delete()
    {

        Debug.Log("Landable Build deleted");

        EventMaster.current.ItemsDeleted -= Delete;
        EventMaster.current.SpawnedUnit -= ChangeCount;

        Destroy(this.gameObject);
    }


}
