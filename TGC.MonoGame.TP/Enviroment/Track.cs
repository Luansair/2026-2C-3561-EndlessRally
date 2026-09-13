using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace TGC.MonoGame.TP
{
    internal class RaceTrack
    {
        private List<Track> _tracks;
        private Vector3 _begin;
        private ModelInfo[] _trackModels;

        public RaceTrack(ModelInfo[] track_models, Vector3 begin, int track_count)
        {
            _trackModels = track_models;
            _begin = begin;
            _tracks = new List<Track>();

            for (int i = 0; i < track_count; i++ )
            {
                ModelInfo track_model_picked = _trackModels[i % _trackModels.Length];
                Vector3 track_position = (i == 0) ? _begin :  new Vector3(
                                                                _tracks[^1].get_position().X,
                                                                _tracks[^1].get_position().Y,
                                                                _tracks[^1].get_position().Z + get_model_size(track_model_picked.Model).Z * 10
                                                                );
                float track_rotation = (i == 0) ? 0 : _tracks[^1].get_rotation(); //TO DO logica segun tipo de track
                float track_scale = track_model_picked.Scale * 10;
                Track track = new Track(track_model_picked.Model, track_position, track_rotation, track_scale, 1);
                _tracks.Add(track);
            }
        }

        public Vector3 get_model_size(Model model)
        {
            Vector3 min = Vector3.Zero;
            Vector3 max = Vector3.Zero;
            bool initialized = false;

            foreach (var mesh in model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    // Obtener vértices del buffer
                    var vertices = new VertexPositionNormalTexture[part.NumVertices];
                    part.VertexBuffer.GetData(vertices);

                    // Encontrar min/max de posiciones
                    foreach (var vertex in vertices)
                    {
                        if (!initialized)
                        {
                            min = max = vertex.Position;
                            initialized = true;
                        }
                        else
                        {
                            min = Vector3.Min(min, vertex.Position);
                            max = Vector3.Max(max, vertex.Position);
                        }
                    }
                }
            }
            return max - min;
        }
        public void Draw(GraphicsDevice graphicsDevice, Effect effect, Matrix view, Matrix projection)
        {
            foreach (Track track in _tracks)
            {
                track.Draw(graphicsDevice, effect, view, projection);
            }
        }
    }

    public class Track
    {
        private Model _model;

        private Vector3 position;
        private float rotation;
        public float scale;
        public float size;


        public Track(Model model, Vector3 position, float rotation, float scale, float size)
        {
            this._model = model;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.size = size;
        }
        public Model get_model(){return _model;}
        public Vector3 get_position(){return position;}
        public float get_rotation(){return rotation;}
        public void Draw(GraphicsDevice graphicsDevice, Effect effect, Matrix view, Matrix projection)
        {
            // Crear la matriz de mundo para el árbol
            Matrix world = Matrix.CreateScale(scale) *
                           Matrix.CreateRotationY(rotation) *
                           Matrix.CreateTranslation(position);

            // Dibujar el modelo del árbol con la matriz de mundo, vista y proyección
            foreach (ModelMesh mesh in _model.Meshes)
            {
                var modelMeshesBaseTransforms = new Matrix[_model.Bones.Count];
                _model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);
                    effect.Parameters["DiffuseColor"].SetValue(Vector3.One);
                }
                mesh.Draw();
            }
        }
    }


}
