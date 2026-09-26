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
            {TipoVehiculo.LIGERO,new StatVehiculo(100f,10f,150f,100f,50f)},
            {TipoVehiculo.MEDIANO, new StatVehiculo(125f,5f,100f,85f,100f)},
            {TipoVehiculo.PESADO, new StatVehiculo(150f,1f,50f,50f,150f)}
        };
        public StatVehiculo stats;
        public ModelInfo modelI { get; }
        public float currentHealth, currentFuel;
        public Vector3 pos;
        public int score;
        public float carYaw=0f;
        public BoundingBox hitbox;

        public Vehiculo(StatVehiculo stats, ModelInfo model, Vector3 pos)
        {
            this.stats = stats;
            this.modelI = model;
            this.currentFuel = stats.maxFuel;
            this.currentHealth = stats.maxHealth;
            this.pos = pos;
        }

        //  Hay que aplicarle la logica del turnspeed el fuel etc
        //  Vector3 direccion = carworld.Foward
        public void Update(GameTime gameTime, KeyboardState keyboardState, Vector3 direccion) {  
            float velocidad = this.stats.accel;
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (keyboardState.IsKeyDown(Keys.A))
            {
                this.carYaw += MathHelper.ToRadians(100f) * elapsedTime;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                this.carYaw -= MathHelper.ToRadians(100f) * elapsedTime;
            }
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
            return Matrix.CreateRotationY(this.carYaw) * Matrix.CreateTranslation(this.pos);
        }

        public void DrawHitbox()
        {

        }

        public void Draw(Effect effect, Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in modelI.Model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                    effect.Parameters["World"].SetValue(this.getCarWorld());
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);
                    effect.Parameters["DiffuseColor"].SetValue(Color.White.ToVector3());
                }
                mesh.Draw();
            }
        }
    }
}
