 using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Pong
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _doubleBuffer;
        private Rectangle _renderRectangle;
        private Texture2D _texture;

        private Rectangle _ball;
        private Point _ballVelocity;
        private bool _lastPointSide = true;
        private readonly Random _rand;

        private Rectangle[] _paddles;
        public enum GameState { Idle, Start, Play, CHeckEnd }
        private GameState _gamestate;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            this.TargetElapsedTime = new TimeSpan(333333);
            Window.AllowUserResizing = true;

            _gamestate = GameState.Idle;

            _rand = new Random();
        }

        protected override void Initialize()
        {
            _doubleBuffer = new RenderTarget2D(GraphicsDevice, 640, 480);
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;

            _graphics.IsFullScreen = false;

            _graphics.ApplyChanges();

            Window.ClientSizeChanged += OnWindowSizeChange;
            OnWindowSizeChange(null, null);

            ResetBall();


            base.Initialize();
        }
        private void AIPaddle(int index)
        {
            var delta = _ball.Y + _ball.Height / 2 - (_paddles[index].Y + _paddles[index].Height / 2);
            _paddles[index].Y += delta;
        }

        private bool PaddleCheck(int index, int x, int y)    //AABB
        {
            return x <= _paddles[index].X + _paddles[index].Width &&
                x + _ball.Width >= _paddles[index].X &&
                y <= _paddles[index].Y + _paddles[index].Height &&
                y + _ball.Height >= _paddles[index].Y;


        }

        private bool PaddleBallCheck()
        {
            float delta = 0;
            int pad = 0;

            if (_ballVelocity.X > 0 && _ball.X + _ball.Width > _paddles[1].X)
            {
                delta = _ball.X + _ball.Width - _paddles[1].X;
                if (delta > _ballVelocity.X + _ball.Width) return false;
                pad = 1;
            }
            else if (_ballVelocity.X < 0 && _ball.X < _paddles[0].X + _paddles[0].Width)
            {
                delta = _ball.X - (_paddles[0].X + _paddles[0].Width);
                if (delta < _ballVelocity.X)
                {
                    return false;
                }
                pad = 0;
            }
            else return false;

            float deltaTime = delta / _ballVelocity.X;
            int collY = (int)(_ball.Y - _ballVelocity.Y * deltaTime);
            int collX = (int)(_ball.X - _ballVelocity.X * deltaTime);

            //check for collision
            if (PaddleCheck(delta < 0? 0 : 1,collX,collY))
            {
                _ball.X = collX;
                _ball.Y = collY;

                _ballVelocity.X = -(_ballVelocity.X + Math.Sign(_ballVelocity.X));

                var diffy = (collY + _ball.Height / 2) - (_paddles[pad].Y + _paddles[pad].Height / 2);

                diffy /= _paddles[pad].Height / 8;

                diffy -= Math.Sign(diffy);
                return true;

            }

            return false;
        }

        private int MoveBall(bool bounceOffSide)
        {
            _ball.X += _ballVelocity.X;
            _ball.Y += _ballVelocity.Y;

            if (_ball.Y < 0)
            {
                _ball.Y = -_ball.Y;
                _ballVelocity.Y = -_ballVelocity.Y;
            }

            if (_ball.Y + _ball.Height > _doubleBuffer.Height)
            {
                _ball.Y = _doubleBuffer.Height - _ball.Height - (_ball.Y + _ball.Height - _doubleBuffer.Height);
                _ballVelocity.Y = -_ballVelocity.Y;
            }

            if (_ball.X < 0)
            {
                if (bounceOffSide)
                {
                    _ball.X = 0;
                    _ballVelocity.X = -_ballVelocity.X;
                }
                else return -1;

                if(_ball.X + _ball.Width > _doubleBuffer.Width)
                {
                    if (bounceOffSide)
                    {
                        _ball.X = _doubleBuffer.Width - _ball.Width;
                        _ballVelocity.X = -_ballVelocity.X;
                    }
                    else return 1;
                }


               

            }

            return 0;
        }
        private void ResetBall()
        {
            _ball = new Rectangle(_doubleBuffer.Width / 2 - 4, _doubleBuffer.Height / 2 - 4, 8, 8);
            _ballVelocity = new Point(_lastPointSide ? _rand.Next(2, 7) : -_rand.Next(2, 7),
                _rand.Next() > int.MaxValue / 2 ? _rand.Next(2, 7) : -_rand.Next(2, 7));
        }

        public void OnWindowSizeChange(object sender, EventArgs e)
        {
            var width = Window.ClientBounds.Width;
            var height = Window.ClientBounds.Height;

            if (height < width / (float)_doubleBuffer.Width * _doubleBuffer.Height)
            {
                width = (int)(height / (float)_doubleBuffer.Height * _doubleBuffer.Width);
            }
            else height = (int)(width / (float)_doubleBuffer.Width * _doubleBuffer.Height);

            var x = (Window.ClientBounds.Width - width) / 2;
            var y = (Window.ClientBounds.Height - height) / 2;
            _renderRectangle = new Rectangle(x, y, width, height);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _texture = new Texture2D(GraphicsDevice, 1, 1);
            Color[] data = new Color[1];
            data[0] = Color.White;
            _texture.SetData(data);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (_gamestate)
            {
                case GameState.Idle:
                    MoveBall(true);
                    _gamestate = GameState.Start;
                    break;
                case GameState.Start:
                    ResetBall();
                    _paddles = new Rectangle[]
                    {
                        new Rectangle(32, _doubleBuffer.Height/2-16, 8, 32),
                        new Rectangle(_doubleBuffer.Width -40, _doubleBuffer.Height / 2- 16, 8, 32)
                    };
                    _gamestate = GameState.Play;
                    break;
                case GameState.Play:
                    var scored = MoveBall(false);

                    AIPaddle(0);
                    AIPaddle(1);
                    PaddleBallCheck();

                    if (scored == 1)
                    {
                        //left side scored on right
                        _lastPointSide = true;
                        _gamestate = GameState.CHeckEnd;

                    }

                    if (scored == -1)
                    {
                        _lastPointSide = false;
                        _gamestate = GameState.CHeckEnd;
                    }

                    break;
                case GameState.CHeckEnd:
                    ResetBall();
                    _gamestate = GameState.Play;
                    break;
                default:
                    _gamestate = GameState.Idle;
                    break;
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_doubleBuffer);
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();
            for(int i = 0; i < 31; i++)
            {
                _spriteBatch.Draw(_texture, 
                    new Rectangle(_doubleBuffer.Width / 2 , i * _doubleBuffer.Height / 31,
                    2, _doubleBuffer.Height / 62), Color.White);
            }

            switch (_gamestate)
            {
                case GameState.Idle:
                    _spriteBatch.Draw(_texture, _ball, Color.White);
                    break;
                case GameState.Start:
                    break;
                case GameState.Play:
                    
                case GameState.CHeckEnd:
                    _spriteBatch.Draw(_texture, _ball, Color.White);

                    _spriteBatch.Draw(_texture, _paddles[0], Color.White);
                    _spriteBatch.Draw(_texture, _paddles[1], Color.White);
                    break;
            }
            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            _spriteBatch.Draw(_doubleBuffer, _renderRectangle, Color.White);
            _spriteBatch.End();
            

            base.Draw(gameTime);
        }
    }
}
