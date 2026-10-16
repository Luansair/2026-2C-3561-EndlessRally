using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.src.Assets;
using TGC.MonoGame.TP.src.Collectibles;
using TGC.MonoGame.TP.src.Core;
using TGC.MonoGame.TP.src.Decorations;

namespace TGC.MonoGame.TP.src.Road
{
    // Configura y crea RoadSpawner
    internal class RoadSpawnerFactory
    {
        private readonly RoadAssets _roadAssets;
        private readonly DecorationAreaFactory _decorationsFactory;
        private readonly CollectibleFactory _collectibleFactory;
        private readonly Dictionary<RoadPieceType, RoadPiece> _roadDefs;

        public RoadSpawnerFactory(RoadAssets roadAssets, DecorationAreaFactory decorationsFactory, CollectibleFactory collectibleFactory)
        {
            _roadAssets = roadAssets;
            _decorationsFactory = decorationsFactory;
            _collectibleFactory = collectibleFactory;

            _roadDefs = new Dictionary<RoadPieceType, RoadPiece>
            {
                { RoadPieceType.STRAIGHT, new RoadPiece(_roadAssets.RoadStraight, (new Vector3(0, 0, 10f))*_roadAssets.RoadStraight.Scale, 0f) },
                { RoadPieceType.RAMP, new RoadPiece(_roadAssets.RoadRamp, (new Vector3(0, 0, 10f))*_roadAssets.RoadRamp.Scale, 0f) },

                // Curva derecha: offset real al centro del carril de salida (15, 0, 15)
                { RoadPieceType.CORNERLARGE, new RoadPiece(_roadAssets.RoadCornerLeft, (new Vector3(15f, 0, 15f))*_roadAssets.RoadCornerLeft.Scale, MathHelper.PiOver2) },

                // Curva izquierda: offset real (-15, 0, 15)
                { RoadPieceType.CORNERLARGELEFT, new RoadPiece(_roadAssets.RoadCornerRight, (new Vector3(-15f, 0, 15f))*_roadAssets.RoadCornerRight.Scale, -MathHelper.PiOver2) }
            };
        }

        public RoadSpawner Create(RoadSpawnerConfig spawnerConfig, World world)
        {
            return new RoadSpawner(
                _roadDefs, 
                spawnerConfig, 
                _decorationsFactory, 
                _collectibleFactory,
                world);
        }
    }
}
