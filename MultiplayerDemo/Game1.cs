using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Concurrent;

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
        //OOPS
        Action<string> _loggingFunc;
        //Func<string> _loggingFunc;


        public void Start()
        {
            _lastUpdate = 0;
            EventBasedNetListener _listener = new EventBasedNetListener();
            _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                string msg = dataReader.GetString(100);
                Console.WriteLine("We got: {0}", msg);
                _loggingFunc(msg);
            };

            _client = new NetManager(_listener);
            _client.Start();
            TryConnect();
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

        public void Update(GameTime gameTime, Keys[] keysPressed)
        {
            
            if (_isConnected)
            {

                // we'll send keypress events to the server (raw, for testing)
                _client.PollEvents();

                NetDataWriter writer = new NetDataWriter();
                foreach (var key in keysPressed)
                {
                    writer.Put((int)key);
                }

                _server.Send(writer, DeliveryMethod.ReliableOrdered);
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
        double messageLifetimeMilliseconds = 5000d;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _messageQueue = new ConcurrentQueue<string>();
        }

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
            netClient.Start();
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

            netClient.Update(gameTime, Keyboard.GetState().GetPressedKeys());

           
            // TODO: Add your update logic here

            base.Update(gameTime);
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

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
