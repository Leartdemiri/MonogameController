using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MonogameController
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private Texture2D _circleTex;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("Arial"); // Assurez-vous d'avoir une SpriteFont

            // Créer une texture cercle simple
            _circleTex = new Texture2D(GraphicsDevice, 1, 1);
            _circleTex.SetData(new[] { Color.White });
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        // Dessine un cercle simple
        private void DrawCircle(Vector2 position, float radius, Color color)
        {
            int segments = 30;
            Vector2 prev = position + new Vector2(radius, 0);
            for (int i = 1; i <= segments; i++)
            {
                float theta = MathHelper.TwoPi * i / segments;
                Vector2 next = position + new Vector2(radius * (float)Math.Cos(theta), radius * (float)Math.Sin(theta));
                DrawLine(prev, next, color, 2);
                prev = next;
            }
        }

        // Dessine une ligne simple
        private void DrawLine(Vector2 start, Vector2 end, Color color, int thickness)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            _spriteBatch.Draw(_circleTex, start, null, color,
                angle, Vector2.Zero, new Vector2(edge.Length(), thickness), SpriteEffects.None, 0);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();

            var state = GamePad.GetState(PlayerIndex.One);

            if (!state.IsConnected)
            {
                _spriteBatch.DrawString(_font, "Manette non connectée", new Vector2(20, 20), Color.Red);
            }
            else
            {
                // Position de départ pour les boutons
                Vector2 startPos = new Vector2(100, 100);
                float spacing = 60;

                // Liste des boutons principaux
                var buttons = new Dictionary<string, ButtonState>()
                {
                    { "A", state.Buttons.B },
                    { "B", state.Buttons.A },
                    { "X", state.Buttons.Y },
                    { "Y", state.Buttons.X },
                    { "Start", state.Buttons.Start },
                    { "Select", state.Buttons.Back },
                    { "L1", state.Buttons.LeftShoulder },
                    { "R1", state.Buttons.RightShoulder },
                    { "L3", state.Buttons.LeftStick },
                    { "R3", state.Buttons.RightStick },
                    { "DUp", state.DPad.Up },
                    { "DDown", state.DPad.Down },
                    { "DLeft", state.DPad.Left },
                    { "DRight", state.DPad.Right }
                };

                int i = 0;
                foreach (var b in buttons)
                {
                    Vector2 pos = startPos + new Vector2((i % 4) * spacing, (i / 4) * spacing);
                    Color color = b.Value == ButtonState.Pressed ? Color.Red : Color.White;

                    DrawCircle(pos, 20, color);
                    _spriteBatch.DrawString(_font, b.Key, pos - new Vector2(10, 10), Color.White);
                    i++;
                }

                // Stick gauche
                Vector2 leftStickCenter = new Vector2(500, 150);
                DrawCircle(leftStickCenter, 30, Color.White);
                Vector2 leftStickPos = leftStickCenter + new Vector2(state.ThumbSticks.Left.X * 20, -state.ThumbSticks.Left.Y * 20);
                _spriteBatch.Draw(_circleTex, new Rectangle((int)leftStickPos.X - 5, (int)leftStickPos.Y - 5, 10, 10), Color.Green);

                // Stick droit
                Vector2 rightStickCenter = new Vector2(600, 150);
                DrawCircle(rightStickCenter, 30, Color.White);
                Vector2 rightStickPos = rightStickCenter + new Vector2(state.ThumbSticks.Right.X * 20, -state.ThumbSticks.Right.Y * 20);
                _spriteBatch.Draw(_circleTex, new Rectangle((int)rightStickPos.X - 5, (int)rightStickPos.Y - 5, 10, 10), Color.Green);

                // Triggers gauche
                Vector2 ltPos = new Vector2(100, 400);
                DrawCircle(ltPos, 20, Color.White);
                _spriteBatch.Draw(_circleTex, new Rectangle((int)ltPos.X - 10, (int)(ltPos.Y + 10 - state.Triggers.Left * 20), 20, (int)(state.Triggers.Left * 20)), Color.Yellow);

                // Triggers droite
                Vector2 rtPos = new Vector2(200, 400);
                DrawCircle(rtPos, 20, Color.White);
                _spriteBatch.Draw(_circleTex, new Rectangle((int)rtPos.X - 10, (int)(rtPos.Y + 10 - state.Triggers.Right * 20), 20, (int)(state.Triggers.Right * 20)), Color.Yellow);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
