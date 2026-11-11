using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.src.Physics
{
    /// <summary>
    ///     Helpers que usa OrientedBoundingBox (en los samples de TGC vienen en otra libreria).
    /// </summary>
    public static class BoundingVolumesExtensions
    {
        public static Vector3 GetCenter(BoundingBox box) => (box.Min + box.Max) * 0.5f;

        public static Vector3 GetExtents(BoundingBox box) => (box.Max - box.Min) * 0.5f;

        public static float GetVolume(BoundingBox box)
        {
            var size = box.Max - box.Min;
            return size.X * size.Y * size.Z;
        }
    }
}
