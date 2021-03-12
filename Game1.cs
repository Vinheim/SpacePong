using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Ping_Pong
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>  
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // sound effect used in playing bopping bouncy ball sound effects
        SoundEffect ballBounce;

        // song initialized to begin playing with execution of the game
        Song spaceSong;

        // the font to use (trivializes multi digit scores)
        SpriteFont font;

        // the background
        Texture2D spaceBG;

        // the score
        int m_Score1 = 0;
        int m_Score2 = 0;
        Texture2D m_textureNumbers;
        Rectangle[] m_ScoreRect = null;

        // the ball
        Ball m_ball;
        Texture2D m_textureBall;

        // the paddles
        Paddle m_paddle1;
        Paddle m_paddle2;
        Texture2D m_texturePaddle;

        // constants
        const int SCREEN_WIDTH = 640;
        const int SCREEN_HEIGHT = 480;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // use a fixed frame rate of 30 frames per second
            IsFixedTimeStep = true;
            TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 33);

            InitScreen();
            InitGameObjects();

            base.Initialize();
        }

        // screen-related init sc
        public void InitScreen()
        {
            // back buffer
            graphics.PreferredBackBufferHeight = SCREEN_HEIGHT;
            graphics.PreferredBackBufferWidth = SCREEN_WIDTH;
            graphics.PreferMultiSampling = false;
            graphics.ApplyChanges();
        }

        // game-related init tasks
        public void InitGameObjects()
        {
            // create an instance of our ball
            m_ball = new Ball();

            // set the size of the ball
            m_ball.Width = 30.0f;
            m_ball.Height = 30.0f;

            // create 2 instances of our paddle
            m_paddle1 = new Paddle();
            m_paddle2 = new Paddle();

            // set the size of the paddles
            m_paddle1.Width = 50.0f;
            m_paddle1.Height = 75.0f;
            m_paddle2.Width = 50.0f;
            m_paddle2.Height = 75.0f;

            // map the digits in the image to actual numbers
            m_ScoreRect = new Rectangle[100];
            
            for (int i = 0; i < 100; i++)
            {
                m_ScoreRect[i] = new Rectangle(
                    i * 45, // X
                    0,      // Y
                    45,     // Width
                    75);    // Height
            }

            ResetGame();
        }

        // initial play state, called when the game is first
        // run, and whever a player scores 10 goals
        public void ResetGame()
        {
            // reset scores
            m_Score1 = 0;
            m_Score2 = 0;

            // place the ball at the center of the screen
            m_ball.X =
                SCREEN_WIDTH / 2 - m_ball.Width / 2;
            m_ball.Y =
                SCREEN_HEIGHT / 2 - m_ball.Height / 2;

            // set a speed and direction for the ball
            m_ball.DX = 10.0f;
            m_ball.DY = 10.0f;

            // place the paddles at either end of the screen
            m_paddle1.X = 30;
            m_paddle1.Y =
                SCREEN_HEIGHT / 2 - m_paddle1.Height / 2;
            m_paddle2.X =
                SCREEN_WIDTH - 30 - m_paddle2.Width;
            m_paddle2.Y =
                SCREEN_HEIGHT / 2 - m_paddle1.Height / 2;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Texture2D earthBall = Content.Load<Texture2D>(@"media\earth");
            Texture2D spacePaddle = Content.Load<Texture2D>(@"media\shuttle");
            Texture2D starBG = Content.Load<Texture2D>(@"media\stars");
            // SpriteFont font = Content.Load<SpriteFont>(@"media\Score");
            
            // Load legendary chiptune space song 
            spaceSong = Content.Load<Song>("Fortress of Lies");
            MediaPlayer.Play(spaceSong);

            // Load tiny ping pong ball bouncing sound 
            ballBounce = Content.Load<SoundEffect>("PLANETARY EXPLOSION");

            // load images from disk
            LoadGameGraphics();
        }

        // load our textures from disk
        protected void LoadGameGraphics()
        {
            // load background texture for cool stars like the original Final Fantasy 7 opening
            spaceBG =
                Content.Load<Texture2D>(@"media\stars");

            // load the texture for the ball
            m_textureBall = 
                Content.Load<Texture2D>(@"media\earth");
            m_ball.Visual = m_textureBall;

            // load the texture for the paddles
            m_texturePaddle =
                Content.Load<Texture2D>(@"media\shuttle");
            m_paddle1.Visual = m_texturePaddle;
            m_paddle2.Visual = m_texturePaddle;

            // load the texture for the score
            m_textureNumbers =
                Content.Load<Texture2D>(@"media\numbers");
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
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
        protected void Update(GameTime gameTime, SoundEffect ballBounce)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // update the ball's location on the screen
            //MoveBall(ballBounce);
            // update the paddles' locations on the screen
            MovePaddles();

            base.Update(gameTime);
        }

        // move the ball based on it's current DX and DY 
        // settings. check for collisions
        private void MoveBall(SoundEffect ballBounce)
        {
            // actually move the ball
            m_ball.X += m_ball.DX;
            m_ball.Y -= m_ball.DY;

            // did ball touch top or bottom side?
            if (m_ball.Y <= 0 ||
                m_ball.Y >= SCREEN_HEIGHT - m_ball.Height)
            {
                // play ball bounce sound 
                ballBounce.Play();

                // reverse vertical direction
                m_ball.DY *= -1f; 
            }

            // did ball touch the left side?
            if (m_ball.X <= 0)
            {
                // play ball bounce sound
                ballBounce.Play();

                // at higher speeds, the ball can leave the 
                // playing field, make sure that doesn't happen
                m_ball.X = 0;

                // increment player 2's score
                m_Score2++;

                // reduce speed, reverse direction
                m_ball.DX = 5.0f;
                m_ball.DY = 3.5f;
            }

            // did ball touch the right side?
            if (m_ball.X >= SCREEN_WIDTH - m_ball.Width)
            {
                // play ball bounce sound
                ballBounce.Play();
                // at higher speeds, the ball can leave the 
                // playing field, make sure that doesn't happen
                m_ball.X = SCREEN_WIDTH - m_ball.Width;

                // increment player 1's score
                m_Score1++;

                // reduce speed, reverse direction
                m_ball.DX = -5.0f;
                m_ball.DY = -3.5f;
            }

            // reset game if a player scores 100 goals
            if (m_Score1 >= 100 || m_Score2 >= 100)
            {
                ResetGame();
            }

            // did ball hit the paddle from the front?
            if (CollisionOccurred())
            {
                // reverse hoizontal direction
                m_ball.DX *= -1;

                // increase the speed a little.
                m_ball.DX *= 1.15f;
            }
        }

        // check for a collision between the ball and paddles
        private bool CollisionOccurred()
        {
            // assume no collision
            bool retval = false;

            // heading towards player one
            if (m_ball.DX < 0)
            {
                Rectangle b = m_ball.Rect;
                Rectangle p = m_paddle1.Rect;
                retval =
                    b.Left < p.Right &&
                    b.Right > p.Left &&
                    b.Top < p.Bottom &&
                    b.Bottom > p.Top;
            }
            // heading towards player two
            else // m_ball.DX > 0  
            {
                Rectangle b = m_ball.Rect;
                Rectangle p = m_paddle2.Rect;
                retval =
                    b.Left < p.Right &&
                    b.Right > p.Left &&
                    b.Top < p.Bottom &&
                    b.Bottom > p.Top;
            }

            return retval;
        }

        // how much to move paddle each frame
        private const float PADDLE_STRIDE = 10.0f;

        // actually move the paddles
        private void MovePaddles()
        {
            // define bounds for the paddles
            float MIN_Y = 0.0f;
            float MAX_Y = SCREEN_HEIGHT - m_paddle1.Height;

            // get player input
            GamePadState pad1 =
                GamePad.GetState(PlayerIndex.One);
            GamePadState pad2 =
                GamePad.GetState(PlayerIndex.Two);
            KeyboardState keyb =
                Keyboard.GetState();

            // check the controller, PLAYER ONE
            bool PlayerUp =
                pad1.DPad.Up == ButtonState.Pressed;
            bool PlayerDown =
                pad1.DPad.Down == ButtonState.Pressed;

            // also check the keyboard, PLAYER ONE
            PlayerUp |= keyb.IsKeyDown(Keys.W);
            PlayerDown |= keyb.IsKeyDown(Keys.S);

            // move the paddle
            if (PlayerUp)
            {
                m_paddle1.Y -= PADDLE_STRIDE;
                if (m_paddle1.Y < MIN_Y)
                {
                    m_paddle1.Y = MIN_Y;
                }
            }
            else if (PlayerDown)
            {
                m_paddle1.Y += PADDLE_STRIDE;
                if (m_paddle1.Y > MAX_Y)
                {
                    m_paddle1.Y = MAX_Y;
                }
            }

            // check the controller, PLAYER TWO
            PlayerUp =
                pad2.DPad.Up == ButtonState.Pressed;
            PlayerDown =
                pad2.DPad.Down == ButtonState.Pressed;

            // also check the keyboard, PLAYER TWO
            PlayerUp |= keyb.IsKeyDown(Keys.Up);
            PlayerDown |= keyb.IsKeyDown(Keys.Down);

            // move the paddle
            if (PlayerUp)
            {
                m_paddle2.Y -= PADDLE_STRIDE;
                if (m_paddle2.Y < MIN_Y)
                {
                    m_paddle2.Y = MIN_Y;
                }
            }
            else if (PlayerDown)
            {
                m_paddle2.Y += PADDLE_STRIDE;
                if (m_paddle2.Y > MAX_Y)
                {
                    m_paddle2.Y = MAX_Y;
                }
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // our game-specific drawing logic
            Render();
            
            base.Draw(gameTime);
        }
        // draw the score at the specified location
        public void DrawScore(float x, float y, int score)
        {
            // print only single digit for [1, 9] inclusive
            if(score < 10)
            {
                spriteBatch.Draw((Texture2D)m_textureNumbers,
                new Vector2(x, y),
                m_ScoreRect[score % 10],
                Color.BlueViolet);
            }
            
        }

        // actually draw our game objects
        public void Render()
        {
            // black background
            // graphics.GraphicsDevice.Clear(Color.Black);

            // start rendering our game graphics
            spriteBatch.Begin();

            spriteBatch.Draw(spaceBG, new Rectangle(0, 0, 640, 480), Color.White);

            // draw the score first, so the ball can
            // move over it without being obscured
            DrawScore((float)SCREEN_WIDTH * 0.25f,
                20, m_Score1);
            DrawScore((float)SCREEN_WIDTH * 0.65f,
                20, m_Score2);

            // render the game objects
            spriteBatch.Draw((Texture2D)m_ball.Visual,
                m_ball.Rect, Color.White);
            spriteBatch.Draw((Texture2D)m_paddle1.Visual,
                m_paddle1.Rect, Color.White);
            spriteBatch.Draw((Texture2D)m_paddle2.Visual,
                m_paddle2.Rect, Color.White);

            // we're done drawing
            spriteBatch.End();
        }

    }
}
