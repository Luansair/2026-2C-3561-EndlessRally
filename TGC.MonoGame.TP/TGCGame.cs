using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    private Effect _effect;
    private Model _model;
    private Model _carModel;
    private Model _treeModel;
    private DecorationArea _area;
    private DecorationArea _farArea;
    private DecorationGroup _treesGroup;
    private DecorationGroup _rocksGroup;

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

    // Geometría del piso (vertices e indices)
    private VertexPositionColor[] _floorVertices;
    private short[] _floorIndices;

    private Tree _tree;
    private Forest _forest;

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

    //esto tiene que ir en el roadSpawner despues
    List<Collectible> collectibles = new List<Collectible>();
    
    /// <summary>
    ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
    ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
    ///     que podemos pre calcular para nuestro juego.
    /// </summary>
    
    protected override void LoadContent()
    {
        // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "DefaultFont");

        _effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");

        _treeModel = Content.Load<Model>(ContentFolder3D + "Tree/Tree");

        var ligeroModel = new ModelInfo(Content.Load<Model>(ContentFolder3D + "raceCarWhite"), 0.23f);
        var medianoModel = new ModelInfo(Content.Load<Model>(ContentFolder3D + "Kenney_Cars/hatchback-sportsV2"), 0.01f);
        var pesadoModel = new ModelInfo(Content.Load<Model>(ContentFolder3D + "Kenney_Cars/suvV2"), 0.01f);

        ApplyShaderToModel(ligeroModel.Model, _effect);
        ApplyShaderToModel(medianoModel.Model, _effect);
        ApplyShaderToModel(pesadoModel.Model, _effect);
        
        Vector3 largada = new Vector3(0f, 0f, 0f);

        _opcionesVehiculos.Add(new Vehiculo(TipoVehiculo.LIGERO, ligeroModel, largada, MathHelper.Pi));
        _opcionesVehiculos.Add(new Vehiculo(TipoVehiculo.MEDIANO, medianoModel, largada, MathHelper.Pi));
        _opcionesVehiculos.Add(new Vehiculo(TipoVehiculo.PESADO, pesadoModel, largada, MathHelper.Pi));

        _vehiculoActual = _opcionesVehiculos[_indiceVehiculoSeleccionado];

        var rockModel = new ModelInfo(Content.Load<Model>(ContentFolder3D + "Stones/Rock0"), 0.01f);
        var rockModel1 = new ModelInfo(Content.Load<Model>(ContentFolder3D + "Stones/Rock1"), 0.01f);
        var rockModel2 = new ModelInfo(Content.Load<Model>(ContentFolder3D + "Stones/Rock2"), 0.01f);
        var rockModel3 = new ModelInfo(Content.Load<Model>(ContentFolder3D + "Stones/Rock3"), 0.01f);
        var rockModel4 = new ModelInfo(Content.Load<Model>(ContentFolder3D + "Stones/Rock4"), 0.01f);
        var rockModel5 = new ModelInfo(Content.Load<Model>(ContentFolder3D + "Stones/Rock5"), 0.01f);
        var rockModel6 = new ModelInfo(Content.Load<Model>(ContentFolder3D + "Stones/Rock6"), 0.01f);
        var rockModel7 = new ModelInfo(Content.Load<Model>(ContentFolder3D + "Stones/Rock7"), 0.01f);
        var rockModel8 = new ModelInfo(Content.Load<Model>(ContentFolder3D + "Stones/Rock8"), 0.01f);
        var rockModel9 = new ModelInfo(Content.Load<Model>(ContentFolder3D + "Stones/Rock9"), 0.01f);
        var rockModel10 = new ModelInfo(Content.Load<Model>(ContentFolder3D + "Stones/Rock10"), 0.01f);
        
        _random = new Random(SEED);
        _tree = new Tree(_treeModel, Vector3.Zero, 0, 10);
        _forest = new Forest([new ModelInfo(_treeModel, 6)], new Vector3(0, 0, 200), 100, 25, _random);

        _treesGroup = new DecorationGroup(DecorationType.Tree, 3, [new ModelInfo(_treeModel, 6)]);
        _rocksGroup = new DecorationGroup(DecorationType.Rock, 1, [rockModel1, rockModel2, rockModel3, rockModel4, rockModel5, rockModel6, rockModel7, rockModel8, rockModel9, rockModel10]);

        _area = new DecorationArea(new RectangleShape(new Vector3(200, 0, 0), 300, 250), [_treesGroup, _rocksGroup], _random);
        _farArea = new DecorationArea(new RectangleShape(new Vector3(0, 0, 1600), 3600, 7200), [_treesGroup, _rocksGroup], _random);

        //se cargan los modelos
        var roadStraightModel = Content.Load<Model>(ContentFolder3D + "Kenny_races/roadStraight");
        var roadRampModel = Content.Load<Model>(ContentFolder3D + "Kenny_races/roadRamp");
        var roadCurvedSplitModel = Content.Load<Model>(ContentFolder3D + "Kenny_races/roadCurvedSplit");

        foreach (var model in new[] { roadStraightModel, roadRampModel, roadCurvedSplitModel })
        {
            ApplyShaderToModel(model, _effect);
        }
        //se define un dicc con Tipo de camino  y los modelo con sus datos (offset para la siguiente posisicion y si rota o no)
        var roadDefs = new Dictionary<RoadPieceType, RoadPiece>
        {
            { RoadPieceType.STRAIGHT, new RoadPiece(roadStraightModel, new Vector3(0, 0, 10), 0f) },
            { RoadPieceType.RAMP, new RoadPiece(roadRampModel, new Vector3(0, 0, 10), 0f) },
            { RoadPieceType.CURVEDSPLIT, new RoadPiece(roadCurvedSplitModel, new Vector3(0, 0, 20), -MathHelper.PiOver2) },
            { RoadPieceType.CURVEDSPLITLEFT, new RoadPiece(roadCurvedSplitModel, new Vector3(0, 0, 20), MathHelper.PiOver2) }
        };

        var decorationsFactory = new DecorationAreaFactory([_treesGroup, _rocksGroup]);

        //se instancia con el diccionario, el inicio y la distancia de espawn y de "culling"
        _roadSpawner = new RoadSpawner(roadDefs, Vector3.Zero, 3600f, 7200f, decorationsFactory);
        
        var collectibleModel = new ModelInfo(Content.Load<Model>(ContentFolder3D + "Coleccionables/Sphere"), 0.05f);
        foreach (var mesh in collectibleModel.Model.Meshes)
        {
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect = _effect;
            }
        }
        //despues hay que pasarlo al roadspawnder
        collectibles.Add(new FichaCollectible(collectibleModel, new Vector3(0f, 0f, -150f), 10));
        collectibles.Add(new FichaCollectible(collectibleModel, new Vector3(50f, 0f, -150f), 10));
        collectibles.Add(new FichaCollectible(collectibleModel, new Vector3(-50f, 0f, -150f), 10));

        base.LoadContent();
    }

    protected void ApplyShaderToModel(Model model, Effect effect)
    {
        foreach (var mesh in model.Meshes)
        {
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect = effect;
            }
        }
    }

    /// <summary>
    ///     Se llama en cada frame.
    ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
    ///     ante ellas.
    /// </summary>
    protected override void Update(GameTime gameTime)
    {
        // Aca deberiamos poner toda la logica de actualizacion del juego.
        float elapsedTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
        // Capturo el estado del teclado.
        var keyboardState = Keyboard.GetState();
        
        // Capturar Input teclado
        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            //Salgo del juego.
            Exit();
        }


        if (!_enCarrera)
        {
            // Cambiar de _vehiculoActual con flechas
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
            // Todo el movimiento se delega a la clase
            _vehiculoActual.Update(gameTime, keyboardState);
            _followCamera.Update(gameTime, _vehiculoActual.getCarWorld());
            checkCollisions();
        }

        _prevKeyboard = keyboardState;


        //_vehiculoActual.Update(gameTime, keyboardState, _carWorld.Forward);

        this.Window.Title = "Score: " + _vehiculoActual.score;
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
        foreach (Collectible coll in collectibles)
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
        // Fondo oscuro estilo concesionaria / garaje
        GraphicsDevice.Clear(new Color(24, 26, 32));

        // Cámara fija del menú mirando al centro (0, 0, 0)
        Matrix menuView = Matrix.CreateLookAt(new Vector3(0f, 6f, 18f), new Vector3(0f, 1.5f, 0f), Vector3.Up);
        Matrix menuProj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(50f), GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000f);

        _effect.Parameters["View"]?.SetValue(menuView);
        _effect.Parameters["Projection"]?.SetValue(menuProj);

        // Dibujamos el _vehiculoActual seleccionado girando en el centro
        Matrix menuCarWorld = Matrix.CreateScale(_vehiculoActual.modelI.Scale * 1.5f)
                            * Matrix.CreateRotationY(_menuCarYaw)
                            * Matrix.CreateTranslation(Vector3.Zero);

        _effect.Parameters["DiffuseColor"]?.SetValue(Color.White.ToVector3());

        // Obtenemos los huesos del modelo seleccionado
        var bones = new Matrix[_vehiculoActual.modelI.Model.Bones.Count];
        _vehiculoActual.modelI.Model.CopyAbsoluteBoneTransformsTo(bones);

        foreach (var mesh in _vehiculoActual.modelI.Model.Meshes)
        {
            _effect.Parameters["World"]?.SetValue(bones[mesh.ParentBone.Index] * menuCarWorld);
            mesh.Draw();
        }

        // Dibujar interfaz de usuario en 2D
        _spriteBatch.Begin();

        string titulo = "SELECCIONA TU VEHICULO";
        string nombreVehiculo = $"< {_vehiculoActual.Tipo} >";
        string statsTexto = $"Velocidad/Acel: {_vehiculoActual.stats.accel} | Giro: {_vehiculoActual.stats.turnSpeed} | Tanque: {_vehiculoActual.stats.maxFuel}";
        string ayuda = "[FLECHAS] Cambiar _vehiculoActual    -    [ENTER] Empezar Carrera";

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
        foreach (Collectible coll in collectibles)
        {
            if (coll.Collected) continue;
            coll.Draw(_effect, _followCamera.View, _followCamera.Projection, gameTime);
            var hitWorld = Matrix.CreateScale(coll.modelI.Scale) * Matrix.CreateTranslation(coll.pos);
            GizmoPrimitives.DrawBoundingBox(GraphicsDevice, _effect, coll.hitbox.Min, coll.hitbox.Max, hitWorld, _followCamera.View, _followCamera.Projection, Microsoft.Xna.Framework.Color.Blue);

        }
    }

    private void DrawModel(Model model, Matrix world, Random random)
    {
        var modelMeshesBaseTransforms = new Matrix[model.Bones.Count];
        model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
        foreach (var mesh in model.Meshes)
        {
            var relativeTransform = modelMeshesBaseTransforms[mesh.ParentBone.Index];
            _effect.Parameters["World"].SetValue(relativeTransform * world);
            _effect.Parameters["DiffuseColor"].SetValue(RandomColor(_random).ToVector3());
            mesh.Draw();
        }
    }

    private Color RandomColor(Random random)
    {
        // Construye un color aleatorio en base a un entero de 32 bits
        return new Color((uint)random.Next());
    }

    //Creamos geometria del piso 
    //(basicamente un cuadrado con 4 vertices, segun el tamaño que le pasemos, claramente van a ser dos triangulos grandes)
    private void CreateFloorGeometry(float size)
    {
        Color grassColor = new(34, 110, 34);
        _floorVertices = new VertexPositionColor[]
        {
            new(new Vector3(-size, -1.5f, -size), grassColor),
            new(new Vector3(size, -1.5f, -size), grassColor),
            new(new Vector3(size, -1.5f, size), grassColor),
            new(new Vector3(-size, -1.5f, size), grassColor)
        };
        _floorIndices = new short[] { 0, 1, 2, 0, 2, 3 };
    }

    //Metodo para que dibuje el piso
    private void DrawCustomFloor()
    {
        _effect.Parameters["World"]?.SetValue(Matrix.Identity);
        _effect.Parameters["DiffuseColor"]?.SetValue(new Vector3(0.13f, 0.53f, 0.10f)); // Verde pasto

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