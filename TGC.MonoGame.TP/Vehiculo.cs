using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NVorbis.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using TGC.MonoGame.TP;

namespace TGC.MonoGame.TP
{
    public enum TipoVehiculo
    {
        LIGERO, MEDIANO, PESADO
    }
    public struct StatVehiculo
    {
        public readonly float maxFuel;
        public readonly float fuelConsumption;
        public readonly float turnSpeed;
        public readonly float accel;
        public readonly float maxHealth;

        public StatVehiculo(float maxFuel, float fuelConsumption, float accel, float turnSpeed, float maxHealth)
        {
            this.maxFuel = maxFuel;
            this.fuelConsumption = fuelConsumption;
            this.turnSpeed = turnSpeed;
            this.accel = accel;
            this.maxHealth = maxHealth;
        }
    }
   
    public class Vehiculo
    {
        public static Dictionary<TipoVehiculo, StatVehiculo> VEHICULOSDEFS = new Dictionary<TipoVehiculo, StatVehiculo> {
            {TipoVehiculo.LIGERO,new StatVehiculo(100f,10f,50f,100f,50f)},
            {TipoVehiculo.MEDIANO, new StatVehiculo(125f,5f,30f,85f,100f)},
            {TipoVehiculo.PESADO, new StatVehiculo(150f,1f,20f,50f,150f)}
        };
        public StatVehiculo stats;
        public ModelInfo modelI { get; }
        public TipoVehiculo Tipo { get; }

        public float currentHealth { get; set; }
        public float currentFuel { get; set; }
        public Vector3 pos;
        public int score;
        public float carYaw;
        public BoundingBox hitbox;

        public BoundingBox hitboxWorld => GizmoPrimitives.TransformAABB(hitbox, getCarWorld());
        private readonly Matrix[] _boneTransforms;

        public Vehiculo(TipoVehiculo tipo, ModelInfo model, Vector3 initialPos, float initialYaw)
        {
            this.Tipo = tipo;
            this.stats = VEHICULOSDEFS[tipo];
            this.modelI = model;
            this.pos = initialPos;
            this.carYaw = initialYaw;
            
            this.currentFuel = stats.maxFuel;
            this.currentHealth = stats.maxHealth;
            _boneTransforms = new Matrix[modelI.Model.Bones.Count];
            modelI.Model.CopyAbsoluteBoneTransformsTo(_boneTransforms);
            this.hitbox = GizmoPrimitives.CreateAABBFrom(model.Model);
        }

        //  Hay que aplicarle la logica del turnspeed el fuel etc
        //  Vector3 direccion = carworld.Foward
        public void Update(GameTime gameTime, KeyboardState keyboardState) {  
            float velocidad = this.stats.accel * 100f;
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (keyboardState.IsKeyDown(Keys.A))
            {
                this.carYaw += MathHelper.ToRadians(100f) * elapsedTime;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                this.carYaw -= MathHelper.ToRadians(100f) * elapsedTime;
            }
            Vector3 direccion = this.getCarWorld().Forward;
            if (keyboardState.IsKeyDown(Keys.W))
            {
                this.pos += direccion * velocidad * elapsedTime;
            }
            if (keyboardState.IsKeyDown(Keys.S))
            {
                this.pos -= direccion * velocidad * elapsedTime;
            }
        }

        public Matrix getCarWorld()
        {
            return Matrix.CreateScale(modelI.Scale) * Matrix.CreateRotationY(this.carYaw) * Matrix.CreateTranslation(this.pos);
        }

        public void Draw(Effect effect, Matrix view, Matrix projection, Matrix? customWorld = null)
        {
            Matrix carWorld = customWorld ?? getCarWorld();

            var model = modelI.Model;
            var boneTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            effect.Parameters["baseTexture"]?.SetValue(TGCGame.DefaultTexture);

            foreach (var mesh in model.Meshes)
            {
                Matrix meshWorld = boneTransforms[mesh.ParentBone.Index] * carWorld;
                Matrix invTranspose = Matrix.Transpose(Matrix.Invert(meshWorld));

                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = effect;

                    effect.Parameters["World"]?.SetValue(meshWorld);
                    effect.Parameters["View"]?.SetValue(view);
                    effect.Parameters["Projection"]?.SetValue(projection);
                    effect.Parameters["InverseTransposeWorld"]?.SetValue(invTranspose);

                    // Asignamos el color propio de esta parte específica
                    if (modelI.PartMaterials.TryGetValue(part, out var mat))
                    {
                        effect.Parameters["DiffuseColor"]?.SetValue(mat.DiffuseColor);
                    }
                    else
                    {
                        effect.Parameters["DiffuseColor"]?.SetValue(Color.White.ToVector3());
                    }

                    // DIBUJAMOS ESTA PARTE ESPECÍFICA ANTES DE PASAR A LA SIGUIENTE
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
                // Ya no se llama a mesh.Draw() acá afuera, eso causaba que no cargue el color de cada parte
            }
        }
    }
}
