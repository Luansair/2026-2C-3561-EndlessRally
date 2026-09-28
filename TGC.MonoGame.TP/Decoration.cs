using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP
{
    public enum DecorationType
    {
        Tree,
        Rock,
        Bush,
        Flower,
        House
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

        public void DrawRelativeTo(Matrix parentWorld, Effect effect, Matrix view, Matrix projection)
        {
            Matrix world = Matrix.CreateScale(scale * _model.Scale) *
                        Matrix.CreateRotationY(rotation) *
                        Matrix.CreateTranslation(position) *
                        parentWorld;

            var boneTransforms = new Matrix[_model.Model.Bones.Count];
            _model.Model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            // 1. Colores vivos y explícitos para no depender del (0,0,0) del FBX
            Vector3 diffuseColor = _type switch
            {
                DecorationType.Tree => new Vector3(0.20f, 0.65f, 0.20f),    // Verde hoja brillante
                DecorationType.Rock => new Vector3(0.65f, 0.65f, 0.68f),    // Gris piedra claro
                DecorationType.House => new Vector3(0.85f, 0.80f, 0.75f),   // Cemento / beige claro
                _ => Vector3.One
            };

            // 2. Si el modelo traía textura propia la usa; si no, textura blanca 1x1
            Texture2D textureToUse = _model.Texture ?? TGCGame.DefaultTexture;

            foreach (ModelMesh mesh in _model.Model.Meshes)
            {
                Matrix meshWorld = boneTransforms[mesh.ParentBone.Index] * world;
                Matrix invTranspose = Matrix.Transpose(Matrix.Invert(meshWorld));

                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;

                    effect.Parameters["World"]?.SetValue(meshWorld);
                    effect.Parameters["View"]?.SetValue(view);
                    effect.Parameters["Projection"]?.SetValue(projection);
                    effect.Parameters["InverseTransposeWorld"]?.SetValue(invTranspose);

                    effect.Parameters["baseTexture"]?.SetValue(textureToUse);
                    effect.Parameters["DiffuseColor"]?.SetValue(diffuseColor);
                }

                mesh.Draw();
            }
        }
    }
}

