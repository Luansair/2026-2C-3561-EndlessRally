using System;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP;

/// <summary>
///     Cámara en tercera persona que sigue al vehículo desde atrás y arriba
/// </summary>
internal class FollowCamera
{
    // Distancia hacia atrás y hacia arriba respecto al vehículo
    private const float DistanceBack = 160f;
    private const float DistanceUp = 60f;

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

    public Matrix Projection { get; private set; }
    public Matrix View { get; private set; }

    /// <summary>
    ///     Actualiza la posición y orientación de la cámara siguiendo la matriz de mundo dada.
    /// </summary>
    /// <param name="gameTime">Tiempo transcurrido para independizar del framerate.</param>
    /// <param name="followedWorld">Matriz de mundo del objeto a seguir (ej. chasis del auto).</param>
    public void Update(GameTime gameTime, Matrix followedWorld)
    {
        var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

        var followedPosition = followedWorld.Translation;
        var followedBack = followedWorld.Backward;

        // Si la rotación entre el vector trasero previo y el actual es suave, interpolamos
        if (Vector3.Dot(followedBack, _pastBackVector) > AngleThreshold)
        {
            _backVectorInterpolator += elapsedTime * AngleFollowSpeed;
            _backVectorInterpolator = MathF.Min(_backVectorInterpolator, 1f);

            // Interpolación no lineal (curva x^2) para suavizar el inicio del giro
            _currentBackVector = Vector3.Lerp(
                _currentBackVector, 
                followedBack, 
                _backVectorInterpolator * _backVectorInterpolator
            );
        }
        else
        {
            // Si el cambio de dirección fue brusco (ej. teletransporte o trompo), reseteamos el interpolador
            _backVectorInterpolator = 0f;
        }

        _pastBackVector = followedBack;

        // Posición final de la cámara: detrás del auto y elevada en Y
        var cameraPosition = followedPosition
                             + _currentBackVector * DistanceBack
                             + Vector3.Up * DistanceUp;

        // Vector de visión hacia el vehículo
        var forward = followedPosition - cameraPosition;
        forward.Normalize();

        // Si el auto no está completamente vertical, recalculamos el vector Up ortogonal
        var right = Vector3.Cross(forward, Vector3.Up);
        
        // Evitamos división por cero o NaN si el forward apunta directo hacia abajo
        Vector3 cameraCorrectUp = Vector3.Up;
        if (right.LengthSquared() > 0.0001f)
        {
            right.Normalize();
            cameraCorrectUp = Vector3.Cross(right, forward);
        }

        // Target: Podés sumar un pequeño offset en Y (ej. + Vector3.Up * 10f) para que mire al centro de masa del auto
        View = Matrix.CreateLookAt(cameraPosition, followedPosition, cameraCorrectUp);
    }
}