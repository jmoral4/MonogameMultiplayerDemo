using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Threading;
using Neutrino.Core;

namespace MultiplayerServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // let's get our bearings first
            SimpleLiteNetServer server = new SimpleLiteNetServer();
            server.Start();

            Console.WriteLine("Server Shutdown");
            //pause before exit
            Console.ReadKey(); 
        }
    }

    class SimpleLiteNetServer {
        Random r = new Random();
        public void Start() {
            EventBasedNetListener listener = new EventBasedNetListener();
            NetManager server = new NetManager(listener);
            server.Start(9050 /* port */);

            listener.ConnectionRequestEvent += request =>
            {
                if (server.PeersCount < 10 /* max connections */)
                    request.AcceptIfKey("SomeConnectionKey");
                else
                    request.Reject();
            };

            listener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine("We got connection: {0}", peer.EndPoint); // Show peer ip
                NetDataWriter writer = new NetDataWriter();                 // Create writer class
                writer.Put("Hello client!");                                // Put some string
                peer.Send(writer, DeliveryMethod.ReliableOrdered);             // Send with reliability
            };

            listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;

            while (!Console.KeyAvailable)
            {
                server.PollEvents();
                
                Thread.Sleep(15);
            }
            server.Stop();

        }

        private void Listener_NetworkReceiveEvent(LiteNetLib.NetPeer peer, NetDataReader reader, DeliveryMethod deliveryMethod)
        {
            if (reader.AvailableBytes > 0)
            {
                Console.WriteLine($"{DateTime.Now.ToLongTimeString()} Recv: {reader.AvailableBytes} from client {peer.ConnectId} method {deliveryMethod}");
                //lets reply back with whatever

                NetDataWriter writer = new NetDataWriter();
                writer.Put("Got it." + r.Next(0, 1000));
                peer.Send(writer, DeliveryMethod.ReliableOrdered);
            }
        }
    }

}
