

namespace TGC.MonoGame.TP
{
    internal class SurfaceMaterial
    {
        public float Friction { get; init; } = 1.0f;
        public float Drag { get; init; } = 0f;
        public float Restitution { get; init; } = 0f;

        public static readonly SurfaceMaterial Asphalt = new SurfaceMaterial
        {
            Friction = 1f,
            Drag = 0.0f,
            Restitution = 0.0f
        };

        public static readonly SurfaceMaterial Grass = new SurfaceMaterial
        {
            Friction = 0.7f,
            Drag = 0.5f,
            Restitution = 0.0f
        };
    } 
}
