using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP
{
    public enum RoadPieceType
    {
        STRAIGHT,
        RAMP,
        CORNERLARGE,
        CORNERLARGELEFT,
    }

    public struct RoadPiece
    {
        public ModelInfo ModelInfo { get; }
        public Vector3 offsetLocal { get; }
        public float rotacionY { get; }

        public RoadPiece(ModelInfo modelInfo, Vector3 offsetLocal, float rotacionY)
        {
            ModelInfo = modelInfo;
            this.offsetLocal = offsetLocal;
            this.rotacionY = rotacionY;
        }
    }

    public class RoadSegment
    {
        // Indice de la parte del modelo que es la calzada (el resto son cordones).
        // Si en tu FBX el asfalto no es la parte 0, cambialo aca.
        private const int AsphaltPartIndex = 0;

        public ModelInfo ModelInfo { get; }
        public Matrix World { get; }

        public RoadSegment(ModelInfo modelInfo, Matrix world)
        {
            ModelInfo = modelInfo;
            World = world;
        }

        public void Draw(Effect effect, Matrix view, Matrix projection)
        {
            var model = ModelInfo.Model;
            var boneTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            foreach (var mesh in model.Meshes)
            {
                Matrix meshWorld = boneTransforms[mesh.ParentBone.Index] * World;
                Matrix invTranspose = Matrix.Transpose(Matrix.Invert(meshWorld));

                for (int i = 0; i < mesh.MeshParts.Count; i++)
                {
                    var part = mesh.MeshParts[i];
                    part.Effect = effect;

                    effect.Parameters["World"]?.SetValue(meshWorld);
                    effect.Parameters["View"]?.SetValue(view);
                    effect.Parameters["Projection"]?.SetValue(projection);
                    effect.Parameters["InverseTransposeWorld"]?.SetValue(invTranspose);

                    Texture2D textureToUse = TGCGame.DefaultTexture;
                    Vector3 partColor;

                    if (ModelInfo.PartMaterials.TryGetValue(part, out var mat))
                    {
                        if (mat.Texture != null)
                        {
                            textureToUse = mat.Texture;
                            partColor = Vector3.One;
                        }
                        else if (mat.DiffuseColor != Vector3.Zero)
                        {
                            partColor = mat.DiffuseColor;
                        }
                        else
                        {
                            partColor = GetRoadFallbackColor(i);
                        }
                    }
                    else
                    {
                        partColor = GetRoadFallbackColor(i);
                    }
                    effect.Parameters["baseTexture"]?.SetValue(textureToUse);
                    effect.Parameters["DiffuseColor"]?.SetValue(partColor);

                    // Solo la calzada usa la mezcla de superficies del shader
                    effect.Parameters["UseSurfaceBlend"]?.SetValue(i == AsphaltPartIndex ? 1f : 0f);

                    // Dibujamos EXCLUSIVAMENTE esta parte geométrica
                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        effect.GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        effect.GraphicsDevice.Indices = part.IndexBuffer;
                        effect.GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            part.VertexOffset,
                            part.StartIndex,
                            part.PrimitiveCount
                        );
                    }
                }
            }
        }
        // Si el FBX no trae colores de material: Parte 0 = asfalto, Parte 1+ = cordones
        private static Vector3 GetRoadFallbackColor(int partIndex) => partIndex switch
        {
            0 => new Vector3(0.20f, 0.20f, 0.22f), // Asfalto gris oscuro
            _ => new Vector3(0.60f, 0.60f, 0.60f)  // Cordones gris
        };
    }
}