using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace MultiplayerDemo.Shared
{
    public enum PlayerActions
    {
        PRESS_DOWN, PRESS_UP, PRESS_LEFT, PRESS_RIGHT, PRESS_FIRE
    }

    //keep it simple
    public class PlayerData
    {
        //NOTE to self, we'll have to track movement speed on the server..
        public PlayerData(Point size)
        {
            Size = size;
            MovementSpeed = 5;
        }
        // just a simple number to identify the player in the game (players are player 1-4)
        public int PlayerId { get; set; }
        public string TextureName { get; set; }
        public int Health { get; set; }
        public Point Location;
        public Rectangle BoundingBox { get; set; }
        public Vector2 DirectionVector { get; set; }
        public int MovementSpeed { get; set; }
        public readonly Point Size;
        public bool IsPresent { get; set; }

    }
}
