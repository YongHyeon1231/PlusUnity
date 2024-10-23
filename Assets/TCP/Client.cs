using System;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using System.Text;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

public class Client : MonoBehaviour
{
    TcpListener listener = null;
    TcpClient client = null;
    NetworkStream stream = null;

    Thread thread;
    private Queue<UnityAction> actionBuffer = new Queue<UnityAction>();

    public string SERVER_IP = "43.201.27.61";
    public int SERVER_PORT = 5555;
    public int HEADER_LENGTH = 6;
    public short HANDLER_ID = 10;

    public Text text;

    private void Awake()
    {
        ConnectToServer();
    }

    void Update()
    {
        if (actionBuffer.Count > 0)
        {
            actionBuffer.Dequeue().Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendMessageToServer("a");
        }
    }

    private void ConnectToServer()
    {
        try
        {
            client = new TcpClient(SERVER_IP, SERVER_PORT);
            stream = client.GetStream();
            Debug.Log("Connected to server.");

            thread = new Thread(new ThreadStart(ListenForData));
            thread.IsBackground = true;
            thread.Start();
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException: " + e.ToString());
        }
    }

    private void ListenForData()
    {
        try
        {
            byte[] bytes = new byte[1024];
            while (true)
            {
                if (stream.DataAvailable)
                {
                    int length;
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        length -= HEADER_LENGTH;
                        var incomingData = new byte[length];
                        Array.Copy(bytes, 6, incomingData, 0, length);
                        string serverMessage = Encoding.UTF8.GetString(incomingData);
                        actionBuffer.Enqueue(delegate { OnSetText(serverMessage);});
                        Debug.Log("Server message received: " + serverMessage);
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    public void SendMessageToServer(string message)
    {
        if (client == null || !client.Connected)
        {
            Debug.LogError("Client not connected to server.");
            return;
        }

        byte[] data = SetPacket(message);
        stream.Write(data, 0, data.Length);
        Debug.Log("Sent message to server: " + message);
    }

    byte[] SetPacket(string data)
    {
        byte[] header_length = BitConverter.GetBytes(data.Length);
        Array.Reverse(header_length);
        byte[] header_id = BitConverter.GetBytes(HANDLER_ID);
        Array.Reverse(header_id);

        byte[] m_header = header_length.Concat(header_id).ToArray();

        byte[] body = Encoding.UTF8.GetBytes(data);

        byte[] packet = m_header.Concat(body).ToArray();
        return packet;
    }

    public void OnSetText(string str)
    {
        if(text != null)
        {
            text.text = str;
        }
    }

    private void OnApplicationQuit()
    {
        stream.Close();
        client.Close();
        listener.Stop();
        thread.Abort();
    }
}
