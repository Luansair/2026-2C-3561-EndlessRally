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

            // Siempre usamos la textura blanca lisa para la pista
            effect.Parameters["baseTexture"]?.SetValue(TGCGame.DefaultTexture);

            foreach (var mesh in model.Meshes)
            {
                Matrix meshWorld = boneTransforms[mesh.ParentBone.Index] * World;
                Matrix invTranspose = Matrix.Transpose(Matrix.Invert(meshWorld));

                for (int i = 0; i < mesh.MeshParts.Count; i++)
                {
                    var part = mesh.MeshParts[i];
                    part.Effect = effect;

                    effect.Parameters["World"].SetValue(meshWorld);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);
                    effect.Parameters["InverseTransposeWorld"].SetValue(invTranspose);

                    // Asignamos colores según el material o índice de parte:
                    // Kenney suele usar la parte 0 para el asfalto y la 1 para cordones/líneas
                    Vector3 partColor;
                    if (ModelInfo.PartMaterials.TryGetValue(part, out var mat) && mat.DiffuseColor != Vector3.Zero)
                    {
                        partColor = mat.DiffuseColor;
                    }
                    else
                    {
                        // Fallback seguro si el FBX vino en (0,0,0):
                        // Si es la primera parte, asfalto oscuro; si no, cordón gris claro
                        partColor = (i == 0) 
                            ? new Vector3(0.22f, 0.22f, 0.24f)   // Asfalto gris oscuro
                            : new Vector3(0.75f, 0.75f, 0.75f);  // Cordón gris claro
                    }

                    effect.Parameters["DiffuseColor"].SetValue(partColor);

                    mesh.Draw();
                }
            }
        }
    }
}
