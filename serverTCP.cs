using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using scMessage;

public class serverTCP : MonoBehaviour{
    private static int
            clientPort = 3000, policyFilePort = 2999;

    private static Socket
        policyFileListenSocket, clientListenSocket;

    private static List<clientConnection>
        clients = new List<clientConnection>();

    public roboScript robo;
    public bool moveRight = false;
    public bool moveLeft = false;
    public bool arm2clock = false;
    public bool arm2count = false;
    public bool arm3clock = false;
    public bool arm3count = false;
    public bool yPlus = false;
    public bool yMinus = false;
    public bool paint = false;
    public bool isDelayed = false;
    public bool isInvoke = false;
    public char message = ' ';

    public serverTCP()
    {
        try
        {
            //listen for policy requests
            policyFileListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            policyFileListenSocket.Bind(new IPEndPoint(IPAddress.Any, policyFilePort));
            policyFileListenSocket.Listen(int.MaxValue);
            ThreadPool.QueueUserWorkItem(new WaitCallback(listenForPFR));

            //listen for clients
            clientListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientListenSocket.Bind(new IPEndPoint(IPAddress.Any, clientPort));
            clientListenSocket.Listen(int.MaxValue);
            ThreadPool.QueueUserWorkItem(new WaitCallback(listenForClients));

            output.outToScreen("waiting for client policy file requests on port" + policyFilePort + " and clients on port " + clientPort);
        }
        catch { }
    }

    private void listenForClients(object x)
    {
        while (loginScript.serverflag)
        {
            Socket cSocket = clientListenSocket.Accept();
            ServerConnection newCon = new ServerConnection(cSocket, this);
        }
    }

    private void listenForPFR(object x)
    {
        while (loginScript.serverflag)
        {
            Socket pfRequest = policyFileListenSocket.Accept();
            policyFileConnection newRequest = new policyFileConnection(pfRequest);
        }
    }

    public void updateFlags()
    {
        if (message == '0')
        {
            moveLeft = true;
        }
        else if (message == '1')
        {
            moveRight = true;
        }
        else if (message == '2')
        {
            arm2clock = true;
        }
        else if (message == '3')
        {
            arm2count = true;
        }
        else if (message == '4')
        {
            arm3clock = true;
        }
        else if (message == '5')
        {
            arm3count = true;
        }
        else if (message == '6')
        {
            yPlus = true;
        }
        else if (message == '7')
        {
            yMinus = true;
        }
        else if (message == '8')
        {
            paint = true;
        }
        else if (message == '9')
        {
            isDelayed = !isDelayed;
        }
    }

    public void handleClientData(message incObject)
    {
        output.outToScreen("the client sent a message: " + incObject.messageText);

        message = incObject.messageText[0];

        if (isDelayed)
            isInvoke = true;
        else
            updateFlags();
    }

    public void sendClientMessage(Socket cSock, message mes)
    {
        try
        {
            // convert message into byte array, wrap the message , then send it.
            byte[] messageObject = conversionTools.convertObjectToBytes(mes);
            byte[] readyToSend = conversionTools.wrapMessage(messageObject);
            cSock.Send(readyToSend);
        }
        catch { }
    }

}
