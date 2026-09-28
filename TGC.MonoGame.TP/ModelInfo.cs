using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     This structure stores the model and its scale for normalization, which means everytime the model is drawn, it should be scaled by this factor.
    /// </summary>
    public class ModelInfo
    {
        public Model Model { get; }
        public float Scale { get; }
        
        // Mantenemos la propiedad Texture para no romper Decoration.cs ni RoadSegment.cs
        public Texture2D Texture { get; }

        // Diccionario para modelos con múltiples materiales (autos y pistas de Kenney)
        public Dictionary<ModelMeshPart, (Texture2D Texture, Vector3 DiffuseColor)> PartMaterials { get; } 
            = new Dictionary<ModelMeshPart, (Texture2D, Vector3)>();

        public ModelInfo(Model model, float scale, Texture2D customTexture = null)
        {
            Model = model;
            Scale = scale;

            Texture2D firstDetectedTexture = customTexture;

            foreach (var mesh in model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    if (part.Effect is BasicEffect be)
                    {
                        var tex = customTexture ?? be.Texture;
                        if (firstDetectedTexture == null && tex != null)
                        {
                            firstDetectedTexture = tex;
                        }

                        PartMaterials[part] = (tex, be.DiffuseColor);
                    }
                }
            }

            Texture = firstDetectedTexture;
        }
    }
}
