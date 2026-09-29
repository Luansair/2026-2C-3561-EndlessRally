using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

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
        public List<Collectible> Collectibles { get; } = new List<Collectible>();
        private readonly ModelInfo _coinModel;
        private readonly ModelInfo _fuelModel;
        private readonly ModelInfo _wrenchModel;
        private readonly ModelInfo _obstacleModel;
        private int _segmentsSinceLastCollectible = 0;

        // ---- Superficies (asfalto / tierra / nieve) ----
        // Cantidad de chunks que dura el fundido entre dos superficies
        private const int TransitionChunks = 6;
        private const int MinZoneChunks = 15;
        private const int MaxZoneChunks = 30;

        private SurfaceType _currentSurface = SurfaceType.Asphalt;
        private SurfaceType _nextSurface = SurfaceType.Asphalt;
        private int _chunksLeftInZone = 20;   // la primera zona (asfalto) dura 20 chunks
        private int _transitionLeft = 0;
        

        public RoadSpawner(
            Dictionary<RoadPieceType, RoadPiece> defs, 
            Vector3 startPos, 
            float spawnDistance, 
            float despawnDistance, 
            DecorationAreaFactory decorationFactory,
            ModelInfo coinModel,
            ModelInfo fuelModel,
            ModelInfo wrenchModel,
            ModelInfo obstacleModel)
        {
            this.defs = defs;
            this.colaSegmentos = new Queue<RoadChunk>();
            this.random = new Random();
            this._decorationFactory = decorationFactory;
            this.despawnDistance = despawnDistance;
            this.spawnDistance = spawnDistance;
            this._chunksGenerados = 0;

            // Guardamos los modelos
            _coinModel = coinModel;
            _fuelModel = fuelModel;
            _wrenchModel = wrenchModel;
            _obstacleModel = obstacleModel;

            nextPos = startPos;
            nextRot = 0f;
            lastType = RoadPieceType.STRAIGHT;
            sameRoadRacha = 0;

            for (int i = 0; i < 100; i++)
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
                Collectibles.RemoveAll(c => c.Collected || Vector3.Distance(c.pos, carPosition) > despawnDistance);
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

            var roadSegment = new RoadSegment(def.ModelInfo, world);
            var decorations = _decorationFactory.CreateFor(def, _chunksGenerados);
            SurfaceBlend surface = NextSurfaceBlend();
            var chunk = new RoadChunk(_chunksGenerados, world, roadSegment, decorations, surface, def.offsetLocal.Z);

            colaSegmentos.Enqueue(chunk);
            _chunksGenerados++;

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

                int roll = random.Next(100);
                Collectible nuevo;

                if (roll < 55) // 45% Moneda/Gema
                    nuevo = new FichaCollectible(_coinModel, worldPos - new Vector3(0f,1f,0f), 10);
                else if (roll < 70) // 25% Nafta
                    nuevo = new FuelCollectible(_fuelModel, worldPos, 25f);
                else if (roll < 85) // 15% Llave inglesa (Reparación)
                    nuevo = new WrenchCollectible(_wrenchModel, worldPos, 20f);
                else // 15% Obstáculo / Trampa
                    nuevo = new DamageCollectible(_obstacleModel, worldPos - new Vector3(0f,1f,0f), 15f);

                Collectibles.Add(nuevo);
            }
        }

        /// <summary>
        ///     Decide la mezcla de superficies del proximo chunk: zona estable de N chunks
        ///     y despues una transicion de TransitionChunks chunks hacia otra superficie.
        /// </summary>
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

        /// <summary>
        ///     Chunk mas cercano a una posicion (el que esta bajo el auto).
        ///     GetCurrentSegment devuelve el mas VIEJO de la cola, no este.
        /// </summary>
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
