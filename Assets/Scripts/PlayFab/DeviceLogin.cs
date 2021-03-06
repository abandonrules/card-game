﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using TMPro;
using PlayFab.ClientModels;

public class DeviceLogin : MonoBehaviour {

    public MenuManager menuManager
    {
        get
        {
            return GetComponent<MenuManager>();
        }
    }

    #region Begin Login services

    public void LoginToPlayFab(string playerCustomId)
    {
        Debug.Log("Logging into PlayFab...");
        ShowConnectingLayer();
        menuManager.infoPanels[3].action = "Login";

        GetPlayerCombinedInfoRequestParams playerInfoRequest = new GetPlayerCombinedInfoRequestParams();
        playerInfoRequest.GetUserData = true;

        #if UNITY_EDITOR

            LoginWithCustomIDRequest request = new LoginWithCustomIDRequest();
            request.CustomId = playerCustomId;
            request.CreateAccount = false;
            request.InfoRequestParameters = playerInfoRequest;

            PlayFabClientAPI.LoginWithCustomID(request, OnPlayFabLoginSuccess, OnPlayFabLoginError);

        #elif UNITY_IOS

            LoginWithIOSDeviceIDRequest request = new LoginWithIOSDeviceIDRequest();
            request.DeviceId = playerCustomId;
            request.OS = SystemInfo.operatingSystem;
            request.DeviceModel = SystemInfo.deviceModel;
            request.CreateAccount = false;
            request.InfoRequestParameters = playerInfoRequest;

            PlayFabClientAPI.LoginWithIOSDeviceID(request, OnPlayFabLoginSuccess, OnPlayFabLoginError);

        #endif
    }

    private void OnPlayFabLoginSuccess(LoginResult result)
    {
        Debug.Log("Login to PlayFab successful.");

        PlayFabInfo.PlayerName = result.InfoResultPayload.UserData["Name"].Value;
        PlayFabInfo.PlayerLevel = int.Parse(result.InfoResultPayload.UserData["Level"].Value);
        PlayFabInfo.PlayerRank = int.Parse(result.InfoResultPayload.UserData["Rank"].Value);
        PlayFabInfo.PlayerColor = result.InfoResultPayload.UserData["Color"].Value;

        HideConnectingLayer();

        //GetPhotonAuthenticationTokenRequest request = new GetPhotonAuthenticationTokenRequest();
        //request.PhotonApplicationId = PhotonNetwork.PhotonServerSettings.AppID.Trim();

        //PlayFabClientAPI.GetPhotonAuthenticationToken(request, OnPhotonAuthenticationSuccess, OnPhotonAuthenticationFailure);
    }

    private void OnPlayFabLoginError(PlayFabError error)
    {
        menuManager.ShowPanel(menuManager.infoPanels[3]);
        Debug.LogError(error.GenerateErrorReport());
    }

    #endregion

    #region Begin Create services

    public void CreatePlayFabAccount(string playerCustomId, string playerName)
    {
        Debug.Log("Creating account...");
        ShowConnectingLayer();
        menuManager.infoPanels[3].action = "Create";
        menuManager.infoPanels[3].playerName = playerName;

        #if UNITY_EDITOR

            LoginWithCustomIDRequest request = new LoginWithCustomIDRequest();
            request.CustomId = playerCustomId;
            request.CreateAccount = true;

            PlayFabClientAPI.LoginWithCustomID(request, OnCreateSuccess, OnCreateError, playerName, null);

        #elif UNITY_IOS

            LoginWithIOSDeviceIDRequest request = new LoginWithIOSDeviceIDRequest();
            request.DeviceId = playerCustomId;
            request.OS = SystemInfo.operatingSystem;
            request.DeviceModel = SystemInfo.deviceModel;
            request.CreateAccount = true;

            PlayFabClientAPI.LoginWithIOSDeviceID(request, OnCreateSuccess, OnCreateError, playerName, null);

        #endif
    }

