using UnityEngine;
using System.Collections;

public class GlobalManager : MonoBehaviour {

    public static GlobalManager Instance;

    public PlayerState playerState = new PlayerState();
    public PlayerState opponentState = new PlayerState();

    void Awake ()   
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
}
