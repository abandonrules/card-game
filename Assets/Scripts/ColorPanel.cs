using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ColorPanel : MonoBehaviour {

    [SerializeField]
    private GameObject colorPrefab;
    [SerializeField]
    private GameObject colorParent;
    private int offset = 150;
    private float lowerBounds
    {
        get
        {
            return GetLowerBounds();
        }
    }
    private float upperBounds
    {
        get
        {
            return GetUpperBounds();
        }
    }
    private bool canScroll;
    private float speed = 2000;
    private List<Color> presetColors = new List<Color>()
    {
        new Color(1f, 0.74f, 0.74f),
        new Color(0.74f, 1f, 0.74f),
        new Color(0.74f, 0.74f, 1f),
        new Color(1f, 0.74f, 1f)
        //new Color(1f, 1f, 0.74f),
        //new Color(0.74f, 1f, 1f)
    };
    public List<Card> colorObjects;
    public Color selectedColor
    {
        get
        {
            return GetSelectedColor();
        }
    }

    void Start()
    {
        for (int i = 0; i < presetColors.Count; i++)
        {
            GameObject colorGO = Instantiate(colorPrefab, Vector3.zero, Quaternion.identity, colorParent.transform) as GameObject;
            colorObjects.Add(colorGO.GetComponent<Card>());
            colorGO.name = presetColors[i].ToString();
            colorGO.GetComponent<Image>().color = presetColors[i];
            colorGO.transform.localPosition = new Vector2(0, offset * -i);
            colorGO.transform.localScale = Vector2.one;
            colorGO.GetComponent<Selectable>().enabled = true;

            foreach (Transform child in colorGO.transform)
            {
                Destroy(child.gameObject);
            }
        }

        SetSelectedColor();
    }

    private float GetLowerBounds()
    {
        return offset * (presetColors.Count - 1);
    }

    private float GetUpperBounds()
    {
        return 0;
    }

    public void ToggleScroll()
    {
        canScroll = !canScroll;
    }

    void Update()
    {
        #if UNITY_EDITOR || UNITY_STANDALONE

            if (canScroll)
            {
                ScrollColors();
            }

        #endif

        #if UNITY_IOS

            SwipeColors();

        #endif
    }

    // Get selected color from player props
    private Color GetSelectedColor()
    {
        foreach (Card colorGO in colorObjects)
        {
            if (colorGO.GetComponent<Outline>().isActiveAndEnabled)
            {
                return colorGO.GetComponent<Image>().color;
            }
        }

        return colorObjects[0].GetComponent<Image>().color;
    }

    private void SetSelectedColor()
    {
        // Check player props first
        colorObjects[0].GetComponent<Outline>().enabled = true;
    }

    public void MoveToColor(float colorPos)
    {
        float newY = colorPos * -1;

        if (LeanTween.isTweening(colorParent))
        {
            LeanTween.cancel(colorParent);
        }

        LeanTween.moveLocal(colorParent, new Vector3(colorParent.transform.localPosition.x, newY, colorParent.transform.localPosition.z), 0.1f)
            .setEase(LeanTweenType.easeInQuad);
    }

    private void ScrollColors()
    {
        #if UNITY_EDITOR_WIN

            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
            {
                MoveUp();
            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
            {
                MoveDown();   
            }

        #elif UNITY_EDITOR_OSX || UNITY_STANDALONE

            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                if (colorParent.transform.localPosition.y >= upperBounds)
                {
                    MoveUp();
                }
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                if (colorParent.transform.localPosition.y <= lowerBounds)
                {
                    MoveDown(); 
                }
            }

        #endif
    }

    private void SwipeColors()
    {
        
    }

    private void MoveDown()
    {
        float newY = colorParent.transform.localPosition.y + speed * Time.deltaTime;
        Mathf.Clamp(newY, lowerBounds, upperBounds);
        colorParent.transform.localPosition = new Vector2(colorParent.transform.localPosition.x, newY);
    }

    private void MoveUp()
    {
        float newY = colorParent.transform.localPosition.y - speed * Time.deltaTime;
        Mathf.Clamp(newY, lowerBounds, upperBounds);
        colorParent.transform.localPosition = new Vector2(colorParent.transform.localPosition.x, newY);
    }
}