    private void OnCreateSuccess(LoginResult result)
    {
        Debug.Log("Create account success.");

        Dictionary<string, string> newUserData = new Dictionary<string, string>();
        newUserData.Add("Name", result.CustomData.ToString());
        newUserData.Add("Level", "1");
        newUserData.Add("Rank", "0");
        newUserData.Add("Color", "1,0.74,0.74");

        UpdateUserDataRequest request = new UpdateUserDataRequest();
        request.Data = newUserData;
        request.Permission = UserDataPermission.Public;

        PlayFabClientAPI.UpdateUserData(request, OnUpdateUserDataSuccess, OnUpdateUserDataError);
    }

    private void OnUpdateUserDataSuccess(UpdateUserDataResult result)
    {
        Debug.Log("User data updated.");
        HideConnectingLayer();

        menuManager.ShowPanel(menuManager.infoPanels[4]);
    }

    private void OnUpdateUserDataError(PlayFabError error)
    {
        TextMeshProUGUI errorMessage = menuManager.infoPanels[1].transform.Find("Error Text").GetComponent<TextMeshProUGUI>();
        if (error.Error == PlayFabErrorCode.ServiceUnavailable)
        {
            menuManager.ShowPanel(menuManager.infoPanels[3]);
            menuManager.serverCover.DisableAnimations();
        }
        else
        {
            HideConnectingLayer();
            errorMessage.text = error.ErrorMessage;
        }
        Debug.Log(error.GenerateErrorReport());
    }

    private void OnCreateError(PlayFabError error)
    {
        TextMeshProUGUI errorMessage = menuManager.infoPanels[1].transform.Find("Error Text").GetComponent<TextMeshProUGUI>();
        if (error.Error == PlayFabErrorCode.ServiceUnavailable)
        {
            menuManager.ShowPanel(menuManager.infoPanels[3]);
            menuManager.serverCover.DisableAnimations();
        }
        else
        {
            HideConnectingLayer();
            errorMessage.text = error.ErrorMessage;
        }
        Debug.LogError(error.GenerateErrorReport());
    }

    #endregion

    #region Begin Transfer services

    public void TransferPlayFabAccount(string playerCustomId)
    {
        Debug.Log("Attempting to transfer account...");
        ShowConnectingLayer();
        menuManager.infoPanels[3].action = "Transfer";
        menuManager.infoPanels[3].transferPasscode = playerCustomId;

        GetPlayerCombinedInfoRequestParams playerInfoRequest = new GetPlayerCombinedInfoRequestParams();
        playerInfoRequest.GetUserAccountInfo = true;
        playerInfoRequest.GetUserData = true;

        #if UNITY_EDITOR

            LoginWithCustomIDRequest request = new LoginWithCustomIDRequest();
            request.CustomId = playerCustomId;
            request.CreateAccount = false;
            request.InfoRequestParameters = playerInfoRequest;

            PlayFabClientAPI.LoginWithCustomID(request, OnTransferSuccess, OnTransferError);

        #elif UNITY_IOS

            LoginWithIOSDeviceIDRequest request = new LoginWithIOSDeviceIDRequest();
            request.DeviceId = playerCustomId;
            request.OS = SystemInfo.operatingSystem;
            request.DeviceModel = SystemInfo.deviceModel;
            request.CreateAccount = false;
            request.InfoRequestParameters = playerInfoRequest;

            PlayFabClientAPI.LoginWithIOSDeviceID(request, OnTransferSuccess, OnTransferError);

        #endif
    }

