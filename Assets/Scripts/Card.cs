using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Card : MonoBehaviour {

    // Reference of card's in-game id
    [SerializeField]
    private int id;
    // Reference of card's owner
    public Transform owner;
    // Reference of card's sprite
    private Sprite sprite;
    // Reference of card's attack; KEYS: "top", "right", "bottom", "left"
    public Dictionary<string, int> attack = new Dictionary<string, int>();
    // Reference of card's effect (if it has one)
    //public string effect;
    // Reference of card's child attack UI
    public List<TextMeshProUGUI> attackUI;
    // Reference of Board class
    public Board board;
    // Listener properties
    private EventTrigger trigger;
    private EventTrigger.Entry entry;
    // Reference of old card transform
    private Vector2 movedPosition;
    private Vector2 movedSize;

    #region Menu Scene

    public void SelectColor()
    {
        ColorPanel colorPanel = FindObjectOfType<ColorPanel>();
        foreach (Card color in colorPanel.colorObjects)
        {
            color.GetComponent<Outline>().enabled = false;
        }

        GetComponent<Outline>().enabled = true;

        colorPanel.MoveToColor(transform.localPosition.y);
    }

    public void DeselectColor()
    {
        LeanTween.delayedCall(0.01f, () =>
        {
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                if (!EventSystem.current.currentSelectedGameObject.GetComponent<Outline>())
                {
                    GetComponent<Outline>().enabled = true;
                    return;
                }
            }
        });

        GetComponent<Outline>().enabled = false;
    }

    #endregion

    #region Gameplay Scene

    public void SetOwner(Transform newOwner)
    {
        if (owner != newOwner)
        {
            owner = newOwner;

            if (owner.tag == "Player" && !GetComponentInParent<Cell>())
            {
                AddPlayerListener();
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
        foreach(TextMeshProUGUI val in attackUI)
        {
            val.color = Color.white;
            ResizeText(GetComponent<RectTransform>().sizeDelta);
        }
    }

    public void HideAttackUI()
    {
        foreach (TextMeshProUGUI val in attackUI)
        {
            val.color = Color.clear;
        }
    }

    public void Select()
    {
        // Player Cycle #2
        //board.ShowCover();
        //Debug.Log("Select: " + name);

        float targetX = -owner.GetComponent<RectTransform>().localPosition.x - 475f;
        movedPosition = transform.localPosition;
        LeanTween.moveLocal(gameObject, new Vector2(targetX, 0), 0.1f)
            .setEase(LeanTweenType.easeInQuad)
            .setOnComplete(() =>
            {
                board.gameManager.ShowCardControls();
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
        //Debug.Log("Deselect: " + name);

        board.gameManager.HideCardControls();
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
        board.ResetValidCells();
    }

    public IEnumerator Place(Vector2 targetPos, Vector2 targetSize, string[] cardAttack)
    {
        movedPosition = Vector2.zero;
        movedSize = Vector2.zero;

        ResetRotation(cardAttack);

        LeanTween.moveLocal(gameObject, targetPos, 0.1f)
            .setEase(LeanTweenType.easeInQuad);

        LeanTween.value(gameObject, GetComponent<RectTransform>().sizeDelta, targetSize, 0.1f)
            .setEase(LeanTweenType.easeInQuad)
            .setOnUpdateVector2((Vector2 val) =>
            {
               GetComponent<RectTransform>().sizeDelta = val;
               ResizeText(val); 
            });

        RemovePlayerListener();

        yield return new WaitForSeconds(0.1f);
    }

    public void Rotate(bool isClockwise)
    {
        float targetRotateCard = 0;
        float targetRotateAttack = 0;

        if (isClockwise)
        {
            targetRotateCard = transform.localEulerAngles.z - 90;
            targetRotateAttack = attackUI[0].transform.localEulerAngles.z + 90;

            int[] tempAttack = new int[4];
            tempAttack[0] = attack["Top"];
            tempAttack[1] = attack["Right"];
            tempAttack[2] = attack["Bottom"];
            tempAttack[3] = attack["Left"];

            attack["Right"] = tempAttack[0];
            attack["Bottom"] = tempAttack[1];
            attack["Left"] = tempAttack[2];
            attack["Top"] = tempAttack[3];

            RotateUI();
        }
        else
        {
            
        }

        LeanTween.value(gameObject, transform.localEulerAngles.z, targetRotateCard, 0.1f)
            .setEase(LeanTweenType.easeInQuad)
            .setOnUpdate((float val) =>
            {
               transform.localEulerAngles = new Vector3(0, 0, val); 
            })
            .setOnComplete(() =>
            {
                board.ShowValidCells();
            });

        LeanTween.value(attackUI[0].transform.localEulerAngles.z, targetRotateAttack, 0.1f)
            .setEase(LeanTweenType.easeInQuad)
            .setOnUpdate((float val) =>
            {
                foreach(TextMeshProUGUI attack in attackUI)
                {
                    attack.transform.localEulerAngles = new Vector3(0, 0, val); 
                }
            });
    }

    private void RotateUI()
    {
        string initialDirection = attackUI[0].name;

        for (int i = 0; i < attackUI.Count - 1; i++)
        {
            attackUI[i].name = attackUI[i+1].name;
        }

        attackUI[attackUI.Count - 1].name = initialDirection;
    }

    private void ResetRotation(string[] newAttack)
    {
        transform.localEulerAngles = Vector3.zero;
        for (int i = 0; i < attackUI.Count; i++)
        {
            attackUI[i].transform.localEulerAngles = Vector3.zero; 

            if (i == 0)
            {
                attackUI[i].name = "Top";
                attack["Top"] = int.Parse(newAttack[i]);
            }
            if (i == 1)
            {
                attackUI[i].name = "Right";
                attack["Right"] = int.Parse(newAttack[i]);
            }
            if (i == 2)
            {
                attackUI[i].name = "Bottom";
                attack["Bottom"] = int.Parse(newAttack[i]);
            }
            if (i == 3)
            {
                attackUI[i].name = "Left";
                attack["Left"] = int.Parse(newAttack[i]);
            }

            attackUI[i].text = newAttack[i];
        }
    }

    public IEnumerator Flip(Color newColor)
    {
        LeanTween.value(0.1f, 360f, 0.5f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float val) =>
            {
                transform.localEulerAngles = new Vector3(0, val, 0);

                foreach(TextMeshProUGUI attack in attackUI)
                {
                    attack.transform.localEulerAngles = new Vector3(0, val, 0);
                }

                if (val >= 180f && GetComponent<Image>().color != newColor)
                {
                    GetComponent<Image>().color = newColor;
                }
            });

        LeanTween.value(1f, 1.5f, 0.25f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float val) =>
            {
                transform.localScale = new Vector2(val, val);
            })
            .setLoopPingPong(1);

        yield return new WaitForSeconds(0.25f);
    }

    #endregion
}
