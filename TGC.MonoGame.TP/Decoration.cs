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
    ///     Clase encargada de posicionar y renderizar elementos decorativos del escenario.
    /// </summary>
    public class Decoration
    {
        private readonly ModelInfo _model;
        private readonly DecorationType _type;

        private Vector3 position;
        private float rotation;
        public float scale;

        public Decoration(ModelInfo model, DecorationType type, Vector3 position, float rotation, float scale)
        {
            _model = model;
            _type = type;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        /// <summary>
        ///     Dibuja la decoración en coordenadas absolutas del mundo.
        /// </summary>
        public void Draw(Effect effect, Matrix view, Matrix projection)
        {
            DrawRelativeTo(Matrix.Identity, effect, view, projection);
        }

        /// <summary>
        ///     Dibuja la decoración relativa al sistema de coordenadas de un tramo de pista / chunk.
        /// </summary>
        public void DrawRelativeTo(Matrix parentWorld, Effect effect, Matrix view, Matrix projection)
        {
            Matrix world = Matrix.CreateScale(scale * _model.Scale) *
                           Matrix.CreateRotationY(rotation) *
                           Matrix.CreateTranslation(position) *
                           parentWorld;

            var boneTransforms = new Matrix[_model.Model.Bones.Count];
            _model.Model.CopyAbsoluteBoneTransformsTo(boneTransforms);

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

                    // Determinamos textura y color específicos para esta parte
                    Texture2D textureToUse = TGCGame.DefaultTexture;
                    Vector3 diffuseColor;

                    if (_model.PartMaterials.TryGetValue(part, out var mat))
                    {
                        if (mat.Texture != null)
                        {
                            // Si la parte trajo textura (como las casas): textura + blanco
                            textureToUse = mat.Texture;
                            diffuseColor = Vector3.One;
                        }
                        else if (mat.DiffuseColor != Vector3.Zero)
                        {
                            // Color propio de material en el FBX (ventanas, techos, etc.)
                            diffuseColor = mat.DiffuseColor;
                        }
                        else
                        {
                            // Color por defecto si el material vino en (0,0,0)
                            diffuseColor = GetFallbackColor(_type);
                        }
                    }
                    else
                    {
                        textureToUse = _model.Texture ?? TGCGame.DefaultTexture;
                        diffuseColor = _model.Texture != null ? Vector3.One : GetFallbackColor(_type);
                    }

                    effect.Parameters["baseTexture"]?.SetValue(textureToUse);
                    effect.Parameters["DiffuseColor"]?.SetValue(diffuseColor);

                    // Dibujamos esta parte específica de forma independiente
                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        effect.GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        effect.GraphicsDevice.Indices = part.IndexBuffer;
                        effect.GraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            part.VertexOffset,
                            part.StartIndex,
                            part.PrimitiveCount
                        );
                    }
                }
            }
        }

        // Método auxiliar para colores de respaldo cuando el FBX no define materiales válidos
        private Vector3 GetFallbackColor(DecorationType type) => type switch
        {
            DecorationType.Tree => new Vector3(0.20f, 0.65f, 0.20f),    // Verde hoja
            DecorationType.Rock => new Vector3(0.65f, 0.65f, 0.68f),    // Gris piedra
            DecorationType.Bush => new Vector3(0.15f, 0.55f, 0.15f),    // Verde arbusto
            DecorationType.Flower => new Vector3(0.95f, 0.30f, 0.35f),  // Tono claro flor
            DecorationType.House => new Vector3(0.85f, 0.80f, 0.75f),   // beige
            _ => Vector3.One
        };
    }
}