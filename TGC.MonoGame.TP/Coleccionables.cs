using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace TGC.MonoGame.TP
{
    public abstract class Collectible
    {
        protected readonly ModelInfo modelI;
        protected readonly Vector3 pos;

        public bool Collected { get; private set; }

        protected Collectible(ModelInfo model, Vector3 pos)
        {
            this.modelI = model; 
            this.pos = pos;
            this.Collected = true;
        }

        //esto hay que pasarlo en la colision 
        public void tryCollect(Vehiculo vehiculo)
        {
            if (Collected) return;
            apply(vehiculo);
            Collected = true;
        }

        protected abstract void apply(Vehiculo vehiculo);

        public void Draw(Effect effect, Matrix view, Matrix projection,GameTime gameTime)
        {
            if (!Collected) {
                Matrix world = Matrix.CreateScale(modelI.Scale) *
                          Matrix.CreateRotationY((float)gameTime.ElapsedGameTime.TotalSeconds) *
                          Matrix.CreateTranslation(pos);

                foreach (ModelMesh mesh in modelI.Model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        part.Effect = effect;
                        effect.Parameters["World"].SetValue(world);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                        effect.Parameters["DiffuseColor"].SetValue(Color.White.ToVector3());
                    }
                    mesh.Draw();
                }
            }
        }
    }

    public class FichaCollectible : Collectible
    {
        private readonly int points;
        public FichaCollectible(ModelInfo model, Vector3 pos, int points) : base(model, pos) => this.points = points;
        protected override void apply(Vehiculo v) => v.score += points;
    }

    public class FuelCollectible : Collectible
    {
        private readonly float fuel;
        public FuelCollectible(ModelInfo model, Vector3 pos, float amount) : base(model, pos) => fuel = amount;
        protected override void apply(Vehiculo v) => v.currentFuel = Math.Min(v.currentFuel + fuel, v.stats.maxHealth);
    }

    public class WrenchCollectible : Collectible
    {
        private readonly float hp;
        public WrenchCollectible(ModelInfo model, Vector3 pos, float amount) : base(model, pos) => hp = amount;
        protected override void apply(Vehiculo v) => v.currentHealth = Math.Min(v.currentFuel + hp, v.stats.maxHealth);
    }
}
