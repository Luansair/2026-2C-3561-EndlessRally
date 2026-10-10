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

    private Entity _currentVehicle;
    private PlayerController _player;
    private World _world;
    private VehicleCarrousel _vehicles;
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
        // Carga de cosas del menu supongo
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "DefaultFont");

        DefaultTexture = new Texture2D(GraphicsDevice, 1, 1);
        DefaultTexture.SetData(new[] { Color.White });
        
        _effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
        LightManager.ApplyDefaults(_effect);

        // Carga de assets
        var contentLoader = new GameContentLoader();
        _assets = contentLoader.Load(Content, GraphicsDevice);

        // Carga de opciones de vehiculos
        var vehiclePresets = new VehiclePresets(_assets.Cars);
        _vehicles = new VehicleCarrousel();
        _vehicles.Add(vehiclePresets.Create(VehicleType.LightCar));
        _vehicles.Add(vehiclePresets.Create(VehicleType.MediumCar));
        _vehicles.Add(vehiclePresets.Create(VehicleType.HeavyCar));

        _currentVehicle = _vehicles.GetCurrent();

        // Configuracion de factories para crear entidades
        _random = new Random(SEED);

        var treesGroup = new DecorationGroup(DecorationType.Tree, 3, [_assets.Decorations.Tree]);
        var rocksGroup = new DecorationGroup(DecorationType.Rock, 1, _assets.Decorations.Rocks);
        var houseGroup = new DecorationGroup(DecorationType.House, 1, [_assets.Decorations.House]);

        var natureRecipes = new List<DecorationGroup> { treesGroup, rocksGroup };
        var houseRecipes = new List<DecorationGroup> { houseGroup };

        var decorationsFactory = new DecorationAreaFactory(natureRecipes, houseRecipes);
        var collectibleFactory = new CollectibleFactory(_assets.Collectibles);
        var roadSpawnerFactory = new RoadSpawnerFactory(_assets.Roads, decorationsFactory, collectibleFactory);

        // ??
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

        // Creacion de mundo y road spawner
        _world = new World();
        var spawnerConfig = new RoadSpawnerConfig
        {
            StartPosition = new Vector3(0f, 0.05f, 0f),
            SpawnDistance = 800f,
            DespawnDistance = 1600f,
            InitialSegmentCount = 100
        };

        _roadSpawner = roadSpawnerFactory.Create(spawnerConfig, _world);

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
        // Menu de seleccion
        if (!_enCarrera)
        {
            _menuCarYaw += elapsedTime * 1.5f;
            // Cambiar de auto con flechas
            if (keyboardState.IsKeyDown(Keys.Right) && _prevKeyboard.IsKeyUp(Keys.Right))
            {
                _currentVehicle = _vehicles.GetNext();
            }
            if (keyboardState.IsKeyDown(Keys.Left) && _prevKeyboard.IsKeyUp(Keys.Left))
            {
                _currentVehicle = _vehicles.GetPrev();
            }

            // comenzar
            if (keyboardState.IsKeyDown(Keys.Enter) && _prevKeyboard.IsKeyUp(Keys.Enter))
            {
                _enCarrera = true;
                _estadoActual = GameState.Playing;
                _followCamera.Update(gameTime, _currentVehicle.Transform.World);

                _player = new PlayerController(_currentVehicle);
                _world.Add(_currentVehicle);
            }
        }
        else
        {
            Vector3 lightOffset = new Vector3(400f, 500f, 400f);
            Vector3 lightPosition = _player.Vehicle.Transform.Position + lightOffset;
            _effect.Parameters["lightPosition"]?.SetValue(lightPosition);
            _effect.Parameters["eyePosition"]?.SetValue(_followCamera.Position);

            // Todo el movimiento se delega a la clase
            _player.Update(gameTime, keyboardState);
            _followCamera.Update(gameTime, _player.Vehicle.Transform.World);
        }

        _prevKeyboard = keyboardState;
        
        this.Window.Title = "Score: " + _player.Score + " Fuel: " + _player.Vehicle.Fuel.Current + " Health: " + _player.Vehicle.Damageable.Current;

        _roadSpawner.Update(_player.Vehicle.Transform.Position);


        base.Update(gameTime);
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
        Matrix menuCarWorld = Matrix.CreateScale(_currentVehicle.Render.ModelInfo.Scale * 1.5f)
                            * Matrix.CreateRotationY(_menuCarYaw)
                            * Matrix.CreateTranslation(Vector3.Zero);

        // Le delegamos el dibujo al vehículo pasándole su matriz del menú
        _currentVehicle.Render.Draw(_effect, menuView, menuProj, menuCarWorld);

        // Dibujar interfaz de usuario en 2D
        _spriteBatch.Begin();
        string titulo = "SELECCIONA TU VEHICULO";
        //string nombreVehiculo = $"< {_vehiculoActual.Tipo} >";
        string statsTexto = $"Velocidad/Acel: {_currentVehicle.VehicleStats.Acceleration} | Giro: {_currentVehicle.VehicleStats.TurnSpeed} | Tanque: {_currentVehicle.Fuel.Max}";
        string ayuda = "[FLECHAS] Cambiar Vehiculo    -    [ENTER] Empezar Carrera";

        _spriteBatch.DrawString(_font, titulo, new Vector2(50, 40), Color.Gold);
        //_spriteBatch.DrawString(_font, nombreVehiculo, new Vector2(50, 80), Color.White);
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

        _world.Draw(_effect, _followCamera.View, _followCamera.Projection);  // Esto ya dibuja al auto tambien!
 
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