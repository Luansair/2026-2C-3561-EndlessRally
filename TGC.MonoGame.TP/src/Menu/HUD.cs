using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TGC.MonoGame.TP.src.Menu
{
    public class HUD
    {
        private readonly SpriteFont _font;
        private readonly Texture2D _pixel;

        public HUD(SpriteFont font, Texture2D whitePixel)
        {
            _font = font;
            _pixel = whitePixel;
        }

        public void Draw(SpriteBatch spriteBatch, int score, float health, float maxHealth, float fuel, float maxFuel)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            //Score
            string scoreText = $"SCORE: {score}";
            spriteBatch.DrawString(_font, scoreText, new Vector2(30, 25), Color.Gold);

            //barra de "Salud" 
            Vector2 healthBarPos = new Vector2(30, 65);
            Vector2 barSize = new Vector2(220, 22);
            DrawBar(spriteBatch, healthBarPos, barSize, health, maxHealth, Color.Crimson, "HP");

            //Combustible (anaranjado)
            Vector2 fuelBarPos = new Vector2(30, 100);
            DrawBar(spriteBatch, fuelBarPos, barSize, fuel, maxFuel, new Color(240, 176, 24), "FUEL");

            spriteBatch.End();
        }

        private void DrawBar(SpriteBatch sb, Vector2 pos, Vector2 size, float current, float max, Color fillColor, string label)
        {
            int border = 2;
            float percent = Math.Clamp(max > 0 ? current / max : 0f, 0f, 1f);

            // Fondo negro / contenedor
            sb.Draw(_pixel, new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y), new Color(20, 20, 20, 200));

            // Relleno de la barra según porcentaje
            int fillWidth = (int)((size.X - (border * 2)) * percent);
            if (fillWidth > 0)
            {
                Rectangle fillRect = new Rectangle(
                    (int)pos.X + border,
                    (int)pos.Y + border,
                    fillWidth,
                    (int)size.Y - (border * 2)
                );
                sb.Draw(_pixel, fillRect, fillColor);
            }

            // Texto descriptivo (ej: HP: 80 / 100)
            string text = $"{label}: {(int)current} / {(int)max}";
            Vector2 textPos = new Vector2(pos.X + 8, pos.Y + 2);
            sb.DrawString(_font, text, textPos, Color.White);
        }
    }
}