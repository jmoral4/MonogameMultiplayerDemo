using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Concurrent;
using MultiplayerDemo.Shared;
using System.Collections.Generic;

namespace MultiplayerDemo
{
    public class LiteNetNetworkClient
    {
        NetManager _client;
        EventBasedNetListener _listener;
        bool _isConnected;
        NetPeer _server;
        double _lastUpdate;
        const int RETRY_MILLISECONDS = 5000;        
        Action<string> _loggingFunc;
        string selfId;
        readonly NetSerializer _networkSerializer = new NetSerializer();
        readonly NetPacketProcessor _packetProcessor = new NetPacketProcessor();

        Func<PlayerData> _playerHandler;
        Func<PlayerData[]> _allPlayers;

        public LiteNetNetworkClient()
        {
            _packetProcessor.SubscribeReusable<WelcomePacket, NetPeer>(WelcomeHandler);
            _packetProcessor.SubscribeReusable<EchoPacket>(EchoHandler);
            _packetProcessor.SubscribeReusable<ServerSnapshotPacket>(SnapshotHandler);
        }

        public void Start()
        {
            _listener = new EventBasedNetListener();
            _lastUpdate = 0;


            _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                // the data reader is a ONE WAY reader similar to a SQL data reader. If you READ from it, you move the pointer forward. 
                // never do the line below or you'll have painful debugging sessions...
                 // ... don't do this unless we _only_ want to display contents -  Console.WriteLine("Server Sent: {0}", dataReader.GetString(100 /* max length of string */));     
                _packetProcessor.ReadAllPackets(dataReader, fromPeer);
            };
            _client = new NetManager(_listener);            
            _client.Start();
            TryConnect();
        }

        public void SnapshotHandler(ServerSnapshotPacket sp)
        {
            if (sp != null)
            {
                Console.Write($"Server snapshot!:");
                for (int i = 0; i < sp.Players; i++)
                {
                    Console.Write($"P[{i+1}] is X{sp.X[i]},Y{sp.Y[i]}");

                    //update all players
                    var players = _allPlayers();
                    players[i].Location = new Point(sp.X[i], sp.Y[i]);
                    players[i].IsPresent = true;
                }
                Console.Write("\n");




            }
        }

        public void WelcomeHandler(WelcomePacket wp, NetPeer peer)
        {
            if (wp != null)
            {
                Console.WriteLine($"We got: {wp.PlayerId+1} from {peer.EndPoint.ToString()}");
                _loggingFunc?.Invoke(wp.PlayerId+1);
                selfId = wp.PlayerId;


                //get player handle
                var player = _playerHandler?.Invoke();
                player.PlayerId = wp.PlayerNumber;
                player.Location = new Point(wp.XStart, wp.YStart);                
                Console.WriteLine($"Player moved to new start location:{wp.XStart},{wp.YStart}");
            }
            else
            {
                Console.WriteLine("Packet was malformed!");
            }
        }

        public void EchoHandler(EchoPacket echo)
        {            
            if (echo != null)
            {
                Console.WriteLine($"Server Echoed: {echo.ClientDirection}");
            }
        }

        public void RegisterPlayerHandler(Func<PlayerData> func)
        {
            _playerHandler = func;
        }

        public void RegisterAllPlayersHandler(Func<PlayerData[]> func)
        {
            _allPlayers = func;
        }

        public void AddLoggingFunc(Action<string> func)
        {
            _loggingFunc = func;
        }

        public void TryConnect()
        {
           _server =  _client.Connect("localhost" /* host ip or name */, 9050 /* port */, "SomeConnectionKey" /* text key or NetDataWriter */);
            if (_server != null)
            {
                _isConnected = true;
                Console.WriteLine("CONNECTED TO SERVER!");
            }
        }

        public void SendClientActions(PlayerActions actions)
        {
            UpdatePacket up = new UpdatePacket() { PlayerAction = (int)actions };
            _packetProcessor.Send<UpdatePacket>(_server, up, DeliveryMethod.ReliableOrdered);


        }

        NetDataReader reader = new NetDataReader();

        public void Update(GameTime gameTime, Keys[] keysPressed)
        {
            
            if (_isConnected)
            {
                
                _client.PollEvents();                                


            }
            else {
                if ((gameTime.TotalGameTime.TotalMilliseconds - _lastUpdate) < RETRY_MILLISECONDS)
                {
                    TryConnect();
                    _lastUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                }
            }
        }

        public void Stop()
        {
            _server.Disconnect();
            _client.Stop();
            _isConnected = false;
        }
    }
}
