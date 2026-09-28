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
        

        public RoadSpawner(Dictionary<RoadPieceType, RoadPiece> defs, Vector3 startPos,float spawnDistance, float despawnDistance, DecorationAreaFactory decorationFactory)
        {
            this.defs = defs;
            colaSegmentos = new Queue<RoadChunk>();
            random = new Random();
            _decorationFactory = decorationFactory;
            _chunksGenerados = 0;
            this.despawnDistance = despawnDistance;

            this.spawnDistance = spawnDistance;
            maxRoads = (int)((spawnDistance + despawnDistance) / TileLength) + 30;

            nextPos = startPos;
            nextRot = 0f;

            lastType = RoadPieceType.STRAIGHT;
            sameRoadRacha = 0;
            for (int i = 0; i < 40; i++)
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

            // Despawn inteligente: SOLO borrar si la pieza más vieja quedó LEJOS del auto
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

            // 1. Matriz de mundo de la pieza actual
            Matrix world = Matrix.CreateRotationY(nextRot) * Matrix.CreateTranslation(nextPos);

            var roadSegment = new RoadSegment(def.ModelInfo, world);
            var decorations = _decorationFactory.CreateFor(def, _chunksGenerados);
            var chunk = new RoadChunk(_chunksGenerados, world, roadSegment, decorations);

            colaSegmentos.Enqueue(chunk);
            _chunksGenerados++;

            // 2. Desplazamos el punto de spawn hacia el final de la pieza actual
            Vector3 offset = Vector3.Transform(def.offsetLocal, Matrix.CreateRotationY(nextRot));
            nextPos += offset;

            // 3. Acumulamos el ángulo para las piezas que vienen después
            nextRot += def.rotacionY;
            nextRot = MathHelper.WrapAngle(nextRot);

            lastType = typeNow;
            
            // Si la pieza fue recta sumamos racha; si fue curva la reiniciamos
            if (typeNow == RoadPieceType.STRAIGHT)
                sameRoadRacha++;
            else
                sameRoadRacha = 0;
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
