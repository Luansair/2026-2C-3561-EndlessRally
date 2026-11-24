using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public static class GizmoPrimitives
{
    /// <summary>
    /// Dibuja un cubo de wireframe (bounding box) dado su vértice mínimo y máximo,
    /// usando un Effect propio (no BasicEffect).
    /// </summary>
    public static BoundingBox TransformAABB(BoundingBox box, Matrix world)
    {
        var corners = box.GetCorners();
        for (int i = 0; i < corners.Length; i++)
            corners[i] = Vector3.Transform(corners[i], world);
        return BoundingBox.CreateFromPoints(corners);
    }
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
    public static BoundingBox CreateAABBFrom(Model model)
    {
        var minPoint = Vector3.One * float.MaxValue;
        var maxPoint = Vector3.One * float.MinValue;

        var transforms = new Matrix[model.Bones.Count];
        model.CopyAbsoluteBoneTransformsTo(transforms);

        var meshes = model.Meshes;
        for (int index = 0; index < meshes.Count; index++)
        {
            var meshParts = meshes[index].MeshParts;
            for (int subIndex = 0; subIndex < meshParts.Count; subIndex++)
            {
                var vertexBuffer = meshParts[subIndex].VertexBuffer;
                var declaration = vertexBuffer.VertexDeclaration;
                var vertexSize = declaration.VertexStride / sizeof(float);

                var rawVertexBuffer = new float[vertexBuffer.VertexCount * vertexSize];
                vertexBuffer.GetData(rawVertexBuffer);

                for (var vertexIndex = 0; vertexIndex < rawVertexBuffer.Length; vertexIndex += vertexSize)
                {
                    var transform = transforms[meshes[index].ParentBone.Index];
                    var vertex = new Vector3(rawVertexBuffer[vertexIndex], rawVertexBuffer[vertexIndex + 1], rawVertexBuffer[vertexIndex + 2]);
                    vertex = Vector3.Transform(vertex, transform);
                    minPoint = Vector3.Min(minPoint, vertex);
                    maxPoint = Vector3.Max(maxPoint, vertex);
                }
            }
        }

        return new BoundingBox(minPoint, maxPoint);
    }
}