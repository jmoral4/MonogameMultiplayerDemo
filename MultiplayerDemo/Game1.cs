using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using MultiplayerDemo.Shared;

namespace MultiplayerDemo
{

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        LiteNetNetworkClient netClient;
        SpriteFont font;
        Vector2 fontLocation;
        ConcurrentQueue<string> _messageQueue;
        string _lastMessage;

        PlayerData[] _otherPlayers;
        PlayerData _self;
        Texture2D _tile;

        
        

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _messageQueue = new ConcurrentQueue<string>();
            //int our list of other players
            _otherPlayers = new PlayerData[4];
            for (int i =0; i < _otherPlayers.Length; i++)
            {
                //init self
                _otherPlayers[i] = new PlayerData(new Point(32, 64));
                _otherPlayers[i].TextureName = "robit";
                _otherPlayers[i].Location = new Point(0, 0);
                _otherPlayers[i].BoundingBox = new Rectangle(_otherPlayers[i].Location.X, _otherPlayers[i].Location.Y, 32, 64);
                _otherPlayers[i].IsPresent = false;
                _otherPlayers[i].PlayerId = i + 1;
            }

        }

        public PlayerData GetPlayerData() => _self;
        public PlayerData[] GetPlayers() => _otherPlayers;

        public void AddToMessageQueue(string message)
        {
            _messageQueue.Enqueue(message);
        }

        public string GetLastMessage()
        {
            if (_messageQueue.IsEmpty)
            {
                return _lastMessage;
            }

            string temp;
            bool success = _messageQueue.TryDequeue(out temp);
            if (success)
            {
                _lastMessage = temp;
            }

            return _lastMessage;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            fontLocation = Vector2.Zero;
            // TODO: Add your initialization logic here
            netClient = new LiteNetNetworkClient();
            netClient.AddLoggingFunc(AddToMessageQueue);
            netClient.RegisterPlayerHandler(GetPlayerData);
            netClient.RegisterAllPlayersHandler(GetPlayers);
            netClient.Start();
            
            //init self
            _self = new PlayerData(new Point(32,64));
            _self.TextureName = "robit";
            _self.Location = new Point(20, 20);
            _self.BoundingBox = new Rectangle(_self.Location.X, _self.Location.Y, 32, 64);
            //_self.MovementSpeed = 5;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("gameFont");

            _tile = Content.Load<Texture2D>("sandtile1");
            //content manager internally caches so we're preloading to ask for them later
            Content.Load<Texture2D>("robit");


            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //simple movement

            //move down
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                _self.Location.Y -= _self.MovementSpeed;
                SendClientUpdate(PlayerActions.PRESS_UP);
            }

            //move down
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                _self.Location.Y += _self.MovementSpeed;
                SendClientUpdate(PlayerActions.PRESS_DOWN);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                _self.Location.X -= _self.MovementSpeed;
                SendClientUpdate(PlayerActions.PRESS_LEFT);
            }

            //move down
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                _self.Location.X += _self.MovementSpeed;
                SendClientUpdate(PlayerActions.PRESS_RIGHT);
            }

            _self.BoundingBox = new Rectangle(_self.Location, _self.Size);

            //update all active player bounding boxes
            foreach (var player in _otherPlayers)
            {
                if (player.IsPresent)
                {
                    player.BoundingBox = new Rectangle(player.Location, player.Size);
                }
            }

            netClient.Update(gameTime, Keyboard.GetState().GetPressedKeys());

           
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        

        private void SendClientUpdate(PlayerActions playerAction)
        {
            //we send actions rather than direct keys because the player could have remapped the keys to whatever they find useful
            // server only cares about what action the client took.
            netClient.SendClientActions(playerAction);
        }

        protected Texture2D GetTexture(string name)
        {
            return Content.Load<Texture2D>(name);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            spriteBatch.DrawString(font, $"Server Sent:{GetLastMessage()}", fontLocation, Color.Black);

            //draw self 
            spriteBatch.Draw(GetTexture(_self.TextureName), _self.BoundingBox, Color.White);

            //draw any other known players
            foreach (var player in _otherPlayers)
            {
                if (player.IsPresent && player.PlayerId != _self.PlayerId)
                {
                    spriteBatch.Draw(GetTexture(player.TextureName), player.BoundingBox, Color.Green);
                }
            }


            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
