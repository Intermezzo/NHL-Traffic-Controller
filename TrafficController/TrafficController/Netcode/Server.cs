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

        private Thread _mainReceive, _mainSend, _main;
        private TcpClient _client;
        private LoggerControl _logger;
        private NetworkStream _clientStream;
        
        public  ConcurrentQueue<RPCData> RPCSendQueue {get; private set;}
        public  ConcurrentQueue<RPCData> RPCReceiveQueue {get; private set; }
        public  bool IsStopped { get; private set; }

       

        public Server(LoggerControl logger)
        {
            RPCSendQueue = new ConcurrentQueue<RPCData>();
            RPCReceiveQueue = new ConcurrentQueue<RPCData>();
            _logger = logger;

            _main = new Thread(listenSingleClient);
            _main.Start();
        }

        private void listenSingleClient()
        {
            try 
            {
                _logger.Log(LogType.Notice, "Listening for incoming clients."); 
                TcpListener tcpListener = new TcpListener(IPAddress.Any, 3450);
                tcpListener.Start();
                _client = tcpListener.AcceptTcpClient();
                _logger.Log(LogType.Notice, String.Format("Client {0} connected", _client.Client.LocalEndPoint)); 
                _clientStream = _client.GetStream();

                _mainReceive = new Thread(continiousReceiveLoop);
                _mainSend = new Thread(continiousSendLoop);
                _mainReceive.Start();
                _mainSend.Start();

            }
            catch (Exception e)
            {
                _logger.Log(e);
            }
        }

        private void continiousSendLoop()
        {
            while (!IsStopped)
            {
                //should be a waithandle
                Thread.Sleep(1);


                RPCData newRPC;
                while (RPCSendQueue.TryDequeue(out newRPC))
                {
                    //send all available RPC commands on the queue
                    using (BinaryWriter clientStreamW = new BinaryWriter(new BufferedStream(_clientStream, 1024), Encoding.ASCII))
                    {
                        //should be serialized in RPCData
                        clientStreamW.Write(newRPC.type);
                        clientStreamW.Write(newRPC.arg.Length);
                        clientStreamW.Write(newRPC.arg.ToCharArray());

                        clientStreamW.Flush();
                        _logger.Log(LogType.Spam, String.Format("RPC request send to client:{0}, {1} ", newRPC.type, newRPC.arg));
                    }
                }
            }
        }

        private void continiousReceiveLoop()
        {
            while (!IsStopped)
            {
                //receive packets and add them to the queue.
                using (BinaryReader clientStreamR = new BinaryReader(_clientStream, Encoding.ASCII))
                {
                    //should be deserialized in RPCData
                    byte cmd = clientStreamR.ReadByte();
                    int length = clientStreamR.ReadInt32();
                    string arg = new string(clientStreamR.ReadChars(length));

                    RPCReceiveQueue.Enqueue(new RPCData() { arg = arg, type = (int)cmd });
                    _logger.Log(LogType.Spam, String.Format("RPC request received from client:{0}, {1} ", cmd, arg));
                }
            }
        }

        public void Stop()
        {
            IsStopped = true;
            _main.Abort();
           
            _main.Join();
            _mainSend.Join();
            _mainReceive.Join();

        }


    }
}
