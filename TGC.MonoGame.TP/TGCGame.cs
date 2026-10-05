using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace TGC.MonoGame.TP;

/// <summary>
///     Esta es la clase principal del juego.
///     Inicialmente puede ser renombrado o copiado para hacer mas ejemplos chicos, en el caso de copiar para que se
///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
/// </summary>
public class TGCGame : Game
{
    public const string ContentFolder3D = "Models/";
    public const string ContentFolderEffects = "Effects/";
    public const string ContentFolderMusic = "Music/";
    public const string ContentFolderSounds = "Sounds/";
    public const string ContentFolderSpriteFonts = "SpriteFonts/";
    public const string ContentFolderTextures = "Textures/";
    
    private readonly GraphicsDeviceManager _graphics;
    private GameAssets _assets;

    private Effect _effect;

    // Una camara
    private FollowCamera _followCamera;

    private SpriteBatch _spriteBatch;

    private Random _random;
    private const int SEED = 0;


    //definimos estados de juego (menu, playing, paused)
    public enum GameState
    {
        Menu,
        Playing,
        Paused
    }

    private GameState _estadoActual = GameState.Menu;

    private readonly List<Vehiculo> _opcionesVehiculos = new();
    private Vehiculo _vehiculoActual;
    private int _indiceVehiculoSeleccionado = 0;
    private bool _enCarrera = false;
    private KeyboardState _prevKeyboard;

    //pantalla de menu
    private SpriteFont _font;
    private float _menuCarYaw = 30f;
    public static Texture2D DefaultTexture { get; private set; }

    // Geometría del piso (vertices e indices)
    private VertexPositionNormalTexture[] _floorVertices;
    private short[] _floorIndices;

    /// <summary>
    ///     Constructor del juego.
    /// </summary>
    public TGCGame()
    {
        // Maneja la configuracion y la administracion del dispositivo grafico.
        _graphics = new GraphicsDeviceManager(this);

        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    /// <summary>
    ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
    ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
    /// </summary>
    protected override void Initialize()
    {
        // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.
        //creo una camara para seguir a un _vehiculoActual
        _followCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio);
        
        //crea el piso con un determinado tamaño
        CreateFloorGeometry(50000f);
        base.Initialize();
    }

    RoadSpawner _roadSpawner;

    ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
    ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
    ///     que podemos pre calcular para nuestro juego.
    /// </summary>
    
    protected override void LoadContent()
    {
        // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "DefaultFont");

        DefaultTexture = new Texture2D(GraphicsDevice, 1, 1);
        DefaultTexture.SetData(new[] { Color.White });
        
        _effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
        _effect.Parameters["baseTexture"]?.SetValue(DefaultTexture);
        
        //les asignamos valores a los parametros de iluminacion
        _effect.Parameters["lightAmbientColor"]?.SetValue(new Vector3(0.8f, 0.8f, 0.8f));
        _effect.Parameters["KAmbient"]?.SetValue(0.60f);
        
        _effect.Parameters["lightDiffuseColor"]?.SetValue(new Vector3(1.0f, 0.98f, 0.92f));
        _effect.Parameters["KDiffuse"]?.SetValue(0.70f);
        
        _effect.Parameters["lightSpecularColor"]?.SetValue(new Vector3(1.0f, 1.0f, 1.0f));
        _effect.Parameters["KSpecular"]?.SetValue(0.20f);
        _effect.Parameters["shininess"]?.SetValue(18.0f);

        var contentLoader = new GameContentLoader();
        _assets = contentLoader.Load(Content, GraphicsDevice);

        Vector3 largada = new Vector3(0f, 0f, 0f);

        _opcionesVehiculos.Add(new Vehiculo(TipoVehiculo.LIGERO, _assets.Cars.LightCar, largada, MathHelper.Pi));
        _opcionesVehiculos.Add(new Vehiculo(TipoVehiculo.MEDIANO, _assets.Cars.MediumCar, largada, MathHelper.Pi));
        _opcionesVehiculos.Add(new Vehiculo(TipoVehiculo.PESADO, _assets.Cars.HeavyCar, largada, MathHelper.Pi));

        _vehiculoActual = _opcionesVehiculos[_indiceVehiculoSeleccionado];

        _random = new Random(SEED);

        var treesGroup = new DecorationGroup(DecorationType.Tree, 3, [_assets.Decorations.Tree]);
        var rocksGroup = new DecorationGroup(DecorationType.Rock, 1, _assets.Decorations.Rocks);
        var houseGroup = new DecorationGroup(DecorationType.House, 1, [_assets.Decorations.House]);

        var natureRecipes = new List<DecorationGroup> { treesGroup, rocksGroup };
        var houseRecipes = new List<DecorationGroup> { houseGroup };

        var decorationsFactory = new DecorationAreaFactory(natureRecipes, houseRecipes);
        var collectibleFactory = new CollectibleFactory(_assets.Collectibles);
        var roadSpawnerFactory = new RoadSpawnerFactory(_assets.Roads, decorationsFactory, collectibleFactory);

        foreach (var coll in new ModelInfo[] { _assets.Collectibles.Wrench, _assets.Collectibles.Coin, _assets.Collectibles.Fuel, _assets.Collectibles.Obstacle })
        {
            foreach (var mesh in coll.Model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = _effect;
                }
            }
        }
        var spawnerConfig = new RoadSpawnerConfig
        {
            StartPosition = new Vector3(0f, 0.05f, 0f),
            SpawnDistance = 800f,
            DespawnDistance = 1600f,
            InitialSegmentCount = 100
        };

