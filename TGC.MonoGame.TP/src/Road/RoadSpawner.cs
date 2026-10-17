using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TGC.MonoGame.TP.src.Collectibles;
using TGC.MonoGame.TP.src.Core;
using TGC.MonoGame.TP.src.Decorations;
using TGC.MonoGame.TP.src.Rendering;

namespace TGC.MonoGame.TP.src.Road
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

        // Zonas de superficie (asfalto / tierra / nieve)
        private const int TransitionChunks = 6;
        private const int MinZoneChunks = 15;
        private const int MaxZoneChunks = 30;
        private SurfaceType _currentSurface = SurfaceType.Asphalt;
        private SurfaceType _nextSurface = SurfaceType.Asphalt;
        private int _chunksLeftInZone = 20;   // la primera zona (asfalto) dura 20 chunks
        private int _transitionLeft = 0;
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

            while (Vector3.Distance(nextPos, spawnerConfig.StartPosition) < spawnDistance && _chunksGenerados < 500)
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

            // Despawn, borrar si la pieza más vieja quedó LEJOS del auto
            while (colaSegmentos.Count > 0)
            {
                var oldestChunk = colaSegmentos.Peek();
                
                // Si la distancia entre el auto y el chunk más viejo supera despawnDistance, recién ahí se elimina
                if (Vector3.Distance(oldestChunk.Position, carPosition) > despawnDistance)
                {
                    var chunk = colaSegmentos.Dequeue();
                    chunk.Road.Active = false;
                }
                else
                {
                    break; // Si la pieza más vieja aún está cerca del auto, no borramos nada
                }
            }
        }

        public void Draw(Effect effect, Matrix view, Matrix projection)
        {
            foreach (var segment in colaSegmentos)
            {
                segment.Draw(effect, view, projection);
            }
        }

        private SurfaceBlend NextSurfaceBlend()
        {
            if (_transitionLeft == 0)
            {
                if (_chunksLeftInZone-- > 0)
                    return new SurfaceBlend(_currentSurface, _currentSurface, 0f, 0f);

                // Arranca una transicion hacia una superficie distinta a la actual
                do { _nextSurface = (SurfaceType)random.Next(3); }
                while (_nextSurface == _currentSurface);

                _transitionLeft = TransitionChunks;
            }

            float t0 = 1f - (float)_transitionLeft / TransitionChunks;
            _transitionLeft--;
            float t1 = 1f - (float)_transitionLeft / TransitionChunks;

            var blend = new SurfaceBlend(_currentSurface, _nextSurface, t0, t1);

            if (_transitionLeft == 0)
            {
                _currentSurface = _nextSurface;
                _chunksLeftInZone = random.Next(MinZoneChunks, MaxZoneChunks + 1);
            }

            return blend;
        }

        // Tramo mas cercano a una posicion (para saber sobre que superficie esta el auto)
        public RoadChunk GetChunkNear(Vector3 pos)
        {
            RoadChunk best = null;
            float bestDist = float.MaxValue;

            foreach (var chunk in colaSegmentos)
            {
                float d = Vector3.DistanceSquared(chunk.Position, pos);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = chunk;
                }
            }

            return best;
        }

        public RoadPieceType? GetRoadPieceAt(Vector3 position)
        {
            RoadChunk chunk = FindRoadChunkAt(position, out _);
            return chunk?.Type;
        }

        public Func<float, Vector3> CreateTrafficPath(Vector3 position)
        {
            RoadChunk chunk = FindRoadChunkAt(position, out Vector3 localPosition);
            if (chunk == null)
                return null;

            RoadPiece piece = defs[chunk.Type];
            Func<float, Vector3> localPath = CreatePath(chunk.Type);
            float distanceOnPiece = GetDistanceAlongPiece(piece, localPosition);
            float currentParameter = -position.Z;
            Matrix pieceWorld = Matrix.CreateRotationY(chunk.Road.Transform.RotationY)
                              * Matrix.CreateTranslation(chunk.Road.Transform.Position);

            return parameter =>
            {
                float distance = distanceOnPiece + parameter - currentParameter;
                return Vector3.Transform(localPath(distance), pieceWorld);
            };
        }

        private RoadChunk FindRoadChunkAt(Vector3 position, out Vector3 localPosition)
        {
            RoadChunk bestChunk = null;
            localPosition = Vector3.Zero;
            float bestDistanceSquared = float.MaxValue;

            foreach (var chunk in colaSegmentos)
            {
                Matrix inversePieceWorld =
                    Matrix.CreateTranslation(-chunk.Road.Transform.Position) *
                    Matrix.CreateRotationY(-chunk.Road.Transform.RotationY);
                Vector3 candidatePosition = Vector3.Transform(position, inversePieceWorld);
                RoadPiece piece = defs[chunk.Type];
                float distance = MathHelper.Clamp(
                    GetDistanceAlongPiece(piece, candidatePosition),
                    0f,
                    GetPathLength(piece));
                Vector3 centerline = CreatePath(chunk.Type)(distance);
                float dx = candidatePosition.X - centerline.X;
                float dz = candidatePosition.Z - centerline.Z;
                float distanceSquared = dx * dx + dz * dz;

                if (distanceSquared < bestDistanceSquared)
                {
                    bestChunk = chunk;
                    localPosition = candidatePosition;
                    bestDistanceSquared = distanceSquared;
                }
            }

            const float roadHalfWidth = 5f;
            return bestDistanceSquared <= roadHalfWidth * roadHalfWidth ? bestChunk : null;
        }

        public Func<float, Vector3> CreatePath(RoadPieceType tipo)
        {   
            var piece = defs[tipo];
            return tipo switch
            {
                RoadPieceType.STRAIGHT => distance => StraightPath(piece, distance),
                RoadPieceType.RAMP => distance => StraightPath(piece, distance),
                RoadPieceType.CORNERLARGE => distance => CornerPath(piece, distance),
                RoadPieceType.CORNERLARGELEFT => distance => CornerPath(piece, distance),
                _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null)
            };
        }

        private static float GetDistanceAlongPiece(RoadPiece piece, Vector3 position)
        {
            if (piece.Type == RoadPieceType.CORNERLARGE || piece.Type == RoadPieceType.CORNERLARGELEFT)
            {
                float horizontalX = MathF.Abs(piece.offsetLocal.X);
                float horizontalZ = MathF.Abs(piece.offsetLocal.Z);
                float radius = (horizontalX + horizontalZ) / 2f;
                float direction = MathF.Sign(piece.offsetLocal.X);
                if (horizontalX <= 0f || horizontalZ <= 0f || radius <= 0f)
                    return 0f;

                float cosine = 1f - direction * position.X / horizontalX;
                float sine = position.Z / piece.offsetLocal.Z;
                float angle = MathHelper.Clamp(MathF.Atan2(sine, cosine), 0f, MathHelper.PiOver2);
                return angle * radius;
            }

            Vector3 forward = new Vector3(piece.offsetLocal.X, 0f, piece.offsetLocal.Z);
            float length = forward.Length();
            if (length <= 0f)
                return 0f;

            return Vector3.Dot(position, forward / length);
        }

        private static float GetPathLength(RoadPiece piece)
        {
            if (piece.Type == RoadPieceType.CORNERLARGE || piece.Type == RoadPieceType.CORNERLARGELEFT)
            {
                float radius = (MathF.Abs(piece.offsetLocal.X) + MathF.Abs(piece.offsetLocal.Z)) / 2f;
                return radius * MathHelper.PiOver2;
            }

            return new Vector3(piece.offsetLocal.X, 0f, piece.offsetLocal.Z).Length();
        }

        private static Vector3 StraightPath(RoadPiece piece, float distance)
        {
            float length = new Vector3(piece.offsetLocal.X, 0f, piece.offsetLocal.Z).Length();
            if (length <= 0f)
                throw new System.InvalidOperationException("A road piece must have a non-zero offset.");

            return new Vector3(piece.offsetLocal.X, 0f, piece.offsetLocal.Z) / length * distance;
        }

        private static Vector3 CornerPath(RoadPiece piece, float distance)
        {
            float direction = MathF.Sign(piece.offsetLocal.X);
            if (direction == 0f)
                direction = 1f;

            float radius = (MathF.Abs(piece.offsetLocal.X) + MathF.Abs(piece.offsetLocal.Z)) / 2f;
            if (radius <= 0f)
                throw new System.InvalidOperationException("A corner piece must have a non-zero horizontal offset.");

            float arcLength = radius * MathHelper.PiOver2;
            float arcDistance = MathHelper.Clamp(distance, 0f, arcLength);
            float angle = arcDistance / radius;
            var pointOnArc = new Vector3(
                direction * MathF.Abs(piece.offsetLocal.X) * (1f - MathF.Cos(angle)),
                piece.offsetLocal.Y * arcDistance / arcLength,
                piece.offsetLocal.Z * MathF.Sin(angle));

            if (distance < 0f)
            {
                var entryDirection = new Vector3(
                    0f,
                    piece.offsetLocal.Y / arcLength,
                    piece.offsetLocal.Z / radius);
                return entryDirection * distance;
            }

            if (distance > arcLength)
            {
                var exitDirection = new Vector3(
                    direction * MathF.Abs(piece.offsetLocal.X) / radius,
                    piece.offsetLocal.Y / arcLength,
                    0f);
                return pointOnArc + exitDirection * (distance - arcLength);
            }

            return pointOnArc;
        }
        private void SpawnNext()
        {
            RoadPieceType typeNow = nextType();
            RoadPiece def = defs[typeNow];

            // Matriz de mundo de la pieza actual
            Matrix world = Matrix.CreateRotationY(nextRot) * Matrix.CreateTranslation(nextPos);

            var roadEntity = new Entity
            {
                Render = new RenderModel(def.ModelInfo) { FallbackColor = new Vector3(0.20f, 0.20f, 0.22f), SurfaceBlendPartIndex = 0 },
                Surface = SurfaceMaterial.Asphalt,
                Blend = NextSurfaceBlend(),
                ChunkLength = def.offsetLocal.Z,
                IsMovable = false
            };

            var decorationArea = _decorationFactory.CreateFor(def, _chunksGenerados);
            var decorations = decorationArea.Generate();

            var chunk = new RoadChunk(roadEntity, decorations, roadEntity.Surface, typeNow);
            colaSegmentos.Enqueue(chunk);
            _world.Add(roadEntity);
            roadEntity.Transform.Position = nextPos;
            roadEntity.Transform.RotationY = nextRot;
            roadEntity.Transform.Scale = def.ModelInfo.Scale;
            
            foreach (var d in decorations)
            {
                d.Transform.Position = Vector3.Transform(d.Transform.Position, world);
                d.Transform.RotationY += nextRot;
                _world.Add(d);
            }

            TrySpawnCollectibleOnSegment(typeNow, world);

            // Desplazamos el punto de spawn hacia el final de la pieza actual
            Vector3 offset = Vector3.Transform(def.offsetLocal, Matrix.CreateRotationY(nextRot));
            nextPos += offset;

            // Acumulamos el ángulo para las piezas que vienen después
            nextRot += def.rotacionY;
            nextRot = MathHelper.WrapAngle(nextRot);
            
            _chunksGenerados++;

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

                _world.Add(_collectibleFactory.CreateRandom(worldPos, random));
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
