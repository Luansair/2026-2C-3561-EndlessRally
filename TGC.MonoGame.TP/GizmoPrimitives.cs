using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public static class GizmoPrimitives
{
    /// <summary>
    /// Dibuja un cubo de wireframe (bounding box) dado su vértice mínimo y máximo,
    /// usando un Effect propio (no BasicEffect).
    /// </summary>
    public static void DrawBoundingBox(GraphicsDevice graphicsDevice, Effect effect,
        Vector3 min, Vector3 max, Matrix world, Matrix view, Matrix projection, Color color)
    {
        var corners = new VertexPosition[8]
        {
            new VertexPosition(new Vector3(min.X, min.Y, min.Z)), // 0
            new VertexPosition(new Vector3(max.X, min.Y, min.Z)), // 1
            new VertexPosition(new Vector3(max.X, max.Y, min.Z)), // 2
            new VertexPosition(new Vector3(min.X, max.Y, min.Z)), // 3
            new VertexPosition(new Vector3(min.X, min.Y, max.Z)), // 4
            new VertexPosition(new Vector3(max.X, min.Y, max.Z)), // 5
            new VertexPosition(new Vector3(max.X, max.Y, max.Z)), // 6
            new VertexPosition(new Vector3(min.X, max.Y, max.Z)), // 7
        };

        var indices = new short[]
        {
            0,1, 1,2, 2,3, 3,0, // cara trasera
            4,5, 5,6, 6,7, 7,4, // cara delantera
            0,4, 1,5, 2,6, 3,7  // uniones entre caras
        };

        // Seteás los parámetros de TU shader (ajustá los nombres si son distintos)
        effect.Parameters["World"]?.SetValue(world);
        effect.Parameters["View"]?.SetValue(view);
        effect.Parameters["Projection"]?.SetValue(projection);
        effect.Parameters["DiffuseColor"]?.SetValue(color.ToVector3());

        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.LineList,
                corners, 0, 8,
                indices, 0, 12);
        }
    }
}