        _roadSpawner = roadSpawnerFactory.Create(spawnerConfig);

        base.LoadContent();
    }

    /// <summary>
    ///     Se llama en cada frame.
    ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
    ///     ante ellas.
    /// </summary>
    protected override void Update(GameTime gameTime)
    {
        float elapsedTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
        var keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            //Salgo del juego.
            Exit();
        }
        if (!_enCarrera)
        {
            _menuCarYaw += elapsedTime * 1.5f;
            // Cambiar de auto con flechas
            if (keyboardState.IsKeyDown(Keys.Right) && _prevKeyboard.IsKeyUp(Keys.Right))
            {
                _indiceVehiculoSeleccionado = (_indiceVehiculoSeleccionado + 1) % _opcionesVehiculos.Count;
                _vehiculoActual = _opcionesVehiculos[_indiceVehiculoSeleccionado];
            }
            if (keyboardState.IsKeyDown(Keys.Left) && _prevKeyboard.IsKeyUp(Keys.Left))
            {
                _indiceVehiculoSeleccionado = (_indiceVehiculoSeleccionado - 1 + _opcionesVehiculos.Count) % _opcionesVehiculos.Count;
                _vehiculoActual = _opcionesVehiculos[_indiceVehiculoSeleccionado];
            }

            // comenzar
            if (keyboardState.IsKeyDown(Keys.Enter) && _prevKeyboard.IsKeyUp(Keys.Enter))
            {
                _enCarrera = true;
                _estadoActual = GameState.Playing;
                _followCamera.Update(gameTime, _vehiculoActual.getCarWorld());
            }
        }
        else
        {
            Vector3 lightOffset = new Vector3(400f, 500f, 400f);
            Vector3 lightPosition = _vehiculoActual.pos + lightOffset;
            _effect.Parameters["lightPosition"]?.SetValue(lightPosition);
            _effect.Parameters["eyePosition"]?.SetValue(_followCamera.Position);
            // Todo el movimiento se delega a la clase
            _vehiculoActual.Update(gameTime, keyboardState);
            _followCamera.Update(gameTime, _vehiculoActual.getCarWorld());
            checkCollisions();
        }

        _prevKeyboard = keyboardState;


        //_vehiculoActual.Update(gameTime, keyboardState, _carWorld.Forward);
        
        this.Window.Title = "Score: " + _vehiculoActual.score + " Fuel: " + _vehiculoActual.currentFuel + " Health: " + _vehiculoActual.currentHealth;
        //Actualizo la matriz de mundo del _vehiculoActual con la rotacion respecto al eje Y 
        // y con el vector3 de posicion, siguiendo la regla de SRT

        //_carWorld = Matrix.CreateRotationY(carYaw) * Matrix.CreateTranslation(_carPosition);
        //_carWorld = _vehiculoActual.getCarWorld();
        // Actualizo la camara, enviandole la matriz de mundo del _vehiculoActual.
        //_followCamera.Update(gameTime, _carWorld);
        //_roadSpawner.Update(_carPosition);
        _roadSpawner.Update(_vehiculoActual.pos);


        base.Update(gameTime);
    }

    private void checkCollisions()
    {
        foreach (Collectible coll in _roadSpawner.collectibles)
        {
            if (coll.Collected) continue;
            coll.tryCollect(_vehiculoActual);
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        if (_estadoActual == GameState.Playing)
        {
            
            DrawGame(gameTime);
        }
        if(_estadoActual == GameState.Menu)
        {
            DrawMenu();
        }

        base.Draw(gameTime);
    }

    private void DrawMenu()
    {
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

        GraphicsDevice.Clear(new Color(24, 26, 32));
        Vector3 menuCameraPos = new Vector3(0f, 6f, 18f);
        Matrix menuView = Matrix.CreateLookAt(menuCameraPos, new Vector3(0f, 1.5f, 0f), Vector3.Up);
        Matrix menuProj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(50f), GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000f);

        _effect.Parameters["View"]?.SetValue(menuView);
        _effect.Parameters["Projection"]?.SetValue(menuProj);
        
        _effect.Parameters["lightPosition"]?.SetValue(new Vector3(10f, 25f, 15f));
        _effect.Parameters["eyePosition"]?.SetValue(menuCameraPos);

        // Matriz del auto girando
        Matrix menuCarWorld = Matrix.CreateScale(_vehiculoActual.modelI.Scale * 1.5f)
                            * Matrix.CreateRotationY(_menuCarYaw)
                            * Matrix.CreateTranslation(Vector3.Zero);

        // Le delegamos el dibujo al vehículo pasándole su matriz del menú
        _vehiculoActual.Draw(_effect, menuView, menuProj, menuCarWorld);

        // Dibujar interfaz de usuario en 2D
        _spriteBatch.Begin();
        string titulo = "SELECCIONA TU VEHICULO";
        string nombreVehiculo = $"< {_vehiculoActual.Tipo} >";
        string statsTexto = $"Velocidad/Acel: {_vehiculoActual.stats.accel} | Giro: {_vehiculoActual.stats.turnSpeed} | Tanque: {_vehiculoActual.stats.maxFuel}";
        string ayuda = "[FLECHAS] Cambiar Vehiculo    -    [ENTER] Empezar Carrera";

        _spriteBatch.DrawString(_font, titulo, new Vector2(50, 40), Color.Gold);
        _spriteBatch.DrawString(_font, nombreVehiculo, new Vector2(50, 80), Color.White);
        _spriteBatch.DrawString(_font, statsTexto, new Vector2(50, 120), Color.LightGreen);
        _spriteBatch.DrawString(_font, ayuda, new Vector2(50, GraphicsDevice.Viewport.Height - 60), Color.LightGray);
        _spriteBatch.End();
    }

    private void DrawGame(GameTime gameTime)
    {
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        
        GraphicsDevice.Clear(new Color(110, 160, 230)); // Cielo azul
        _effect.Parameters["View"].SetValue(_followCamera.View);
        _effect.Parameters["Projection"].SetValue(_followCamera.Projection);
        
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        //Dibujamos un piso
        DrawCustomFloor();

        _roadSpawner.Draw(_effect, _followCamera.View, _followCamera.Projection);
        GizmoPrimitives.DrawBoundingBox(GraphicsDevice, _effect, _vehiculoActual.hitbox.Min, _vehiculoActual.hitbox.Max, _vehiculoActual.getCarWorld(), _followCamera.View, _followCamera.Projection, Microsoft.Xna.Framework.Color.Blue);
        _vehiculoActual.Draw(_effect, _followCamera.View, _followCamera.Projection);


        //mandar a func en coll
        foreach (Collectible coll in _roadSpawner.collectibles)
        {
            if (coll.Collected) continue;
            coll.Draw(_effect, _followCamera.View, _followCamera.Projection, gameTime);
            
            var hitWorld = Matrix.CreateScale(coll.modelI.Scale) * Matrix.CreateTranslation(coll.pos);
            GizmoPrimitives.DrawBoundingBox(GraphicsDevice, _effect, coll.hitbox.Min, coll.hitbox.Max, hitWorld, _followCamera.View, _followCamera.Projection, Color.Blue);
        }
    }

    //Creamos geometria del piso 
    //(basicamente un cuadrado con 4 vertices, segun el tamaño que le pasemos, claramente van a ser dos triangulos grandes)
    private void CreateFloorGeometry(float size)
    {
        Vector3 up = Vector3.Up;
        _floorVertices = new VertexPositionNormalTexture[]
        {
            new(new Vector3(-size, -1.5f, -size), up, new Vector2(0f, 0f)),
            new(new Vector3(size, -1.5f, -size),  up, new Vector2(100f, 0f)),
            new(new Vector3(size, -1.5f, size),   up, new Vector2(100f, 100f)),
            new(new Vector3(-size, -1.5f, size),  up, new Vector2(0f, 100f))
        };
        _floorIndices = new short[] { 0, 1, 2, 0, 2, 3 };
    }

    //Metodo para que dibuje el piso
    private void DrawCustomFloor()
    {
        Matrix floorWorld = Matrix.CreateTranslation(new Vector3(0f, -0.2f, 0f));
        Matrix invTransposeFloor = Matrix.Transpose(Matrix.Invert(floorWorld));
        _effect.Parameters["World"]?.SetValue(floorWorld);
        _effect.Parameters["View"]?.SetValue(_followCamera.View);
        _effect.Parameters["Projection"]?.SetValue(_followCamera.Projection);
        _effect.Parameters["InverseTransposeWorld"]?.SetValue(invTransposeFloor);
        _effect.Parameters["DiffuseColor"]?.SetValue(new Vector3(0.13f, 0.53f, 0.10f)); // Verde pasto
        _effect.Parameters["baseTexture"]?.SetValue(DefaultTexture);

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                _floorVertices, 0, 4,
                _floorIndices, 0, 2
            );
        }
    }
   

    /// <summary>
    ///     Libero los recursos que se cargaron en el juego.
    /// </summary>
    protected override void UnloadContent()
    {
        // Libero los recursos.
        Content.Unload();

        base.UnloadContent();
    }
}