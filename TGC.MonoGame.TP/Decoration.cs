using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP
{
    public enum DecorationType
    {
        Tree,
        Rock,
        Bush,
        Flower
    }
    /// <summary>
    ///     Esta clase es la que se encarga de manejar el arbol.
    /// </summary>
    public class Decoration
    {
        private ModelInfo _model;
        private DecorationType _type;

        private Vector3 position;
        private float rotation;
        public float scale;

        public Decoration(ModelInfo model, DecorationType type, Vector3 position, float rotation, float scale)
        {
            this._model = model;
            this._type = type;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public void Draw(GraphicsDevice graphicsDevice, Effect effect, Matrix view, Matrix projection)
        {
            Matrix world = Matrix.CreateScale(scale * _model.Scale) *
                           Matrix.CreateRotationY(rotation) *
                           Matrix.CreateTranslation(position);

            foreach (ModelMesh mesh in _model.Model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);
                    if (_type == DecorationType.Tree)
                    {
                        effect.Parameters["DiffuseColor"].SetValue(Color.DarkGreen.ToVector3());
                    }
                    else if(_type == DecorationType.Rock)
                    {
                        effect.Parameters["DiffuseColor"].SetValue(Color.Gray.ToVector3());
                    }
                }
                mesh.Draw();
            }
        }
    }
}

