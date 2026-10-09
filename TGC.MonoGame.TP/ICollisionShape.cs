using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP
{
    internal interface ICollisionShape
    {
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

        public static AABBShape FromModel(ModelInfo model) => new AABBShape(GizmoPrimitives.CreateAABBFrom(model.Model));

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
        public bool Intersects(ICollisionShape other, Matrix worldA, Matrix worldB) => false;
    }
}


