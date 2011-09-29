using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Net;

namespace TrafficController
{
    public struct RPCData
    {
        public string arg;
        public int type; // should be enum
    }


    class Server
    {
        private const int C_PORT = 3450;

        private Thread _main;
        private TcpClient client;
        public  ConcurrentQueue<RPCData> RPCSendQueue {get; private set;}
        public  ConcurrentQueue<RPCData> RPCReceiveQueue {get; private set; }

        Server()
        {
            RPCSendQueue = new ConcurrentQueue<RPCData>();
            RPCReceiveQueue = new ConcurrentQueue<RPCData>();

            _main = new Thread(listenSingleClient);
        }

        private void listenSingleClient()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 3450);
            tcpListener.Start();
            client = tcpListener.AcceptTcpClient();
            NetworkStream clientStream = client.GetStream();

            while (client.Connected)
            {
                //should be a waithandle
                Thread.Sleep(1);


                RPCData newRPC;
                while (RPCSendQueue.TryDequeue(out newRPC))
                {
                    //send all available RPC commands on the queue
                    using (BinaryWriter clientStreamW = new BinaryWriter(new BufferedStream(clientStream, 1024), Encoding.ASCII))
                    {
                        //should be serialized in RPCData
                        clientStreamW.Write(newRPC.type);
                        clientStreamW.Write(newRPC.arg.Length);
                        clientStreamW.Write(newRPC.arg.ToCharArray());

                        clientStreamW.Flush();
                    }
                }

                if (!clientStream.DataAvailable)
                    continue;

                //receive packets and add them to the queue.
                using (BinaryReader clientStreamR = new BinaryReader(clientStream, Encoding.ASCII))
                {
                    //should be deserialized in RPCData
                    byte cmd = clientStreamR.ReadByte();
                    int length = clientStreamR.ReadInt32();
                    string arg = new string(clientStreamR.ReadChars(length));

                    RPCReceiveQueue.Enqueue(new RPCData() { arg = arg, type = (int)cmd });
                }


            }
        }


    }
}
