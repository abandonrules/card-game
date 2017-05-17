using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System.Collections;

public class PlayerPrefsManager : MonoBehaviour {

    const int PasscodeSize = 16;

    public static string GetRandomCustomId()
    {
        char[] chars = new char[62];
        chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        byte[] data = new byte[1];
        RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
        crypto.GetNonZeroBytes(data);
        data = new byte[PasscodeSize];
        crypto.GetNonZeroBytes(data);

        StringBuilder result = new StringBuilder(PasscodeSize);
        foreach (byte b in data)
        {
            result.Append(chars[b % (chars.Length)]);
        }
        return result.ToString();
    }

    public static string GetPlayerCustomId()
    {
        if (PlayerPrefs.HasKey("CustomId"))
        {
            return PlayerPrefs.GetString("CustomId");
        }

        return null;
    }

    public static void SetPlayerCustomId(string customId)
    {
        PlayerPrefs.SetString("CustomId", customId);
    }
}
