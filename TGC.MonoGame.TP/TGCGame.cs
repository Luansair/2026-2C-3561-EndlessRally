using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
    private Model _treeModel;

    private Matrix _projection;
    private float _rotation;
    private SpriteBatch _spriteBatch;
    private Matrix _view;
    private Matrix _world;
    private Vector3 cameraPos = new(-100f, 200f, -200f);

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
        _view = Matrix.CreateLookAt(cameraPos, Vector3.Zero, Vector3.Up);
        _projection =
            Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 1500);

        base.Initialize();
    }

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

        _tree = new Tree(_treeModel, Vector3.Zero, 0, 10);
        _forest = new Forest([new ModelInfo(_treeModel, 6)], new Vector3(0, 0, 200), 100, 25, new Random(SEED));

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

        // Capturar Input teclado
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            //Salgo del juego.
            Exit();
        }

        // Basado en el tiempo que paso se va generando una rotacion.
        //_rotation += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

        _world = Matrix.CreateRotationY(_rotation);

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

        // Para dibujar le modelo necesitamos pasarle informacion que el efecto esta esperando.
        _effect.Parameters["View"].SetValue(_view);
        _effect.Parameters["Projection"].SetValue(_projection);
        _random = new Random(SEED);
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        for(int i=1; i<=100; i++)
        {
            Vector3 nM = Vector3.Zero;
            nM.X += (float) i * 10;
            nM.Z += (float) i * 7;
            _world =  Matrix.CreateTranslation(nM);
            DrawModel(_model, _world, _random);
        }
        for(int i=1; i<=100; i++)
        {
            Vector3 nM = Vector3.Zero;
            nM.X += (float) i * -10;
            nM.Z += (float) i * 7;
            _world =  Matrix.CreateTranslation(nM);
            DrawModel(_model, _world, _random);
        }
        for(int i=1; i<=100; i++)
        {
            Vector3 nM = Vector3.Zero;
            nM.X += (float) i * 10;
            nM.Z += (float) i * -7;
            _world =  Matrix.CreateTranslation(nM);
            DrawModel(_model, _world, _random);
        }
        for(int i=1; i<=100; i++)
        {
            Vector3 nM = Vector3.Zero;
            nM.X += (float) i * -10;
            nM.Z += (float) i * -7;
            _world =  Matrix.CreateTranslation(nM);
            DrawModel(_model, _world, _random);
        }
        _tree.Draw(GraphicsDevice, _effect, _view, _projection);
        _forest.Draw(GraphicsDevice, _effect, _view, _projection);
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