using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class Cell : MonoBehaviour {

    private GameManager gameManager
    {
        get
        {
            return FindObjectOfType<GameManager>();
        }
    }

	void Start()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener( (data) => { gameManager.playerList[0].PlaceCard(this); });
        trigger.triggers.Add(entry);
    }

    public bool IsValid()
    {
        if (GetComponent<Image>().color == Color.green)
        {
            return true;
        }

        return false;
    }
}
