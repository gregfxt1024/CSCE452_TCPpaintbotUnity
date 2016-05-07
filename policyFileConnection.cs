using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Net.Sockets;

public class policyFileConnection
{
    private Socket cSock;
    private int MAX_INC_DATA = 25;

    private const string policyFileRequest = "<policy-file-request/>",
        policyFile =
        @"<?xml version = '1.0'?>
            <cross-domain-policy>
            <allow-access-from domain=""*"" to-ports""*""/>
            </cross-domain-policy>";

    private byte[] policyFileSize = Encoding.UTF8.GetBytes(policyFileRequest);

    public policyFileConnection(Socket s)
    {
        cSock = s;
        ThreadPool.QueueUserWorkItem(new WaitCallback(handleConnection));
    }

    private void handleConnection(object state)
    {
        byte[] message = new byte[MAX_INC_DATA];
        int bytesRead;
        while (true)
        {
            bytesRead = 0;
            try
            {
                //this will stop the thread from doing anything else until data has been received; AKA a blocking socket connection
                bytesRead = cSock.Receive(message, 0, message.Length, SocketFlags.None);
            }
            catch
            {
                // socket error occured
                break;
            }

            //check message
            if (compareMessage(message, bytesRead, policyFileRequest))
            {
                respondToRequest();
            }
        }
    }

    private void respondToRequest()
    {
        cSock.Send(policyFileSize, 0, policyFileSize.Length, SocketFlags.None);
    }

    public bool compareMessage(byte[] mes, int bRead, string wMes)
    {
        string txt = Encoding.UTF8.GetString(mes, 0, bRead);
        byte[] txtArray = Encoding.UTF8.GetBytes(txt);
        byte[] compareTo = Encoding.UTF8.GetBytes(wMes);

        bool result = false;
        try
        {
            for (int i = 0; i < txt.Length; i++)
            {
                if (txtArray[i] == compareTo[i])
                    result = true;
                else
                    return false;
            }
        }
        catch { }

        return result;
    }

}
