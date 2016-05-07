using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using scMessage;

public class ServerConnection{

    private serverTCP svr;
    public Socket cSock;
    private int MAX_INC_DATA = 512000; // half a megabyte

    public ServerConnection(Socket s, serverTCP sv)
    {
        svr = sv;
        cSock = s;
        ThreadPool.QueueUserWorkItem(new WaitCallback(handleConnection));
    }

    public void handleConnection(object x)
    {
        //broadcast new connection
        output.outToScreen("A client connected from the IP address: " + cSock.RemoteEndPoint.ToString());

        //TODO: send a test message
        svr.sendClientMessage(cSock, new message("testMessage"));

        try
        {
            while (cSock.Connected)
            {
                byte[] sizeInfo = new byte[4];

                int bytesRead = 0,
                    currentRead = 0;

                currentRead = bytesRead = cSock.Receive(sizeInfo);

                while (bytesRead < sizeInfo.Length && currentRead > 0)
                {
                    currentRead =
                        cSock.Receive
                        (
                            sizeInfo,  // message frame, size of income message
                            bytesRead, // offset into the buffer
                            sizeInfo.Length - bytesRead, // max amount to read
                            SocketFlags.None // no socket flag
                         );

                    bytesRead += currentRead;
                }

                //get the message size of imcoming message
                int messageSize = BitConverter.ToInt32(sizeInfo, 0);

                // create a byte array with the correct message size
                byte[] incMessage = new byte[messageSize];

                //begin reading message
                bytesRead = 0; // reset to ensure proper byte read count

                currentRead =
                    bytesRead =
                    cSock.Receive
                    (
                        incMessage, // incoming message
                        bytesRead,
                        incMessage.Length - bytesRead,
                        SocketFlags.None
                    );

                // check if we recieve all data
                while (bytesRead < messageSize && currentRead > 0)
                {
                    currentRead =
                        cSock.Receive
                        (
                            incMessage, // incoming message
                            bytesRead,
                            incMessage.Length - bytesRead,
                            SocketFlags.None
                        );
                    bytesRead += currentRead;
                }

                //all data received, continue
                try
                {
                    message incObject = (message)conversionTools.convertBytesToObject(incMessage);

                    if (incObject != null)
                    {
                        //send data to handler
                        svr.handleClientData(incObject);
                    }
                }
                catch { }


            }
        }
        catch
        { }

        output.outToScreen("The client disconnected from IP address: " + cSock.RemoteEndPoint.ToString());
        cSock.Close();
    }
}
