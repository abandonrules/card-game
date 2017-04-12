using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AppPaused : MonoBehaviour {

	public static bool isPaused;

    void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            isPaused = true;

            if (SceneManager.GetActiveScene().name == "Test_Menu")
            {
                if (PhotonNetwork.connected)
                {
                    NetworkManager.DisconnectFromServer();
                }
            }
        }
        else
        {
            isPaused = false;
        }
    }
}
