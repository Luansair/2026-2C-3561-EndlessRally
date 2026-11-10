using Microsoft.Xna.Framework;
using TGC.MonoGame.TP.src.Assets;

namespace TGC.MonoGame.TP.src.Physics
{
    internal interface ICollisionShape
    {
        public BoundingBox LocalBox { get; }
        bool Intersects(ICollisionShape other, Matrix worldA, Matrix worldB);
    }
    // Probably goes in a separate file.

    internal class AABBShape : ICollisionShape
    {
        public BoundingBox LocalBox { get; }

        public AABBShape(BoundingBox localBox)
        {
            LocalBox = localBox;
        }

        public static AABBShape FromModel(ModelInfo model) => new AABBShape(model.LocalBounds);

        // thicknessRatio es el porcentaje respecto al tamaño de la copa, heightRatio porcentaje de la altura
        public static AABBShape FromTrunk(ModelInfo model, float thicknessRatio = 0.25f, float heightRatio = 0.45f)
        {
            var b = model.LocalBounds;
            float centerX = (b.Min.X + b.Max.X) * 0.5f;
            float centerZ = (b.Min.Z + b.Max.Z) * 0.5f;
            float halfWidthX = (b.Max.X - b.Min.X) * 0.5f * thicknessRatio;
            float halfWidthZ = (b.Max.Z - b.Min.Z) * 0.5f * thicknessRatio;
            float minY = b.Min.Y;
            float maxY = b.Min.Y + (b.Max.Y - b.Min.Y) * heightRatio;
            Vector3 trunkMin = new Vector3(centerX - halfWidthX, minY, centerZ - halfWidthZ);
            Vector3 trunkMax = new Vector3(centerX + halfWidthX, maxY, centerZ + halfWidthZ);
            
            return new AABBShape(new BoundingBox(trunkMin, trunkMax));
        }

        private BoundingBox TransformAABB(BoundingBox box, Matrix world)
        {
            var corners = box.GetCorners();
            for (int i = 0; i < corners.Length; i++)
                corners[i] = Vector3.Transform(corners[i], world);
            return BoundingBox.CreateFromPoints(corners);
        }

        public bool Intersects(ICollisionShape other, Matrix worldA, Matrix worldB)
        {
            var worldBoxA = TransformAABB(LocalBox, worldA);
            if (other is AABBShape aabb)
            {
                var worldBoxB = TransformAABB(aabb.LocalBox, worldB);
                return worldBoxA.Intersects(worldBoxB);

            }
            return false;  // Other types of collision shapes can be handled here in the future.
        }
    }

    internal class NullShape : ICollisionShape
    {
        public BoundingBox LocalBox { get; }
        public bool Intersects(ICollisionShape other, Matrix worldA, Matrix worldB) => false;
    }
}


