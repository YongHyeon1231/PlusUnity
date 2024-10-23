using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandlerUI : MonoBehaviour
{
    public Client client;
    public InputField inputField;

    public void OnClickHandler10()
    {
        client.HANDLER_ID = 10;
        client.SendMessageToServer(inputField.text);
    }

    public void OnClickHandler11()
    {
        client.HANDLER_ID = 11;
        client.SendMessageToServer(inputField.text);
    }

    public void OnClickHandler12()
    {
        client.HANDLER_ID = 12;
        client.SendMessageToServer(inputField.text);
    }
}
