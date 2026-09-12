using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP
{

    /// <summary>
    ///     Esta clase es la que se encarga de manejar el arbol.
    /// </summary>
    public class Tree
    {
        private Model _model;

        private Vector3 position;
        private float rotation;
        public float scale;

        public Tree(Model model, Vector3 position, float rotation, float scale)
        {
            this._model = model;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public void Draw(GraphicsDevice graphicsDevice, Effect effect, Matrix view, Matrix projection)
        {
            // Crear la matriz de mundo para el árbol
            Matrix world = Matrix.CreateScale(scale) *
                           Matrix.CreateRotationY(rotation) *
                           Matrix.CreateTranslation(position);

            // Dibujar el modelo del árbol con la matriz de mundo, vista y proyección
            foreach (ModelMesh mesh in _model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);
                }
                mesh.Draw();
            }
        }
    }
}
