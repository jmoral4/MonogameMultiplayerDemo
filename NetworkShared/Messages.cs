using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerDemo.Shared
{
    //trying to create a very simple snapshot message
    // expected number of players plus ever location
    public class ServerSnapshotPacket
    {
        public short Players { get; set; }
        public int[] X {get;set;}
        public int[] Y { get;set;}

    }

    public class UpdatePacket
    {
        public int PlayerAction { get; set; }
    }

    // sent to player on initial connection
    public class WelcomePacket
    {
        public string PlayerId { get; set; }
        public int PlayerNumber { get; set; }
        public int XStart { get; set; }
        public int YStart { get; set; }
    }


    //test bouncing this back and forth
    public class EchoPacket
    {
        public int ClientDirection { get; set; }

    }

}
