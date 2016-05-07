using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using scMessage;

public class clientConnection
{
    public Socket sSock;
    private int MAX_INC_DATA = 512000;

    public clientConnection(Socket s)
    {
        sSock = s;
        ThreadPool.QueueUserWorkItem(new WaitCallback(handleConnection));
    }

    private void handleConnection(object x)
    {
        Debug.Log("Connected to Server");

        loginScript.Instance.onConnect();

        try
        {
            while(sSock.Connected)
            {
                byte[] sizeInfo = new byte[4];

                int bytesRead = 0, currentRead = 0;

                currentRead = bytesRead = sSock.Receive(sizeInfo);

                while(bytesRead < sizeInfo.Length && currentRead >0)
                {
                    currentRead = sSock.Receive(sizeInfo, bytesRead, sizeInfo.Length - bytesRead,SocketFlags.None);
                    bytesRead += currentRead;
                }

                int messageSize = BitConverter.ToInt32(sizeInfo,0);
                byte[] incMessage = new byte[messageSize];

                bytesRead = 0;
                currentRead = bytesRead = sSock.Receive(incMessage, bytesRead, incMessage.Length - bytesRead, SocketFlags.None);

                while(bytesRead < messageSize && currentRead >0)
                {
                    currentRead = sSock.Receive(incMessage, bytesRead, incMessage.Length - bytesRead, SocketFlags.None);
                    bytesRead += currentRead;
                }

                try
                {
                    message incMes = (message)conversionTools.convertBytesToObject(incMessage);

                    if(incMes != null)
                    {
                        loginScript.Instance.addMessageToQue(incMes);
                   
                    }
                }
                catch { }
            }
        }
        catch { }

        Debug.Log("Disconnected from the server");

        loginScript.Instance.connectedToServer = false;

        sSock.Close();
    }
}
