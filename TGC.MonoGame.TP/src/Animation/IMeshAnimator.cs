using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.src.Animation
{
    /// <summary>
    ///     Permite que un animador reemplace la matriz de mundo con la que se dibuja cada mesh.
    /// </summary>
    internal interface IMeshAnimator
    {
        /// <param name="boneAbsolute">Matriz absoluta del hueso del mesh (ya incluye la jerarquia del FBX).</param>
        /// <param name="world">Matriz de mundo de la entidad (escala * rotacion * traslacion).</param>
        Matrix GetMeshWorld(ModelMesh mesh, Matrix boneAbsolute, Matrix world);
    }
}
