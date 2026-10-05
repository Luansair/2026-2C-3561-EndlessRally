using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TGC.MonoGame.TP
{
    public sealed class GameContentLoader
    {
        public const string ContentFolder3D = "Models/";
        public GameAssets Load(ContentManager content, GraphicsDevice graphics)
        {
            var decorations = LoadDecorations(content, graphics);
            var cars = LoadCars(content, graphics);
            var roads = LoadRoads(content, graphics);
            var collectibles = LoadCollectibles(content, graphics);
            return new GameAssets
            {
                Decorations = decorations,
                Cars = cars,
                Roads = roads,
                Collectibles = collectibles
            };
        }
        private CarAssets LoadCars(ContentManager content, GraphicsDevice graphics)
        {
            var lightCarModel = new ModelInfo(content.Load<Model>(ContentFolder3D + "raceCarWhiteV2"), 0.01f);
            var mediumCarModel = new ModelInfo(content.Load<Model>(ContentFolder3D + "Kenney_Cars/hatchback-sportsV2"), 0.01f);
            var heavyCarModel = new ModelInfo(content.Load<Model>(ContentFolder3D + "Kenney_Cars/suvV2"), 0.01f);
            return new CarAssets
            {
                LightCar = lightCarModel,
                MediumCar = mediumCarModel,
                HeavyCar = heavyCarModel
            };
        }
        
        private RoadAssets LoadRoads(ContentManager content, GraphicsDevice graphics)
        {
            var roadStraightModel = new ModelInfo(content.Load<Model>(ContentFolder3D + "Kenny_races/roadStraightV2"), 1f);
            var roadRampModel = new ModelInfo(content.Load<Model>(ContentFolder3D + "Kenny_races/roadRamp"), 1f);
            var roadCornerModel = new ModelInfo(content.Load<Model>(ContentFolder3D + "Kenny_races/roadCornerLargeV2"), 1f);
            var roadCornerLeftModel = new ModelInfo(content.Load<Model>(ContentFolder3D + "Kenny_races/roadCornerLargeLeft"), 1f);

            return new RoadAssets
            {
                RoadStraight = roadStraightModel,
                RoadRamp = roadRampModel,
                RoadCornerRight = roadCornerModel,
                RoadCornerLeft = roadCornerLeftModel
            };
        }

        private CollectibleAssets LoadCollectibles(ContentManager content, GraphicsDevice graphics)
        {
            var coinModel = new ModelInfo(content.Load<Model>(ContentFolder3D + "Coleccionables/gems_monogame/gem1"), 200f);
            var fuelModel = new ModelInfo(content.Load<Model>(ContentFolder3D + "Coleccionables/Fuel/gascylinder"), 0.13f);
            var wrenchModel = new ModelInfo(content.Load<Model>(ContentFolder3D + "Coleccionables/Wrench/Monkey-Wrench"), 0.13f);
            var obstacleModel = new ModelInfo(content.Load<Model>(ContentFolder3D + "Coleccionables/Sphere2"), 10f);
            return new CollectibleAssets
            {
                Coin = coinModel,
                Fuel = fuelModel,
                Wrench = wrenchModel,
                Obstacle = obstacleModel
            };
        }
        private DecorationAssets LoadDecorations(ContentManager content, GraphicsDevice graphics)
        {
            var treeModel0 = new ModelInfo(content.Load<Model>(ContentFolder3D + "Tree/Tree"), 2.8f);

            var rockModel0 = new ModelInfo(content.Load<Model>(ContentFolder3D + "Stones/Rock0"), 0.002f);
            var rockModel1 = new ModelInfo(content.Load<Model>(ContentFolder3D + "Stones/Rock1"), 0.003f);
            var rockModel2 = new ModelInfo(content.Load<Model>(ContentFolder3D + "Stones/Rock2"), 0.008f);
            var rockModel3 = new ModelInfo(content.Load<Model>(ContentFolder3D + "Stones/Rock3"), 0.008f);
            var rockModel4 = new ModelInfo(content.Load<Model>(ContentFolder3D + "Stones/Rock4"), 0.008f);
            var rockModel5 = new ModelInfo(content.Load<Model>(ContentFolder3D + "Stones/Rock5"), 0.008f);
            var rockModel6 = new ModelInfo(content.Load<Model>(ContentFolder3D + "Stones/Rock6"), 0.008f);
            var rockModel7 = new ModelInfo(content.Load<Model>(ContentFolder3D + "Stones/Rock7"), 0.008f);
            var rockModel8 = new ModelInfo(content.Load<Model>(ContentFolder3D + "Stones/Rock8"), 0.008f);
            var rockModel9 = new ModelInfo(content.Load<Model>(ContentFolder3D + "Stones/Rock9"), 0.008f);
            var rockModel10 = new ModelInfo(content.Load<Model>(ContentFolder3D + "Stones/Rock10"), 0.008f);
            var rockModels = new List<ModelInfo>
            {
                rockModel0,
                rockModel1,
                rockModel2,
                rockModel3,
                rockModel4,
                rockModel5,
                rockModel6,
                rockModel7,
                rockModel8,
                rockModel9,
                rockModel10
            };

            var houseModel0 = new ModelInfo(content.Load<Model>(ContentFolder3D + "Building_Big"), 0.02f);

            return new DecorationAssets
            {
                Tree = treeModel0,
                Rocks = rockModels,
                House = houseModel0
            };
        }
    }
}