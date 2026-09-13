using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP
{
    public enum RoadPieceType
    {
        STRAIGHT,
        RAMP,
        CURVEDSPLIT,
        CURVEDSPLITLEFT
    }

    public struct RoadPiece
    {
        public Model model;
        public Vector3 offsetLocal;
        public float rotacionY;

        public RoadPiece(Model model, Vector3 offsetLocal, float rotacionY)
        {
            this.model = model;
            this.offsetLocal = offsetLocal;
            this.rotacionY = rotacionY;
        }
    }

    public class RoadSegment
    {
        public Model Model;
        public Matrix World;

        public RoadSegment(Model model, Matrix world)
        {
            Model = model;
            World = world;
        }

        public void Draw(Effect effect, Matrix view, Matrix projection)
        {
            var boneTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            foreach (var mesh in Model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = effect;
                }

                effect.Parameters["World"].SetValue(boneTransforms[mesh.ParentBone.Index] * World);
                effect.Parameters["View"].SetValue(view);
                effect.Parameters["Projection"].SetValue(projection);

                mesh.Draw();
            }
        }
    }
}
