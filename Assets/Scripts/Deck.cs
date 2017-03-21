using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Deck : MonoBehaviour {

    const int totalTiles = 80;
    public GameManager gameManager;
    public GameObject cardPrefab;
    public List<Card> cardList;
    public int currentCardIndex;

    public List<string> tempAttack;
    private int defaultTop = 1;
    private int defaultRight = 1;
    private int defaultBottom = 1;
    private int defaultLeft = 1;

    public IEnumerator Create()
    {
        yield return StartCoroutine(AssignValues());

        for (int i = 0; i < totalTiles; i++)
        {
            GameObject card = Instantiate(cardPrefab, Vector2.zero, Quaternion.identity, transform) as GameObject;
            card.transform.localPosition = Vector2.zero;
            card.name = i.ToString();

            card.GetComponent<Card>().id = i;
            card.GetComponent<Card>().SetOwner(transform);

            string[] randAttack = tempAttack[i].Split(","[0]);
            card.GetComponent<Card>().attack["top"] = int.Parse(randAttack[0]);
            card.GetComponent<Card>().attack["right"] = int.Parse(randAttack[1]);
            card.GetComponent<Card>().attack["bottom"] = int.Parse(randAttack[2]);
            card.GetComponent<Card>().attack["left"] = int.Parse(randAttack[3]);
            
            cardList.Add(card.GetComponent<Card>());
            foreach(Transform child in card.transform)
            {
                child.GetComponent<Text>().color = Color.clear;
                card.GetComponent<Card>().attackUI.Add(child.GetComponent<Text>());
                card.GetComponent<Card>().SetAttackUI(child, child.name);
            }

            card.GetComponent<Card>().board = gameManager.board;
        }
        SetText(cardList.Count);

        yield return null;
    }

    IEnumerator AssignValues()
    {
        for (int i = 1; i < 500; i++)
        {
            tempAttack.Add(defaultTop.ToString() + "," + defaultRight.ToString() + "," + defaultBottom.ToString() + "," + defaultLeft.ToString());

            if (defaultRight == 5 && defaultBottom == 5 && defaultLeft == 5)
            {
                defaultTop++;
                defaultRight = 1;
                defaultBottom = 1;
                defaultLeft = 1;
                continue;
            }
            else if (defaultBottom == 5 && defaultLeft == 5)
            {
                defaultRight++;
                defaultBottom = 1;
                defaultLeft = 1;
                continue;
            }
            else if (defaultLeft == 5)
            {
                defaultBottom++;
                defaultLeft = 1;
                continue;
            }
            else
            {
                defaultLeft++;
            }
        }

        yield return StartCoroutine(ShuffleTemp());
        yield return null;
    }

    public IEnumerator ShuffleTemp()
    {
        for (int i = 0; i < tempAttack.Count; i++)
        {
            int randomIndex = Random.Range(0, tempAttack.Count);
            string newIndex = tempAttack[randomIndex];
            tempAttack[randomIndex] = tempAttack[i];
            tempAttack[i] = newIndex;
        }
        yield return null;
    }

    public int UpdateTotalCards()
    {
        int totalCards = int.Parse(GetComponent<Text>().text.Substring(6));
        totalCards--;
        currentCardIndex++;
        SetText(totalCards);
        return totalCards;
    }

    public int GetTotalCards()
    {
        int totalCards = 0;

        for (int i = 0; i < cardList.Count; i++)
        {
            if (cardList[i] != null)
            {
                totalCards++;
            }
            else
            {
                currentCardIndex++;
            }
        }
        SetText(totalCards);

        return totalCards;
    }

    void SetText(int value)
    {
        int oldValue = int.Parse(GetComponent<Text>().text.Substring(6));

        LeanTween.value(oldValue, value, 0.1f)
            .setEase(LeanTweenType.easeInQuad)
            .setOnUpdate((float val) =>
            {
                GetComponent<Text>().text = "Deck: " + Mathf.RoundToInt(val).ToString();
            });
    }
}
