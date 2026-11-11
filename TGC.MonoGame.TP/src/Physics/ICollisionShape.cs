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

        public static AABBShape FromCar(ModelInfo model, float widthRatio = 0.85f, float lengthRatio = 0.90f)
        {
            var b = model.LocalBounds;
            float centerX = (b.Min.X + b.Max.X) * 0.5f;
            float centerY = (b.Min.Y + b.Max.Y) * 0.5f;
            float centerZ = (b.Min.Z + b.Max.Z) * 0.5f;

            float hx = (b.Max.X - b.Min.X) * 0.5f * widthRatio;
            float hy = (b.Max.Y - b.Min.Y) * 0.5f;
            float hz = (b.Max.Z - b.Min.Z) * 0.5f * lengthRatio;

            return new AABBShape(new BoundingBox(
                new Vector3(centerX - hx, centerY - hy, centerZ - hz),
                new Vector3(centerX + hx, centerY + hy, centerZ + hz)
            ));
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
            // Si uno de los dos es un tronco o cilindro, medimos la distancia real en el plano XZ
            Vector3 posA = worldA.Translation;
            Vector3 posB = worldB.Translation;

            // Distancia plana (X y Z) entre el auto y el árbol
            float dx = posA.X - posB.X;
            float dz = posA.Z - posB.Z;
            float distSquared = dx * dx + dz * dz;

            // Radio aproximado del auto (~1.5m) + radio del tronco (~0.6m) = ~2.1m
            // Si la distancia es mayor a 2.5 metros, es imposible que se toquen
            float radioColision = 2.2f;
            if (distSquared > radioColision * radioColision)
                return false;

            // Si están cerca en el plano horizontal, verificamos altura Y con el AABB
            var worldBoxA = TransformAABB(LocalBox, worldA);
            if (other is AABBShape aabb)
            {
                var worldBoxB = TransformAABB(aabb.LocalBox, worldB);
                return worldBoxA.Intersects(worldBoxB);
            }
            return false;
        }
    }

    internal class NullShape : ICollisionShape
    {
        public BoundingBox LocalBox { get; }
        public bool Intersects(ICollisionShape other, Matrix worldA, Matrix worldB) => false;
    }
}


