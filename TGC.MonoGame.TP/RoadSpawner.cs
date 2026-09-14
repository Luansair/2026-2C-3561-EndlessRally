using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime;

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
        

        public RoadSpawner(Dictionary<RoadPieceType, RoadPiece> defs, Vector3 startPos,float spawnDistance, float despawnDistance, DecorationAreaFactory decorationFactory)
        {
            this.defs = defs;
            colaSegmentos = new Queue<RoadChunk>();
            random = new Random();
            _decorationFactory = decorationFactory;
            _chunksGenerados = 0;

            this.spawnDistance = spawnDistance;
            maxRoads = (int)((spawnDistance + despawnDistance) / TileLength) + 30;

            nextPos = startPos;
            nextRot = 0f;

            lastType = RoadPieceType.STRAIGHT;
            sameRoadRacha = 0;
            for (int i = 0; i < maxRoads; i++)
            {
                SpawnNext();
            }
        }

        public void Update(Vector3 carPosition)
        {
            //    SpawnNext();
            while (Vector3.Distance(nextPos, carPosition) < spawnDistance)
            {
                SpawnNext();
            }

            //culling/despawn
            while (colaSegmentos.Count > maxRoads)
            {
                colaSegmentos.Dequeue();
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
            curving = !curving ? typeNow == RoadPieceType.CURVEDSPLIT || typeNow == RoadPieceType.CURVEDSPLITLEFT: false;

            RoadPiece def = defs[typeNow];

            Matrix world = Matrix.CreateRotationY(nextRot) * Matrix.CreateTranslation(nextPos);

            var roadSegment = new RoadSegment(def.model, world);
            var decorations = _decorationFactory.CreateFor(def, _chunksGenerados);
            var chunk = new RoadChunk(_chunksGenerados, world, roadSegment, decorations);

            colaSegmentos.Enqueue(chunk);
            _chunksGenerados++;

            Vector3 offset = Vector3.Transform(def.offsetLocal, Matrix.CreateRotationY(nextRot));
            nextPos += offset;
            //esto es para que no vaya hacia atras y no pueda hacer una vuelta cerrada
            nextRot += nextRot + def.rotacionY != 0 ? -def.rotacionY : def.rotacionY;
            
            lastType = typeNow;
            if (!curving) sameRoadRacha = typeNow == lastType ? sameRoadRacha + 1 : 1;

        }

        public RoadChunk GetCurrentSegment()
        {
            if (colaSegmentos.Count == 0) return null;
            return colaSegmentos.Peek();
        }
        bool curving  = false;
        int curvingCount  = 2;
        private RoadPieceType nextType()
        {
            if (curving) //para evitar 2 curvas segudas
            {
                curvingCount--;
                if (curvingCount == 0)
                {
                    curving = false;
                    curvingCount = 2;
                }
                return RoadPieceType.STRAIGHT;
            }
            if (sameRoadRacha <= 2)
            {
                return RoadPieceType.STRAIGHT;
            }

            RoadPieceType[] pool =
            [
                RoadPieceType.STRAIGHT,
                RoadPieceType.STRAIGHT,
                RoadPieceType.STRAIGHT,
                RoadPieceType.RAMP,
                RoadPieceType.RAMP,
                RoadPieceType.CURVEDSPLIT,
                RoadPieceType.CURVEDSPLITLEFT,
            ];

            return pool[random.Next(pool.Length)];
        }
    }
}
