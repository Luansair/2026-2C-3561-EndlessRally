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
    private Model _model;
    private Model _carModel;
    private Model _treeModel;
    private DecorationArea _area;
    private DecorationArea _farArea;
    private DecorationGroup _treesGroup;
    private DecorationGroup _rocksGroup;

    // Una camara
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
    private Vector3 cameraPos = new(-100f, 200f, -200f);

    private Random _random;
    private const int SEED = 0;


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
            Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 1500);
        
        //creo una camara para seguir a un auto
        _followCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio);
        _carWorld = Matrix.Identity;

        //crea el piso con un determinado tamaño (init)
        CreateFloorGeometry(50000f);

        base.Initialize();
    }

    RoadSpawner _roadSpawner;

    /// <summary>
    ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
    ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
    ///     que podemos pre calcular para nuestro juego.
    /// </summary>
    protected override void LoadContent()
    {
        // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Cargo el modelo del logo.
        _model = Content.Load<Model>(ContentFolder3D + "raceCarWhite");
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

        // Cargo un efecto basico propio declarado en el Content pipeline.
        // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
        _effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");

        // Asigno el efecto que cargue a cada parte del mesh.
        // Un modelo puede tener mas de 1 mesh internamente.
        foreach (var mesh in _model.Meshes)
        {
            // Un mesh puede tener mas de 1 mesh part (cada 1 puede tener su propio efecto).
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect = _effect;
            }
        }
        foreach (var mesh in _carModel.Meshes)
        {
            // Un mesh puede tener mas de 1 mesh part (cada 1 puede tener su propio efecto).
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect = _effect;
            }
        }

        _random = new Random(SEED);
        _tree = new Tree(_treeModel, Vector3.Zero, 0, 10);
        _forest = new Forest([new ModelInfo(_treeModel, 6)], new Vector3(0, 0, 200), 100, 25, _random);

        _treesGroup = new DecorationGroup(DecorationType.Tree, 3, [new ModelInfo(_treeModel, 6)]);
        _rocksGroup = new DecorationGroup(DecorationType.Rock, 1, [rockModel1, rockModel2, rockModel3, rockModel4, rockModel5, rockModel6, rockModel7, rockModel8, rockModel9, rockModel10]);

        _area = new DecorationArea(new RectangleShape(new Vector3(200, 0, 0), 300, 250), [_treesGroup, _rocksGroup], _random);
        _farArea = new DecorationArea(new RectangleShape(new Vector3(0, 0, 1600), 800, 500), [_treesGroup, _rocksGroup], _random);

        //se cargan los modelos
        var roadStraightModel = Content.Load<Model>(ContentFolder3D + "Kenny_races/roadStraight");
        var roadRampModel = Content.Load<Model>(ContentFolder3D + "Kenny_races/roadRamp");
        var roadCurvedSplitModel = Content.Load<Model>(ContentFolder3D + "Kenny_races/roadCurvedSplit");

        foreach (var model in new[] { roadStraightModel, roadRampModel, roadCurvedSplitModel })
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = _effect;
                }
            }
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
        _roadSpawner = new RoadSpawner(roadDefs, Vector3.Zero, 1000f, 1000f, decorationsFactory);
        

        base.LoadContent();
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

        //La logica debe ir aca
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
        _roadSpawner.Update(_carPosition);

        base.Update(gameTime);
    }

    /// <summary>
    ///     Se llama cada vez que hay que refrescar la pantalla.
    ///     Escribir aqui el codigo referido al renderizado.
    /// </summary>
    protected override void Draw(GameTime gameTime)
    {
        // Aca deberiamos poner toda la logia de renderizado del juego.
        GraphicsDevice.Clear(new Color(110, 160, 230)); // Cielo azul
        
        // Para dibujar le modelo necesitamos pasarle informacion que el efecto esta esperando.
        _effect.Parameters["View"].SetValue(_followCamera.View);
        _effect.Parameters["Projection"].SetValue(_followCamera.Projection);
        
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        //Dibujamos un piso
        DrawCustomFloor();
 
        /*
        _tree.Draw(GraphicsDevice, _effect, _followCamera.View, _followCamera.Projection);
        _forest.Draw(GraphicsDevice, _effect, _followCamera.View, _followCamera.Projection);
        _area.Draw(GraphicsDevice, _effect, _followCamera.View, _followCamera.Projection);
        _farArea.Draw(GraphicsDevice, _effect, _followCamera.View, _followCamera.Projection);
        */
        _roadSpawner.Draw(_effect, _followCamera.View, _followCamera.Projection);

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