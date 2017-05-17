using UnityEngine;
using System.Collections;

public class Parallax : MonoBehaviour {

    public enum Direction
    {
        Horizontal,
        Diagonal
    };
    public Direction direction;

    public Transform foreground;
    public Transform background;

	void Awake()
    {
        if (direction.ToString() == "Diagonal")
        {
            Move(new Vector3(0, 0, 45f), new Vector3(10.86f, 10.86f, 0));
        }
        else if (direction.ToString() == "Horizontal")
        {
            Move(Vector3.zero, new Vector3(30f, 0, 0));
        }
    }

    void Move(Vector3 rotation, Vector3 targetPosition)
    {
        foreground.localEulerAngles = rotation;
        background.localEulerAngles = rotation;

        LeanTween.moveLocal(foreground.gameObject, targetPosition, 40f)
            .setEase(LeanTweenType.linear)
            .setLoopCount(-1);

        LeanTween.moveLocal(background.gameObject, targetPosition, 60f)
            .setEase(LeanTweenType.linear)
            .setLoopCount(-1);
    }
}
