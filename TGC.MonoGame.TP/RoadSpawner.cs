using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime;
using System.Xml.Schema;

namespace TGC.MonoGame.TP
{
    internal class RoadSpawner
    {
        //largo de las piezas
        private const float TileLength = 10f;

        private readonly Dictionary<RoadPieceType, RoadPiece> defs;
        private readonly Queue<RoadChunk> colaSegmentos;
        private readonly Random random;
        // generador de decoraciones del camino
        private readonly DecorationAreaFactory _decorationFactory;
        private CollectibleFactory _collectibleFactory;
        //distancia a la que spawnea camino
        private readonly float spawnDistance;
        private readonly float despawnDistance;
        //cantidad maxima para despawn
        private readonly int maxRoads;

        //punto al que tiene que espanear el siguiente camino
        private Vector3 nextPos;
        //rotacion que recien tuvo
        private float nextRot;

        private RoadPieceType lastType;
        //racha para sacar los repetidos
        private int sameRoadRacha;
        private int _chunksGenerados;
        private int _segmentsSinceLastCollectible = 0;
        private World _world;

        
        public RoadSpawner(
            Dictionary<RoadPieceType, RoadPiece> defs, 
            RoadSpawnerConfig spawnerConfig, 
            DecorationAreaFactory decorationFactory,
            CollectibleFactory collectibleFactory,
            World world)
        {
            this.defs = defs;
            this.colaSegmentos = new Queue<RoadChunk>();
            this.random = new Random();
            this._decorationFactory = decorationFactory;
            this._collectibleFactory = collectibleFactory;
            this._world = world;
            this.despawnDistance = spawnerConfig.DespawnDistance;
            this.spawnDistance = spawnerConfig.SpawnDistance;
            this._chunksGenerados = 0;

            nextPos = spawnerConfig.StartPosition;
            nextRot = 0f;
            lastType = RoadPieceType.STRAIGHT;
            sameRoadRacha = 0;

            for (int i = 0; i < spawnerConfig.InitialSegmentCount; i++)
            {
                SpawnNext();
            }
        }
        public void Update(Vector3 carPosition)
        {
            // Generar hacia adelante si el auto se acerca al final
            while (Vector3.Distance(nextPos, carPosition) < spawnDistance)
            {
                SpawnNext();
            }

            // Despawn, SOLO borrar si la pieza más vieja quedó LEJOS del auto
            while (colaSegmentos.Count > 0)
            {
                var oldestChunk = colaSegmentos.Peek();
                
                // Si la distancia entre el auto y el chunk más viejo supera despawnDistance, recién ahí se elimina
                if (Vector3.Distance(oldestChunk.Position, carPosition) > despawnDistance)
                {
                    colaSegmentos.Dequeue();
                }
                else
                {
                    break; // Si la pieza más vieja aún está cerca del auto, no borramos nada
                }
                collectibles.RemoveAll(c => c.Collected || Vector3.Distance(c.pos, carPosition) > despawnDistance);
            }
        }

        public void Draw(Effect effect, Matrix view, Matrix projection)
        {
            foreach (var segment in colaSegmentos)
            {
                segment.Draw(effect, view, projection);
            }
        }

        private void SpawnNext()
        {
            RoadPieceType typeNow = nextType();
            RoadPiece def = defs[typeNow];

            // Matriz de mundo de la pieza actual
            Matrix world = Matrix.CreateRotationY(nextRot) * Matrix.CreateTranslation(nextPos);

            var roadEntity = new Entity
            {
                Render = new RenderModel(def.ModelInfo) { FallbackColor = new Vector3(0.20f, 0.20f, 0.22f) },
                Surface = SurfaceMaterial.Asphalt,
                IsMovable = false
            };

            var decorationArea = _decorationFactory.CreateFor(def, _chunksGenerados);
            var decorations = decorationArea.Generate();

            var chunk = new RoadChunk(roadEntity, decorations, roadEntity.Surface);

            _world.Add(roadEntity);
            foreach (var d in decorations) _world.Add(d);

            TrySpawnCollectibleOnSegment(typeNow, world);

            // Desplazamos el punto de spawn hacia el final de la pieza actual
            Vector3 offset = Vector3.Transform(def.offsetLocal, Matrix.CreateRotationY(nextRot));
            nextPos += offset;

            // Acumulamos el ángulo para las piezas que vienen después
            nextRot += def.rotacionY;
            nextRot = MathHelper.WrapAngle(nextRot);

            lastType = typeNow;
            
            // Si la pieza fue recta sumamos racha; si fue curva la reiniciamos
            if (typeNow == RoadPieceType.STRAIGHT)
                sameRoadRacha++;
            else
                sameRoadRacha = 0;
        }

        private void TrySpawnCollectibleOnSegment(RoadPieceType type, Matrix segmentWorld)
        {
            // Solo spawneamos en rectas para que queden siempre centrados en los carriles
            if (type != RoadPieceType.STRAIGHT) 
                return;

            _segmentsSinceLastCollectible++;

            // Spawneamos 1 cada 4 tramos
            if (_segmentsSinceLastCollectible >= 4)
            {
                _segmentsSinceLastCollectible = 0;

                // Elegir carril: Izquierda (-2.5), Centro (0), Derecha (2.5)
                float[] lanes = { -2.5f, 0f, 2.5f };
                float laneX = lanes[random.Next(lanes.Length)];

                // Altura de 1.0f para que quede a la altura de la carrocería del auto
                Vector3 localPos = new Vector3(laneX, 1.0f, 5.0f);
                Vector3 worldPos = Vector3.Transform(localPos, segmentWorld);

                collectibles.Add(_collectibleFactory.CreateRandom(worldPos, random));
            }
        }

        public RoadChunk GetCurrentSegment()
        {
            if (colaSegmentos.Count == 0) return null;
            return colaSegmentos.Peek();
        }
        private RoadPieceType nextType()
        {
            // Obligar a que haya al menos 3 rectas entre curvas
            if (sameRoadRacha < 3)
                return RoadPieceType.STRAIGHT;

            // 60% de probabilidad de seguir en línea recta
            if (random.NextDouble() < 0.6)
                return RoadPieceType.STRAIGHT;

            // Control para no dar giros en U ni volver hacia atrás:
            // Si el camino ya está inclinado a la derecha (> 45°), doblamos a la izquierda
            if (nextRot > MathHelper.ToRadians(45f))
                return RoadPieceType.CORNERLARGELEFT;

            // Si ya está inclinado a la izquierda (< -45°), doblamos a la derecha
            if (nextRot < -MathHelper.ToRadians(45f))
                return RoadPieceType.CORNERLARGE;

            // Si viene relativamente recto, 50% para cada lado
            return random.Next(2) == 0 ? RoadPieceType.CORNERLARGE : RoadPieceType.CORNERLARGELEFT;
        }
    }
}

public sealed class RoadSpawnerConfig
{
    public Vector3 StartPosition { get; init; }
    public float SpawnDistance { get; init; }
    public float DespawnDistance { get; init; }
    public int InitialSegmentCount { get; init; }

}
