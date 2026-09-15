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

    private Effect _effect;

    //private Model _model;
    private Model _carModel;
    private Model _treeModel;
    private DecorationArea _area;
    private DecorationArea _farArea;
    private DecorationGroup _treesGroup;
    private DecorationGroup _rocksGroup;


    // Geometría del piso (vertices e indices)
    private VertexPositionColor[] _floorVertices;
    private short[] _floorIndices;


    //Modelos partes del camino
    private Model roadStraightModel ;
    private Matrix[] roadStraightBones;
    private Model roadRampModel ;
    private Model roadCurvedModel ;
    private Matrix[] roadCurvedBones;
    private Model roadCornerLargeModel ;
    private Matrix[] roadCornerBones;

    //Ultima posicion del camino
    private Vector3 origin = new(0f,0.1f,0f);
    //Matriz de mundo de la pieza de camino actual
    private Matrix currentRoadPiece;


    // Entidades del escenario
    private struct TrackPieceInstance
    {
        public Model Model;
        public Matrix[] Bones;
        public Matrix World;
    }
    private readonly List<TrackPieceInstance> _trackPieces = new();

    // Dimensiones de pista
    private const float TrackScale = 10f;
    private const float BaseTileSize = 10f; 
    private const float TileSize = BaseTileSize * TrackScale;


    // Una camara que sigue al auto
    private FollowCamera _followCamera;
    // Posicion del auto a seguir
    private Vector3 _carPosition = new(0f,0f,0f);
    //Rotacion del auto
    private float carYaw = 0f;
    private float velocidad = 400f;


    //private Matrix _projection;
    private SpriteBatch _spriteBatch;
    //private Matrix _view;
    private Matrix _world;
    private Matrix _carWorld;
    //private Vector3 cameraPos = new(-100f, 200f, -200f);

    private Random _random;
    private const int SEED = 0;

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

        // Para que el juego sea pantalla completa se puede usar Graphics IsFullScreen.
        // Carpeta raiz donde va a estar toda la Media.
        Content.RootDirectory = "Content";
        // Hace que el mouse sea visible.
        IsMouseVisible = true;
    }

    /// <summary>
    ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
    ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
    /// </summary>
    protected override void Initialize()
    {
        // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.

        // Apago el backface culling.
        // Esto se hace por un problema en el diseno del modelo del logo de la materia.
        // Una vez que empiecen su juego, esto no es mas necesario y lo pueden sacar.
        //var rasterizerState = new RasterizerState();
        //rasterizerState.CullMode = CullMode.None;
        //GraphicsDevice.RasterizerState = rasterizerState;
        // Seria hasta aca.

        // Configuramos nuestras matrices de la escena.
        _world = Matrix.Identity;
        //_view = Matrix.CreateLookAt(cameraPos, Vector3.Zero, Vector3.Up);
        //_projection =
        //    Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 1500);
        _carWorld = Matrix.Identity;
        currentRoadPiece = Matrix.Identity;

        //creo una camara para seguir a un auto
        _followCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio);

        //crea el piso con un determinado tamaño
        CreateFloorGeometry(5000f);
        
        base.Initialize();
    }

    //RoadSpawner _roadSpawner;

    /// <summary>
    ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
    ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
    ///     que podemos pre calcular para nuestro juego.
    /// </summary>
    protected override void LoadContent()
    {
        // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Cargo los modelos.
        //_model = Content.Load<Model>(ContentFolder3D + "raceCarWhite");
        _treeModel = Content.Load<Model>(ContentFolder3D + "Tree/Tree");
        _carModel = Content.Load<Model>(ContentFolder3D + "raceCarWhite"); 
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

        //se cargan los modelos del camino
        roadStraightModel = Content.Load<Model>(ContentFolder3D + "Kenny_races/roadStraight");
        roadRampModel = Content.Load<Model>(ContentFolder3D + "Kenny_races/roadRamp");
        roadCurvedModel = Content.Load<Model>(ContentFolder3D + "Kenny_races/roadCurved");
        roadCornerLargeModel = Content.Load<Model>(ContentFolder3D + "Kenny_races/roadCornerLarge");

        // Cargo un efecto basico propio declarado en el Content pipeline.
        // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
        _effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");

        ApplyShaderToModel(_carModel, _effect);

        _random = new Random(SEED);
        _tree = new Tree(_treeModel, new Vector3(50f,0f,200f), 0, 10);
        _forest = new Forest([new ModelInfo(_treeModel, 6)], new Vector3(100f, 0f, 200f), 20, 25, _random);

        _treesGroup = new DecorationGroup(DecorationType.Tree, 350, [new ModelInfo(_treeModel, 6)]);
        _rocksGroup = new DecorationGroup(DecorationType.Rock, 100, [rockModel1, rockModel2, rockModel3, rockModel4, rockModel5, rockModel6, rockModel7, rockModel8, rockModel9, rockModel10]);
        _area = new DecorationArea(new RectangleShape(new Vector3(200, 0, 600), 150, 2000), [_treesGroup, _rocksGroup], _random);
        _farArea = new DecorationArea(new RectangleShape(new Vector3(-200, 0, 600), 150, 2000), [_treesGroup, _rocksGroup], _random);

        foreach (var model in new[] { roadStraightModel, roadRampModel, roadCurvedModel, roadCornerLargeModel })
        {
            ApplyShaderToModel(model, _effect);
        }
        //se define un dicc con Tipo de camino  y los modelo con sus datos (offset para la siguiente posisicion y si rota o no)
        var roadDefs = new Dictionary<RoadPieceType, RoadPiece>
        {
            { RoadPieceType.STRAIGHT, new RoadPiece(roadStraightModel, new Vector3(0, 0, 10), 0f) },
            { RoadPieceType.RAMP, new RoadPiece(roadRampModel, new Vector3(0, 0, 10), 0f) },
            { RoadPieceType.CURVEDSPLIT, new RoadPiece(roadCurvedModel, new Vector3(0, 0, 20), -MathHelper.PiOver2) },
            { RoadPieceType.CURVEDSPLITLEFT, new RoadPiece(roadCurvedModel, new Vector3(0, 0, 20), MathHelper.PiOver2) },
            { RoadPieceType.CORNERLARGE, new RoadPiece(roadCornerLargeModel, new Vector3(0, 0, 20), MathHelper.PiOver2) }
        };
        //se instancia con el diccionario, el inicio y la distancia de espawn y de "culling"
        //_roadSpawner = new RoadSpawner(roadDefs, Vector3.Zero, 600f, 100f);

        roadStraightBones = new Matrix[roadStraightModel.Bones.Count];
        roadStraightModel.CopyAbsoluteBoneTransformsTo(roadStraightBones);
        ApplyShaderToModel(roadStraightModel, _effect);

        roadCornerBones = new Matrix[roadCornerLargeModel.Bones.Count];
        roadCornerLargeModel.CopyAbsoluteBoneTransformsTo(roadCornerBones);
        ApplyShaderToModel(roadCornerLargeModel, _effect);

        roadCurvedBones = new Matrix[roadCurvedModel.Bones.Count];
        roadCurvedModel.CopyAbsoluteBoneTransformsTo(roadCurvedBones);
        ApplyShaderToModel(roadCurvedModel, _effect);

        //Ubico el auto centrado en la recta del comienzo
        _carPosition = new Vector3(-10f, 0f, 50f);
        carYaw = MathHelper.Pi;

        //Armamos el circuito (las posiciones de cada parte)
        BuildCircuit();

        base.LoadContent();
    }


    //esto es para no repetir codigo, aplica el efecto a cada parte de cada mesh
    private void ApplyShaderToModel(Model model, Effect effect)
    {
        foreach (var mesh in model.Meshes)
        {
            foreach (var part in mesh.MeshParts)
            {
                part.Effect = effect;
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
        if (keyboardState.IsKeyDown(Keys.A))
        {
            // Roto el auto hacia la izquierda
            carYaw += MathHelper.ToRadians(100f) * elapsedTime;
        }
        if (keyboardState.IsKeyDown(Keys.D))
        {
            // Roto el auto hacia la derecha
            carYaw -= MathHelper.ToRadians(100f) * elapsedTime;
        }

        //obtengo la direccion del auto
        Vector3 direccion = _carWorld.Forward;

        if (keyboardState.IsKeyDown(Keys.W))
        {
            // Muevo el auto hacia adelante
            _carPosition += direccion * velocidad * elapsedTime;
        }
        if (keyboardState.IsKeyDown(Keys.S))
        {
            // Muevo el auto hacia atras
            _carPosition -= direccion * velocidad * elapsedTime;
        }

        //Actualizo la matriz de mundo del auto con la rotacion respecto al eje Y 
        // y con el vector3 de posicion, siguiendo la regla de SRT
        _carWorld = Matrix.CreateRotationY(carYaw) * Matrix.CreateTranslation(_carPosition);

        // Actualizo la camara, enviandole la matriz de mundo del auto.
        _followCamera.Update(gameTime, _carWorld);
        //_roadSpawner.Update(_carPosition);

        base.Update(gameTime);
    }

    /// <summary>
    ///     Se llama cada vez que hay que refrescar la pantalla.
    ///     Escribir aqui el codigo referido al renderizado.
    /// </summary>
    protected override void Draw(GameTime gameTime)
    {
        // Aca deberiamos poner toda la logia de renderizado del juego.
        GraphicsDevice.Clear(Color.Black);
        GraphicsDevice.Clear(new Color(110, 160, 230)); // Cielo azul
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        // Configuramos las matrices de vista y proyeccion en el efecto
        _effect.Parameters["View"].SetValue(_followCamera.View);
        _effect.Parameters["Projection"].SetValue(_followCamera.Projection);

        //Dibujamos un piso
        DrawCustomFloor();

        //Dibujamos el circuito por cada parte
        _effect.Parameters["DiffuseColor"]?.SetValue(Color.SandyBrown.ToVector3());
        foreach (var piece in _trackPieces)
        {
            foreach (var mesh in piece.Model.Meshes)
            {
                _effect.Parameters["World"]?.SetValue(piece.Bones[mesh.ParentBone.Index] * piece.World);
                mesh.Draw();
            }
        }

        _tree.Draw(GraphicsDevice, _effect, _followCamera.View, _followCamera.Projection);
        _forest.Draw(GraphicsDevice, _effect, _followCamera.View, _followCamera.Projection);
        _area.Draw(GraphicsDevice, _effect, _followCamera.View, _followCamera.Projection);
        _farArea.Draw(GraphicsDevice, _effect, _followCamera.View, _followCamera.Projection);
        _effect.Parameters["DiffuseColor"].SetValue(Color.SandyBrown.ToVector3());
        //_roadSpawner.Draw(_effect, _followCamera.View, _followCamera.Projection);

        //Dibujo el auto a seguir
        foreach (var mesh in _carModel.Meshes)
        {
            _effect.Parameters["DiffuseColor"].SetValue(Color.White.ToVector3());
            foreach (var part in mesh.MeshParts)
            {
                // Pasamos las matrices al efecto de esta parte específica
                part.Effect.Parameters["World"]?.SetValue(mesh.ParentBone.Transform * _carWorld);
                part.Effect.Parameters["View"]?.SetValue(_followCamera.View);
                part.Effect.Parameters["Projection"]?.SetValue(_followCamera.Projection);
            }

            mesh.Draw();
        }
    }

    //Creamos geometria del piso 
    //(basicamente un cuadrado con 4 vertices, segun el tamaño que le pasemos, claramente van a ser dos triangulos grandes)
    private void CreateFloorGeometry(float size)
    {
        Color grassColor = new(34, 110, 34);

        _floorVertices = new VertexPositionColor[]
        {
            new(new Vector3(-size, -0.2f, -size), grassColor),
            new(new Vector3(size, -0.2f, -size), grassColor),
            new(new Vector3(size, -0.2f, size), grassColor),
            new(new Vector3(-size, -0.2f, size), grassColor)
        };

        _floorIndices = new short[] { 0, 1, 2, 0, 2, 3 };
    }

    //Metodo para que dibuje el piso
    private void DrawCustomFloor()
    {
        _effect.Parameters["World"]?.SetValue(Matrix.Identity);
        _effect.Parameters["DiffuseColor"]?.SetValue(new Vector3(0.18f, 0.42f, 0.16f)); // Verde pasto

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

    //Funciones para dibujar con distintos colores las partes de los modelos
    /*
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
    */
    
    //Metodo para dibujar el circuito
    private void BuildCircuit()
    {
        _trackPieces.Clear();

        //int width = 8;  // Eje X
        int length = 30; // Eje Z

        //una recta de 30 partes
        for (int x = 0; x < length ; x++)
        {
            AddPiece(roadStraightModel, roadStraightBones, 0f, (float) x, 0, origin); // Tramo Norte
        }

        AddPiece(roadCornerLargeModel, roadCornerBones, 0f, 30f, 0, origin);
        AddPiece(roadCornerLargeModel, roadCornerBones, -1f, 32.3f, 3, origin);
        AddPiece(roadStraightModel, roadStraightBones, -3f, 29f, 0, origin);
        AddPiece(roadStraightModel, roadStraightBones, -3f, 28f, 0, origin);
        AddPiece(roadStraightModel, roadStraightBones, -3f, 27f, 0, origin);
        AddPiece(roadCornerLargeModel, roadCornerBones, -5.3f, 26f, 1, origin);
        AddPiece(roadStraightModel, roadStraightBones, -6.3f, 26f, 1, origin);
        AddPiece(roadStraightModel, roadStraightBones, -7.3f, 26f, 1, origin);
        AddPiece(roadStraightModel, roadStraightBones, -8.3f, 26f, 1, origin);
        AddPiece(roadStraightModel, roadStraightBones, -9.3f, 26f, 1, origin);
        AddPiece(roadCornerLargeModel, roadCornerBones, -8f, 26.3f, 3, origin);

        
    }

    private void AddPiece(Model model, Matrix[] bones, float gridX, float gridZ, int rotationSteps, Vector3 origin)
    {
        Vector3 cellPos = origin + new Vector3(gridX * TileSize, 0f, gridZ * TileSize);
        float angleY = rotationSteps * MathHelper.PiOver2;

        Matrix world = Matrix.CreateScale(TrackScale) *
                       Matrix.CreateRotationY(angleY) *
                       Matrix.CreateTranslation(cellPos);

        _trackPieces.Add(new TrackPieceInstance
        {
            Model = model,
            Bones = bones,
            World = world
        });
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