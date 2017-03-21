using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class Cell : MonoBehaviour {

    public GameManager gameManager;

	void Start()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener( (data) => { gameManager.MoveSelectedCardToBoard(transform); });
        trigger.triggers.Add(entry);
    }
}
