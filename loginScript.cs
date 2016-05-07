using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using scMessage;
using System;

public class loginScript : MonoBehaviour {

    private int
        sPort = 3000,
        pfrPort = 2999;

    private Socket
        cSock;

    private string
        ipAddress = "127.0.0.1";

    public bool
        connectedToServer = false;

    private List<message>
        incMessages = new List<message>();

    public static bool serverflag = false; 

    public static loginScript instance;

    public roboScript robo;

    public static loginScript Instance
    {
        get
        {
            return instance;
        }
    }

    public bool isServer()
    {
        return serverflag;
    }

    void Awake()
    {
        instance = this;
    }

    void OnGUI()
    {
        GUI.Box(new Rect(0, 90, 100, 120), "Server Control");

        if (!connectedToServer && !serverflag)
        {
            if (GUI.Button(new Rect(0, 145, 100, 25), "Connect"))
            {
                connect();
            }

            ipAddress = GUI.TextField(new Rect(0, 175, 100, 25), ipAddress);

            if (GUI.Button(new Rect(0, 115, 100, 25), "Start Server"))
            {
                serverflag = true;
                startServer();
            }

        } else if(connectedToServer)
        {
            GUI.Label(new Rect(0, 115, 100, 25), "Connected");
            if (GUI.Button(new Rect(0, 145, 100, 25), "Disconnect"))
            {
                OnApplicationQuit();
            }
        }
        
        if(serverflag)
        {
            GUI.Label(new Rect(5, 115, 100, 25), "Server Started");
            if (GUI.Button(new Rect(0, 145, 100, 25), "Close server"))
            {
                serverflag = false;
                OnApplicationQuit();
            }
        }
    }

    private void startServer()
    {
        // start the server
        serverTCP startTCP = robo.gameObject.AddComponent<serverTCP>();
        robo.ourServ = startTCP;
        startTCP.robo = robo;
    }

    private void OnApplicationQuit()
    {
        try
        {
            cSock.Close();
        }
        catch { }

        
    }

    private void connect()
    {
        try
        {
            // get policy file

            if ((Application.platform == RuntimePlatform.WindowsWebPlayer) || (Application.platform == RuntimePlatform.WindowsEditor))
            {
                Security.PrefetchSocketPolicy(ipAddress, pfrPort);
            }

            cSock = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
            cSock.Connect(new IPEndPoint(IPAddress.Parse(ipAddress),sPort));
            clientConnection serv = new clientConnection(cSock);
        }
        catch { }
    }

    public void onConnect()
    {
        connectedToServer = true;

        //test the connection
        sendServerMessage(new message("Test Connection"));
    }

    public void addMessageToQue(message incMes)
    {
        incMessages.Add(incMes);
    }

    void Update()
    {
        if(incMessages.Count > 0)
        {
            doMessage();
        }
    }

    private void doMessage()
    {
        List<message> completedMessages = new List<message>();
        for(int i = 0; i < incMessages.Count;i++)
        {
            try
            {
                handleData(incMessages[i]);
                completedMessages.Add(incMessages[i]);
            }
            catch { }
        }

        for (int i = 0; i < completedMessages.Count; i++)
        {
            try
            {
                incMessages.Remove(completedMessages[i]);
            }
            catch { }
        }
    }

    private void handleData(message message)
    {
        Debug.Log("The server send a message: " + message.messageText );
    }

    public void sendServerMessage(message mes)
    {
       if(connectedToServer)
        {
            try
            {
                byte[] mesObj = conversionTools.convertObjectToBytes(mes);
                byte[] readyToSend = conversionTools.wrapMessage(mesObj);

                cSock.Send(readyToSend);
            }
            catch { }
        }
    }
}
