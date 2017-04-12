using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RoomManager : Photon.MonoBehaviour {

	void Awake()
    {
        if (!PhotonNetwork.connected)
        {
            SceneManager.LoadScene(0);
        }
    }

    void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        
    }
}
