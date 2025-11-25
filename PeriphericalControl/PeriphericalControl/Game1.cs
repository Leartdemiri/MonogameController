using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace PeriphericalControl
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private Texture2D _pixelTex;

        private GamePadState _prevGamePadState;
        private KeyboardState _prevKeyboardState;
        private MouseState _prevMouseState;

        // Historique
        private readonly List<InputEvent> _history = new List<InputEvent>();
        private const int MaxHistoryEntries = 25;

        // Calibration par STICK (gauche / droite)
        private readonly List<StickConfig> _sticks = new List<StickConfig>();
        private int _selectedStickIndex = 0;

        // Drag slider
        private bool _isDraggingSlider = false;
        private int _dragStickIndex = -1;
        private bool _dragDeadzone = false; // true = deadzone, false = sensitivity

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();

            // Deux sticks seulement : gauche / droite
            _sticks.Add(new StickConfig("Stick gauche"));
            _sticks.Add(new StickConfig("Stick droit"));
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("Arial");

            _pixelTex = new Texture2D(GraphicsDevice, 1, 1);
            _pixelTex.SetData(new[] { Color.White });
        }

        protected override void Update(GameTime gameTime)
        {
            var keyboard = Keyboard.GetState();
            var mouse = Mouse.GetState();

            if (keyboard.IsKeyDown(Keys.Escape))
                Exit();

            var state = GamePad.GetState(PlayerIndex.One);

            if (state.IsConnected)
            {
                HandleCalibrationKeyboardInput(keyboard);
                HandleSlidersMouseInput(mouse);
                UpdateHistory(state, gameTime);
            }

            _prevGamePadState = state;
            _prevKeyboardState = keyboard;
            _prevMouseState = mouse;

            base.Update(gameTime);
        }

        #region Layout Panels

        private void GetPanels(out Rectangle buttonsPanel, out Rectangle sticksPanel,
                               out Rectangle calibPanel, out Rectangle historyPanel)
        {
            // Boutons
            buttonsPanel = new Rectangle(20, 60, 420, 280);

            // Sticks / triggers
            sticksPanel = new Rectangle(20, 360, 420, 280);

            // Calibration plus large
            calibPanel = new Rectangle(460, 60, 460, 580);

            // Historique un peu plus compact
            historyPanel = new Rectangle(940, 60, 320, 580);
        }

        #endregion

        #region Input, Calibration, History

        private bool IsKeyJustPressed(KeyboardState current, Keys key)
        {
            return current.IsKeyDown(key) && !_prevKeyboardState.IsKeyDown(key);
        }

        private void HandleCalibrationKeyboardInput(KeyboardState keyboard)
        {
            if (_sticks.Count == 0) return;

            // Selection du stick (0 = gauche, 1 = droit)
            if (IsKeyJustPressed(keyboard, Keys.Up))
                _selectedStickIndex = (_selectedStickIndex - 1 + _sticks.Count) % _sticks.Count;

            if (IsKeyJustPressed(keyboard, Keys.Down))
                _selectedStickIndex = (_selectedStickIndex + 1) % _sticks.Count;

            var stick = _sticks[_selectedStickIndex];

            // Inversion (les deux axes du stick)
            if (IsKeyJustPressed(keyboard, Keys.I))
                stick.Inverted = !stick.Inverted;

            // Effacer l'historique
            if (IsKeyJustPressed(keyboard, Keys.H))
                _history.Clear();
        }

        // Pour les sliders (par stick)
        private class StickSliderRects
        {
            public int StickIndex;
            public Rectangle DeadzoneRect;
            public Rectangle SensRect;
        }

        private List<StickSliderRects> GetStickSliderRects()
        {
            GetPanels(out _, out _, out Rectangle calibPanel, out _);

            var list = new List<StickSliderRects>();
            float lineHeight = 140f;
            int startY = calibPanel.Top + 70;
            int sliderHeight = 12;
            int totalSliderWidth = calibPanel.Width - 60;

            for (int i = 0; i < _sticks.Count; i++)
            {
                int rowY = (int)(startY + i * lineHeight);
                int leftX = calibPanel.Left + 30;
                int halfWidth = totalSliderWidth / 2;

                Rectangle dz = new Rectangle(leftX, rowY + 30, halfWidth - 10, sliderHeight);
                Rectangle sens = new Rectangle(leftX + halfWidth + 10, rowY + 30, halfWidth - 10, sliderHeight);

                list.Add(new StickSliderRects
                {
                    StickIndex = i,
                    DeadzoneRect = dz,
                    SensRect = sens
                });
            }

            return list;
        }

        private void HandleSlidersMouseInput(MouseState mouse)
        {
            var sliderRects = GetStickSliderRects();

            bool leftJustPressed = mouse.LeftButton == ButtonState.Pressed &&
                                   _prevMouseState.LeftButton == ButtonState.Released;
            bool leftReleased = mouse.LeftButton == ButtonState.Released &&
                                _prevMouseState.LeftButton == ButtonState.Pressed;

            Point mousePoint = new Point(mouse.X, mouse.Y);

            if (leftJustPressed)
            {
                foreach (var s in sliderRects)
                {
                    if (s.DeadzoneRect.Contains(mousePoint))
                    {
                        _isDraggingSlider = true;
                        _dragStickIndex = s.StickIndex;
                        _dragDeadzone = true;
                        UpdateSliderValueFromMouse(mouse.X, s.DeadzoneRect);
                        return;
                    }
                    if (s.SensRect.Contains(mousePoint))
                    {
                        _isDraggingSlider = true;
                        _dragStickIndex = s.StickIndex;
                        _dragDeadzone = false;
                        UpdateSliderValueFromMouse(mouse.X, s.SensRect);
                        return;
                    }
                }
            }

            if (_isDraggingSlider && mouse.LeftButton == ButtonState.Pressed)
            {
                var s = sliderRects.Find(sl => sl.StickIndex == _dragStickIndex);
                if (s != null)
                {
                    if (_dragDeadzone)
                        UpdateSliderValueFromMouse(mouse.X, s.DeadzoneRect);
                    else
                        UpdateSliderValueFromMouse(mouse.X, s.SensRect);
                }
            }

            if (leftReleased)
            {
                _isDraggingSlider = false;
                _dragStickIndex = -1;
            }
        }

        private void UpdateSliderValueFromMouse(int mouseX, Rectangle rect)
        {
            if (_dragStickIndex < 0 || _dragStickIndex >= _sticks.Count)
                return;

            float t = (mouseX - rect.Left) / (float)rect.Width;
            t = MathHelper.Clamp(t, 0f, 1f);

            var stick = _sticks[_dragStickIndex];

            if (_dragDeadzone)
            {
                stick.DeadZone = t * 0.9f; // 0 -> 0.9
            }
            else
            {
                float min = 0.1f;
                float max = 3.0f;
                stick.Sensitivity = min + t * (max - min);
            }
        }

        private void UpdateHistory(GamePadState state, GameTime gameTime)
        {
            var buttons = new Dictionary<string, ButtonState>()
            {
                { "A",     state.Buttons.A },
                { "B",     state.Buttons.B },
                { "X",     state.Buttons.X },
                { "Y",     state.Buttons.Y },
                { "Start", state.Buttons.Start },
                { "Back",  state.Buttons.Back },
                { "L1",    state.Buttons.LeftShoulder },
                { "R1",    state.Buttons.RightShoulder },
                { "L3",    state.Buttons.LeftStick },
                { "R3",    state.Buttons.RightStick },
                { "Up",    state.DPad.Up },
                { "Down",  state.DPad.Down },
                { "Left",  state.DPad.Left },
                { "Right", state.DPad.Right }
            };

            var prevButtons = new Dictionary<string, ButtonState>()
            {
                { "A",     _prevGamePadState.Buttons.A },
                { "B",     _prevGamePadState.Buttons.B },
                { "X",     _prevGamePadState.Buttons.X },
                { "Y",     _prevGamePadState.Buttons.Y },
                { "Start", _prevGamePadState.Buttons.Start },
                { "Back",  _prevGamePadState.Buttons.Back },
                { "L1",    _prevGamePadState.Buttons.LeftShoulder },
                { "R1",    _prevGamePadState.Buttons.RightShoulder },
                { "L3",    _prevGamePadState.Buttons.LeftStick },
                { "R3",    _prevGamePadState.Buttons.RightStick },
                { "Up",    _prevGamePadState.DPad.Up },
                { "Down",  _prevGamePadState.DPad.Down },
                { "Left",  _prevGamePadState.DPad.Left },
                { "Right", _prevGamePadState.DPad.Right }
            };

            foreach (var kvp in buttons)
            {
                if (kvp.Value != prevButtons[kvp.Key])
                {
                    string stateText = kvp.Value == ButtonState.Pressed ? "PRESS" : "RELEASE";
                    AddHistoryEvent($"{kvp.Key} : {stateText}", gameTime.TotalGameTime);
                }
            }

            LogAxisChange("Left Stick X", _prevGamePadState.ThumbSticks.Left.X, state.ThumbSticks.Left.X, gameTime);
            LogAxisChange("Left Stick Y", _prevGamePadState.ThumbSticks.Left.Y, state.ThumbSticks.Left.Y, gameTime);
            LogAxisChange("Right Stick X", _prevGamePadState.ThumbSticks.Right.X, state.ThumbSticks.Right.X, gameTime);
            LogAxisChange("Right Stick Y", _prevGamePadState.ThumbSticks.Right.Y, state.ThumbSticks.Right.Y, gameTime);
            LogAxisChange("LT", _prevGamePadState.Triggers.Left, state.Triggers.Left, gameTime);
            LogAxisChange("RT", _prevGamePadState.Triggers.Right, state.Triggers.Right, gameTime);
        }

        private void LogAxisChange(string name, float previous, float current, GameTime gameTime)
        {
            const float threshold = 0.15f;
            if (Math.Abs(current - previous) >= threshold)
            {
                AddHistoryEvent($"{name} : {current:0.00}", gameTime.TotalGameTime);
            }
        }

        private void AddHistoryEvent(string text, TimeSpan time)
        {
            _history.Insert(0, new InputEvent(time, text));
            if (_history.Count > MaxHistoryEntries)
                _history.RemoveAt(_history.Count - 1);
        }

        #endregion

        #region Primitives drawing

        private void DrawLine(Vector2 start, Vector2 end, Color color, int thickness = 2)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            _spriteBatch.Draw(_pixelTex, start, null, color,
                angle, Vector2.Zero, new Vector2(edge.Length(), thickness), SpriteEffects.None, 0);
        }

        private void DrawCircle(Vector2 position, float radius, Color color, int thickness = 2)
        {
            int segments = 40;
            Vector2 prev = position + new Vector2(radius, 0);
            for (int i = 1; i <= segments; i++)
            {
                float theta = MathHelper.TwoPi * i / segments;
                Vector2 next = position + new Vector2(
                    radius * (float)Math.Cos(theta),
                    radius * (float)Math.Sin(theta));
                DrawLine(prev, next, color, thickness);
                prev = next;
            }
        }

        private void DrawFilledRect(Rectangle rect, Color color)
        {
            _spriteBatch.Draw(_pixelTex, rect, color);
        }

        private void DrawPanel(Rectangle rect, string title)
        {
            DrawFilledRect(rect, new Color(20, 20, 20, 220));

            DrawLine(new Vector2(rect.Left, rect.Top), new Vector2(rect.Right, rect.Top), Color.Gray);
            DrawLine(new Vector2(rect.Right, rect.Top), new Vector2(rect.Right, rect.Bottom), Color.Gray);
            DrawLine(new Vector2(rect.Right, rect.Bottom), new Vector2(rect.Left, rect.Bottom), Color.Gray);
            DrawLine(new Vector2(rect.Left, rect.Bottom), new Vector2(rect.Left, rect.Top), Color.Gray);

            if (!string.IsNullOrEmpty(title))
                _spriteBatch.DrawString(_font, title, new Vector2(rect.Left + 8, rect.Top + 4), Color.LightSkyBlue);
        }

        #endregion

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(10, 10, 20));
            _spriteBatch.Begin();

            var state = GamePad.GetState(PlayerIndex.One);

            if (!state.IsConnected)
            {
                _spriteBatch.DrawString(_font, "Controller not connected", new Vector2(20, 20), Color.Red);
                _spriteBatch.End();
                base.Draw(gameTime);
                return;
            }

            GetPanels(out Rectangle buttonsPanel, out Rectangle sticksPanel,
                      out Rectangle calibPanel, out Rectangle historyPanel);

            DrawPanel(buttonsPanel, "Buttons");
            DrawPanel(sticksPanel, "Sticks / Triggers");
            DrawPanel(calibPanel, "Calibration");
            DrawPanel(historyPanel, "History");

            _spriteBatch.DrawString(_font, "Gamepad connected", new Vector2(20, 20), Color.LimeGreen);

            DrawButtonsState(state, buttonsPanel);
            DrawSticksAndTriggers(state, sticksPanel);
            DrawCalibrationPanel(state, calibPanel);
            DrawHistoryPanel(historyPanel);

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawButtonsState(GamePadState state, Rectangle panel)
        {
            int columns = 3;
            float marginX = 50f;
            float marginY = 40f;
            float spacingX = (panel.Width - 2 * marginX) / (columns - 1);
            float spacingY = 50f;

            Vector2 startPos = new Vector2(panel.Left + marginX, panel.Top + marginY);

            var buttons = new Dictionary<string, ButtonState>()
            {
                { "A",     state.Buttons.A },
                { "B",     state.Buttons.B },
                { "X",     state.Buttons.X },
                { "Y",     state.Buttons.Y },
                { "Start", state.Buttons.Start },
                { "Back",  state.Buttons.Back },
                { "L1",    state.Buttons.LeftShoulder },
                { "R1",    state.Buttons.RightShoulder },
                { "L3",    state.Buttons.LeftStick },
                { "R3",    state.Buttons.RightStick },
                { "Up",    state.DPad.Up },
                { "Down",  state.DPad.Down },
                { "Left",  state.DPad.Left },
                { "Right", state.DPad.Right }
            };

            int i = 0;
            foreach (var b in buttons)
            {
                int col = i % columns;
                int row = i / columns;

                Vector2 pos = startPos + new Vector2(col * spacingX, row * spacingY);
                Color color = b.Value == ButtonState.Pressed ? Color.OrangeRed : Color.DimGray;

                float radius = 18f;
                DrawCircle(pos, radius, color, 3);

                Vector2 textSize = _font.MeasureString(b.Key);
                Vector2 textPos = pos - textSize / 2f;
                _spriteBatch.DrawString(_font, b.Key, textPos, Color.White);

                i++;
            }
        }

        private void DrawSticksAndTriggers(GamePadState state, Rectangle panel)
        {
            Vector2 leftStickCenter = new Vector2(panel.Left + 110, panel.Top + 120);
            Vector2 rightStickCenter = new Vector2(panel.Left + 310, panel.Top + 120);

            float rawLX = state.ThumbSticks.Left.X;
            float rawLY = state.ThumbSticks.Left.Y;
            float rawRX = state.ThumbSticks.Right.X;
            float rawRY = state.ThumbSticks.Right.Y;

            var cfgLeft = _sticks[0];
            var cfgRight = _sticks[1];

            float adjLX = cfgLeft.Apply(rawLX);
            float adjLY = cfgLeft.Apply(rawLY);
            float adjRX = cfgRight.Apply(rawRX);
            float adjRY = cfgRight.Apply(rawRY);

            // Stick gauche
            DrawCircle(leftStickCenter, 40, Color.White, 2);
            Vector2 leftStickPos = leftStickCenter + new Vector2(adjLX * 30, -adjLY * 30);
            DrawFilledRect(new Rectangle((int)leftStickPos.X - 5, (int)leftStickPos.Y - 5, 10, 10), Color.LimeGreen);
            _spriteBatch.DrawString(_font,
                $"L: ({adjLX:0.00}; {adjLY:0.00})",
                new Vector2(panel.Left + 20, panel.Top + 200), Color.LightGray);

            // Stick droit
            DrawCircle(rightStickCenter, 40, Color.White, 2);
            Vector2 rightStickPos = rightStickCenter + new Vector2(adjRX * 30, -adjRY * 30);
            DrawFilledRect(new Rectangle((int)rightStickPos.X - 5, (int)rightStickPos.Y - 5, 10, 10), Color.LimeGreen);
            _spriteBatch.DrawString(_font,
                $"R: ({adjRX:0.00}; {adjRY:0.00})",
                new Vector2(panel.Left + 220, panel.Top + 200), Color.LightGray);

            // Triggers
            float lt = state.Triggers.Left;
            float rt = state.Triggers.Right;

            int barWidth = panel.Width - 60;
            int barHeight = 20;
            int barX = panel.Left + 30;
            int barY1 = panel.Top + 240;
            int barY2 = panel.Top + 280;

            DrawFilledRect(new Rectangle(barX, barY1, barWidth, barHeight), Color.DimGray);
            DrawFilledRect(new Rectangle(barX, barY2, barWidth, barHeight), Color.DimGray);

            DrawFilledRect(new Rectangle(barX, barY1, (int)(barWidth * lt), barHeight), Color.Yellow);
            DrawFilledRect(new Rectangle(barX, barY2, (int)(barWidth * rt), barHeight), Color.Yellow);

            _spriteBatch.DrawString(_font, $"LT : {lt:0.00}", new Vector2(barX, barY1 - 24), Color.White);
            _spriteBatch.DrawString(_font, $"RT : {rt:0.00}", new Vector2(barX, barY2 - 24), Color.White);
        }

        private void DrawCalibrationPanel(GamePadState state, Rectangle panel)
        {
            float lineHeight = 140f;
            Vector2 pos = new Vector2(panel.Left + 20, panel.Top + 40);

            _spriteBatch.DrawString(_font,
                "I: invert  H: clear history  Up/Down: select stick",
                new Vector2(panel.Left + 12, panel.Bottom - 28),
                Color.LightGray);

            var sliderRects = GetStickSliderRects();

            for (int i = 0; i < _sticks.Count; i++)
            {
                bool selected = (i == _selectedStickIndex);
                var stick = _sticks[i];
                var rects = sliderRects[i];

                float rawX, rawY;
                if (i == 0)
                {
                    rawX = state.ThumbSticks.Left.X;
                    rawY = state.ThumbSticks.Left.Y;
                }
                else
                {
                    rawX = state.ThumbSticks.Right.X;
                    rawY = state.ThumbSticks.Right.Y;
                }

                float adjX = stick.Apply(rawX);
                float adjY = stick.Apply(rawY);

                string label = $"{stick.Name}  DZ={stick.DeadZone:0.00}  S={stick.Sensitivity:0.00}  Inv={(stick.Inverted ? "Y" : "N")}";
                string line2 = $"Raw=({rawX:0.00},{rawY:0.00})  Adj=({adjX:0.00},{adjY:0.00})";

                Color color = selected ? Color.LightGreen : Color.White;

                if (selected)
                {
                    DrawFilledRect(new Rectangle(panel.Left + 10, (int)(pos.Y - 4),
                        panel.Width - 20, 50), new Color(60, 60, 60, 160));
                }

                _spriteBatch.DrawString(_font, label, pos, color);
                _spriteBatch.DrawString(_font, line2, pos + new Vector2(0, 20), Color.LightGray);

                // Sliders
                DrawSlider(rects.DeadzoneRect, stick.DeadZone / 0.9f, "DZ");
                float tSens = (stick.Sensitivity - 0.1f) / (3.0f - 0.1f);
                DrawSlider(rects.SensRect, tSens, "S");

                pos.Y += lineHeight;
                if (pos.Y > panel.Bottom - 60)
                    break;
            }
        }

        private void DrawSlider(Rectangle rect, float t, string label)
        {
            t = MathHelper.Clamp(t, 0f, 1f);

            DrawFilledRect(rect, new Color(50, 50, 50));
            DrawFilledRect(new Rectangle(rect.Left, rect.Top, (int)(rect.Width * t), rect.Height), Color.SteelBlue);

            int knobX = rect.Left + (int)(rect.Width * t);
            Rectangle knob = new Rectangle(knobX - 5, rect.Top - 2, 10, rect.Height + 4);
            DrawFilledRect(knob, Color.White);

            _spriteBatch.DrawString(_font, label, new Vector2(rect.Left, rect.Top - 20), Color.LightGray);
        }

        private void DrawHistoryPanel(Rectangle panel)
        {
            Vector2 pos = new Vector2(panel.Left + 10, panel.Top + 40);
            float lineHeight = 24f;

            foreach (var e in _history)
            {
                string line = $"[{e.Time.TotalSeconds,6:0.0}s] {e.Text}";
                _spriteBatch.DrawString(_font, line, pos, Color.White);
                pos.Y += lineHeight;
                if (pos.Y > panel.Bottom - 20) break;
            }
        }
    }

}