    private void OnTransferSuccess(LoginResult result)
    {
        Debug.Log("Transfer account successful.");

        string newCustomId = PlayerPrefsManager.GetRandomCustomId();
        PlayerPrefsManager.SetPlayerCustomId(newCustomId);

        #if UNITY_EDITOR

            LinkCustomIDRequest request = new LinkCustomIDRequest();
            request.CustomId = newCustomId;
            request.ForceLink = true;

            PlayFabClientAPI.LinkCustomID(request, OnCustomIdTransferLinkSuccess, OnTransferLinkError);

        #elif UNITY_IOS

            LinkIOSDeviceIDRequest request = new LinkIOSDeviceIDRequest();
            request.DeviceId = newCustomId;
            request.OS = SystemInfo.operatingSystem;
            request.DeviceModel = SystemInfo.deviceModel;
            request.ForceLink = true;

            PlayFabClientAPI.LinkIOSDeviceID(request, OnIOSDeviceTransferLinkSuccess, OnTransferLinkError);

        #endif
    }

    private void OnCustomIdTransferLinkSuccess(LinkCustomIDResult result)
    {
        Debug.Log("New transfer passcode linked.");
        HideConnectingLayer();

        menuManager.ShowPanel(menuManager.infoPanels[5]);
    }

    private void OnIOSDeviceTransferLinkSuccess(LinkIOSDeviceIDResult result)
    {
        Debug.Log("New transfer passcode linked.");
        HideConnectingLayer();

        menuManager.ShowPanel(menuManager.infoPanels[5]);
    }

    private void OnTransferError(PlayFabError error)
    {
        TextMeshProUGUI errorMessage = menuManager.infoPanels[2].transform.Find("Error Text").GetComponent<TextMeshProUGUI>();
        if (error.Error == PlayFabErrorCode.ServiceUnavailable)
        {
            menuManager.ShowPanel(menuManager.infoPanels[3]);
            menuManager.serverCover.DisableAnimations();
        }
        else
        {
            HideConnectingLayer();
            errorMessage.text = error.ErrorMessage;
        }
        Debug.LogError(error.GenerateErrorReport());
    }

    private void OnTransferLinkError(PlayFabError error)
    {
        TextMeshProUGUI errorMessage = menuManager.infoPanels[2].transform.Find("Error Text").GetComponent<TextMeshProUGUI>();
        if (error.Error == PlayFabErrorCode.ServiceUnavailable)
        {
            menuManager.ShowPanel(menuManager.infoPanels[3]);
            menuManager.serverCover.DisableAnimations();
        }
        else
        {
            HideConnectingLayer();
            errorMessage.text = error.ErrorMessage;
        }
        Debug.LogError(error.GenerateErrorReport());
    }

    #endregion

    /*
    private void OnPhotonAuthenticationSuccess(GetPhotonAuthenticationTokenResult result)
    {
        Debug.Log("Login to Photon server successfull");

        AuthenticationValues customAuth = new AuthenticationValues();
        customAuth.AuthType = CustomAuthenticationType.Custom;
        customAuth.AddAuthParameter("username", "");
        customAuth.AddAuthParameter("token", result.PhotonCustomAuthenticationToken);

        Debug.Log(result.PhotonCustomAuthenticationToken);
        PhotonNetwork.AuthValues = customAuth;
        Debug.Log(PhotonNetwork.AuthValues.AuthGetParameters);
    }

    private void OnPhotonAuthenticationFailure(PlayFabError error)
    {
        Debug.Log(3);
    }
    */

    private void ShowConnectingLayer()
    {
        LeanTween.pause(FindObjectOfType<Parallax>().foreground.gameObject);
        LeanTween.pause(FindObjectOfType<Parallax>().background.gameObject);

        if (menuManager.serverCover.gameObject.activeSelf)
        {
            menuManager.serverCover.EnableAnimations();
            return;
        }
        menuManager.serverCover.gameObject.SetActive(true);
    }

    private void HideConnectingLayer()
    {
        LeanTween.resume(FindObjectOfType<Parallax>().foreground.gameObject);
        LeanTween.resume(FindObjectOfType<Parallax>().background.gameObject);

        menuManager.serverCover.gameObject.SetActive(false);
    }
}
