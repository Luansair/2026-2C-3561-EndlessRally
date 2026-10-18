using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.src.Road
{
    /// <summary>
    ///     Un punto del eje del camino: posicion, hacia donde avanza y que tipo de pieza es.
    ///     Lo devuelve RoadSpawner.TrySampleRoad.
    /// </summary>
    internal readonly struct RoadSample
    {
        public Vector3 Position { get; init; }
        public Vector3 Forward { get; init; }
        public RoadPieceType Type { get; init; }
    }
}
