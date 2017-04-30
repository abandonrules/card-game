using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Ping : MonoBehaviour {

	void Update()
    {
        GetComponent<TextMeshProUGUI>().text = "Ping: " + PhotonNetwork.GetPing().ToString();
    }
}
