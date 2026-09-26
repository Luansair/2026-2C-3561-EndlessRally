using System;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP;

/// <summary>
///     Cámara en tercera persona que sigue al vehículo desde atrás y arriba
/// </summary>
internal class FollowCamera
{
    // Distancia hacia atrás y hacia arriba respecto al vehículo
    private const float DistanceBack = 50f;
    private const float DistanceUp = 20f;

    // Control de interpolación angular
    private const float AngleFollowSpeed = 2.5f; // Ajustado para que acompañe de forma natural
    private const float AngleThreshold = 0.85f;

    private Vector3 _currentBackVector = Vector3.Backward;
    private Vector3 _pastBackVector = Vector3.Backward;
    private float _backVectorInterpolator;

    public FollowCamera(float aspectRatio)
    {
        // 60 grados de FOV horizontal/vertical según la relación de aspecto
        Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60f), aspectRatio, 0.1f, 100000f);
    }

    private bool _initialized = false;

    public Matrix Projection { get; private set; }
    public Matrix View { get; private set; }

    public void Reset(Matrix initialWorld)
    {
        _currentBackVector = Vector3.Normalize(initialWorld.Backward);
        _initialized = true;
    }

    /// <summary>
    ///     Actualiza la posición y orientación de la cámara siguiendo la matriz de mundo dada.
    /// </summary>
    /// <param name="gameTime">Tiempo transcurrido para independizar del framerate.</param>
    /// <param name="followedWorld">Matriz de mundo del objeto a seguir (ej. chasis del auto).</param>
    public void Update(GameTime gameTime, Matrix followedWorld)
    {
        var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
        var followedPosition = followedWorld.Translation;
        var followedBack = Vector3.Normalize(followedWorld.Backward);

        // Si es la primera vez o se reseteó, colocamos la cámara directamente atrás
        if (!_initialized)
        {
            _currentBackVector = followedBack;
            _initialized = true;
        }

        // Interpolación continua y suave para que la cámara acompañe los giros
        _currentBackVector = Vector3.Lerp(_currentBackVector, followedBack, MathF.Min(1f, elapsedTime * AngleFollowSpeed));
        _currentBackVector.Normalize();

        // Posición final de la cámara: detrás del auto y elevada en Y
        var cameraPosition = followedPosition
                            + _currentBackVector * DistanceBack
                            + Vector3.Up * DistanceUp;

        // Vector de visión hacia el vehículo
        var forward = followedPosition - cameraPosition;
        forward.Normalize();

        // Recalculamos el vector Up ortogonal
        var right = Vector3.Cross(forward, Vector3.Up);
        Vector3 cameraCorrectUp = Vector3.Up;
        if (right.LengthSquared() > 0.0001f)
        {
            right.Normalize();
            cameraCorrectUp = Vector3.Cross(right, forward);
        }

        // Miramos al centro de masa del auto (elevado un poco en Y)
        Vector3 target = followedPosition + Vector3.Up * 2f;
        View = Matrix.CreateLookAt(cameraPosition, target, cameraCorrectUp);
    }
}