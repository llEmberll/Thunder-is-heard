using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LandableUnit : MonoBehaviour, IPointerEnterHandler
{
    public int buttonId, previewId, count;

    public string Health, Damage, Distance, Mobility;

    public Text UIHealth, UIDamage, UIDistance, UIMobility, countText;

    public Sprite image;

    private void Awake()
    {
        EventMaster.current.FightIsStarted += StartFight;
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

    private void StartFight()
    {
        EventMaster.current.FightIsStarted -= StartFight;
        Destroy(this.gameObject);
    }

    public void OnPointerEnter(PointerEventData data)
    {
        UIHealth.text = Health; UIDamage.text = Damage; UIDistance.text = Distance; UIMobility.text = Mobility;
    }

}
