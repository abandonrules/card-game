using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Card : Photon.MonoBehaviour {

    // Reference of card's in-game id
    public int id;
    // Reference of card's owner
    public Transform owner;
    // Reference of card's sprite
    public Sprite sprite;
    // Reference of card's attack; KEYS: "top", "right", "bottom", "left"
    public Dictionary<string, int> attack = new Dictionary<string, int>();
    // Reference of card's effect (if it has one)
    //public string effect;
    // Reference of card's child attack UI
    public List<Text> attackUI;
    // Reference of Board class
    public Board board;
    // Listener properties
    private EventTrigger trigger;
    private EventTrigger.Entry entry;
    // Reference of old card transform
    private Vector2 movedPosition;
    private Vector2 movedSize;
        
    public void SetAttackUI(Transform child, string direction)
    {
        child.GetComponent<Text>().text = attack[direction.ToLower()].ToString();
    }

    public void SetOwner(Transform newOwner)
    {
        if (owner != newOwner)
        {
            owner = newOwner;

            if (owner.gameObject.tag == "Player")
            {
                AddPlayerListener();
            }
            else if (owner.gameObject.tag == "Board")
            {
                RemovePlayerListener();
            }
        }
    }

    void AddPlayerListener()
    {
        trigger = GetComponent<EventTrigger>();
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener( (data) => { owner.GetComponent<Player>().SelectCard(this); });
        trigger.triggers.Add(entry);
    }

    void RemovePlayerListener()
    {
        Debug.Log("Removed Player Listener");
        entry.callback.RemoveAllListeners();
        trigger.triggers.Remove(entry);
        // Add listener when card is on board
    }

    public void ShowAttackUI()
    {
        foreach(Text val in attackUI)
        {
            val.color = Color.black;
            ResizeText(GetComponent<RectTransform>().sizeDelta);
        }
    }

    public void HideAttackUI()
    {
        foreach (Text val in attackUI)
        {
            val.color = Color.clear;
        }
    }

    public void Select()
    {
        if (owner.tag == "Player")
        {
            // Player Cycle #2
            //board.ShowCover();
            Debug.Log("Select: " + name);

            float targetX = -owner.GetComponent<RectTransform>().localPosition.x - 475f;
            movedPosition = transform.localPosition;
            LeanTween.moveLocal(gameObject, new Vector2(targetX, 0), 0.1f)
                .setEase(LeanTweenType.easeInQuad)
                .setOnComplete(() =>
                {
                    owner.GetComponent<Player>().gameManager.ShowCardControls();
                });

            Vector2 targetSize = new Vector2(200f, 200f);
            movedSize = GetComponent<RectTransform>().sizeDelta;
            LeanTween.value(gameObject, GetComponent<RectTransform>().sizeDelta, targetSize, 0.1f)
                .setEase(LeanTweenType.easeInQuad)
                .setOnUpdateVector2((Vector2 val) =>
                {
                    GetComponent<RectTransform>().sizeDelta = val;
                    ResizeText(val);
                });
            board.ShowValidCells();
        }
    }

    void ResizeText(Vector2 val)
    {
        // Top text
        attackUI[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, val.y / 3);
        attackUI[0].GetComponent<RectTransform>().sizeDelta = new Vector2(val.x, val.y / 3);

        // Right text
        attackUI[1].GetComponent<RectTransform>().anchoredPosition = new Vector2(val.x / 3, 0);
        attackUI[1].GetComponent<RectTransform>().sizeDelta = new Vector2(val.x, val.y / 3);

        // Bottom text
        attackUI[2].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -val.y / 3);
        attackUI[2].GetComponent<RectTransform>().sizeDelta = new Vector2(val.x, val.y / 3);

        // Left text
        attackUI[3].GetComponent<RectTransform>().anchoredPosition = new Vector2(-val.x / 3, 0);
        attackUI[3].GetComponent<RectTransform>().sizeDelta = new Vector2(val.x, val.y / 3);
    }

    public void UndoPlayerSelect()
    {
        //board.HideCover();
        Debug.Log("Deselect: " + name);

        owner.GetComponent<Player>().gameManager.HideCardControls();
        Vector2 targetPos = movedPosition;
        movedPosition = Vector2.zero;
        LeanTween.moveLocal(gameObject, targetPos, 0.1f)
            .setEase(LeanTweenType.easeInQuad);

        Vector2 targetSize = movedSize;
        movedSize = Vector2.zero;
        LeanTween.value(gameObject, GetComponent<RectTransform>().sizeDelta, targetSize, 0.1f)
            .setEase(LeanTweenType.easeInQuad)
            .setOnUpdateVector2((Vector2 val) =>
            {
                GetComponent<RectTransform>().sizeDelta = val;
                ResizeText(val);
            });
        board.HideValidCells();
    }

    public void Place(Vector2 targetPos)
    {
        owner.GetComponent<Player>().gameManager.HideCardControls();
        movedPosition = Vector2.zero;
        LeanTween.moveLocal(gameObject, targetPos, 0.1f)
            .setEase(LeanTweenType.easeInQuad);

        Vector2 targetSize = movedSize;
        movedSize = Vector2.zero;
        LeanTween.value(gameObject, GetComponent<RectTransform>().sizeDelta, targetSize, 0.1f)
            .setEase(LeanTweenType.easeInQuad)
            .setOnUpdateVector2((Vector2 val) =>
            {
               GetComponent<RectTransform>().sizeDelta = val;
               ResizeText(val); 
            });
        board.HideValidCells();
        owner.GetComponent<Player>().photonView.RPC("Draw", PhotonTargets.AllViaServer, 1);
        SetOwner(board.transform);
    }

    public void Rotate(bool isClockwise)
    {
        float targetRotateCard = 0;

        if (isClockwise)
        {
            targetRotateCard = transform.localEulerAngles.z - 90;

            Text[] tempAttack = new Text[4];
            for (int i = 0; i < attackUI.Count - 1; i++)
            {
                tempAttack[i+1] = attackUI[i];
                if (i == 0)
                {
                    tempAttack[i+1].name = "Right";
                }
                else if (i == 1)
                {
                    tempAttack[i+1].name = "Bottom";
                }
                else if (i == 2)
                {
                    tempAttack[i+1].name = "Left";
                }
            }
            tempAttack[0] = attackUI[attackUI.Count - 1];
            tempAttack[0].name = "Top";
            attackUI.Clear();
            attackUI.AddRange(tempAttack);
        }
        else
        {
            
        }

        LeanTween.value(transform.localEulerAngles.z, targetRotateCard, 0.1f)
            .setEase(LeanTweenType.easeInQuad)
            .setOnUpdate((float val) =>
            {
               transform.localEulerAngles = new Vector3(0, 0, val); 
            });
    }
}
