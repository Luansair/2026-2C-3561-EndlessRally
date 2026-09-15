using System;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP;

/// <summary>
///     Cámara en tercera persona que sigue al vehículo desde atrás y arriba
/// </summary>
internal class FollowCamera
{
    // Distancias respecto al vehiculo
    private const float DistanceBack = 100f;
    private const float DistanceUp = 50f;

    // Velocidad de interpolación angular
    private const float AngleFollowSpeed = 5.0f;

    private Vector3 _currentBackVector = Vector3.Backward;
    private bool _isFirstFrame = true;

    public FollowCamera(float aspectRatio)
    {
        Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60f), aspectRatio, 0.1f, 100000f);
    }

    public Matrix Projection { get; private set; }
    public Matrix View { get; private set; }

    public void Update(GameTime gameTime, Matrix followedWorld)
    {
        var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
        var followedPosition = followedWorld.Translation;

        // Obtenemos el vector hacia atrás normalizado
        var followedBack = Vector3.Normalize(followedWorld.Backward);

        if (_isFirstFrame)
        {
            _currentBackVector = followedBack;
            _isFirstFrame = false;
        }

        // Interpolación continua para que la cámara acompañe el giro suavemente
        _currentBackVector = Vector3.Lerp(_currentBackVector, followedBack, MathF.Min(1f, elapsedTime * AngleFollowSpeed));
        _currentBackVector.Normalize();

        // Posición: atrás y arriba
        var cameraPosition = followedPosition 
                             + _currentBackVector * DistanceBack 
                             + Vector3.Up * DistanceUp;

        // Mirar hacia el centro de masa del auto
        Vector3 target = followedPosition + Vector3.Up * 2f;
        View = Matrix.CreateLookAt(cameraPosition, target, Vector3.Up);
    }
}