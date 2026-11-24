using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.src.Rendering
{
    internal static class LightManager
    {
        public static void ApplyDefaults(Effect effect)
        {
            if (effect == null) return;

            effect.Parameters["baseTexture"]?.SetValue(TGCGame.DefaultTexture);

            effect.Parameters["lightAmbientColor"]?.SetValue(new Vector3(0.8f, 0.8f, 0.8f));
            effect.Parameters["KAmbient"]?.SetValue(0.60f);

            effect.Parameters["lightDiffuseColor"]?.SetValue(new Vector3(1.0f, 0.98f, 0.92f));
            effect.Parameters["KDiffuse"]?.SetValue(0.70f);

            effect.Parameters["lightSpecularColor"]?.SetValue(new Vector3(1.0f, 1.0f, 1.0f));
            effect.Parameters["KSpecular"]?.SetValue(0.20f);
            effect.Parameters["shininess"]?.SetValue(18.0f);
        }
    }
}
