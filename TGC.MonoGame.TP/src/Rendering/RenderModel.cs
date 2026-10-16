using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.src.Assets;

namespace TGC.MonoGame.TP.src.Rendering
{
    internal class RenderModel
    {
        public ModelInfo ModelInfo { get; }
        public Vector3 FallbackColor { get; set; } = Vector3.One;
        public Texture2D OverrideTexture { get; set; }

        // Indice de la parte del modelo que usa la mezcla de superficies (-1 = ninguna)
        public int SurfaceBlendPartIndex { get; set; } = -1;

        public RenderModel(ModelInfo modelInfo)
        {
            ModelInfo = modelInfo;
        }

        public void Draw(Effect effect, Matrix view, Matrix projection, Matrix world)
        {
            var model = ModelInfo.Model;
            var boneTransforms = ModelInfo.BoneTransforms;

            foreach (var mesh in model.Meshes)
            {
                var meshWorld = boneTransforms[mesh.ParentBone.Index] * world;
                var invTranspose = Matrix.Transpose(Matrix.Invert(meshWorld));

                for (int i = 0; i < mesh.MeshParts.Count; i++)
                {
                    var part = mesh.MeshParts[i];
                    part.Effect = effect;
                    effect.Parameters["World"]?.SetValue(meshWorld);
                    effect.Parameters["View"]?.SetValue(view);
                    effect.Parameters["Projection"]?.SetValue(projection);
                    effect.Parameters["InverseTransposeWorld"]?.SetValue(invTranspose);

                    Texture2D texture = OverrideTexture ?? TGCGame.DefaultTexture;
                    Vector3 color = FallbackColor;

                    if (ModelInfo.PartMaterials.TryGetValue(part, out var mat))
                    {
                        if (mat.Texture != null) { texture = mat.Texture; color = Vector3.One; }
                        else if (mat.DiffuseColor != Vector3.Zero) color = mat.DiffuseColor;
                    }

                    effect.Parameters["baseTexture"]?.SetValue(texture);
                    effect.Parameters["DiffuseColor"]?.SetValue(color);

                    if (SurfaceBlendPartIndex >= 0)
                        effect.Parameters["UseSurfaceBlend"]?.SetValue(i == SurfaceBlendPartIndex ? 1f : 0f);

                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        effect.GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        effect.GraphicsDevice.Indices = part.IndexBuffer;
                        effect.GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            part.VertexOffset,
                            part.StartIndex,
                            part.PrimitiveCount);
                    }
                }
            }
        }
    }
}
