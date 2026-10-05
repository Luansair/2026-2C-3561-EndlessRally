using System.Collections.Generic;

namespace TGC.MonoGame.TP
{
    // Represents a collection of game assets, such as models, textures, and sounds.
    public sealed class GameAssets
    {
        public DecorationAssets Decorations { get; init; }
        public CarAssets Cars { get; init; }
        public RoadAssets Roads { get; init; }
        public CollectibleAssets Collectibles { get; init; }
    }

    public sealed class DecorationAssets
    {
        public ModelInfo Tree { get; init; }
        public IReadOnlyList<ModelInfo> Rocks { get; init; }
        public ModelInfo House { get; init; }
    }

    public sealed class CarAssets
    {
        public ModelInfo LightCar { get; init; }
        public ModelInfo MediumCar { get; init; }
        public ModelInfo HeavyCar { get; init; }
    }

    public sealed class RoadAssets
    {
        public ModelInfo RoadStraight { get; init; }
        public ModelInfo RoadCornerRight { get; init; }
        public ModelInfo RoadCornerLeft { get; init; }
        public ModelInfo RoadRamp { get; init; }
    }

    public sealed class CollectibleAssets
    {
        public ModelInfo Coin { get; init; }
        public ModelInfo Fuel { get; init; }
        public ModelInfo Wrench { get; init; }
        public ModelInfo Obstacle { get; init; }
    }
}
