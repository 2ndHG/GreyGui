using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreyGui;

public class GuiBatch
{
    private const int DEFAULT_VERTEX_COUNT = 2048;
    private const int DEFAULT_INDEX_COUNT = 4096;
    private const int MAX_VERTEX_COUNT = 32768; // Limited by 16-bit index buffer
    private const int MAX_INDEX_COUNT = 65536;
    private readonly GraphicsDevice _device;
    private readonly Effect _uiShader;
    private static readonly RasterizerState ScissorState = new() { ScissorTestEnable = true };
    private GameTime _gameTime;

    private DynamicVertexBuffer _vertexBuffer;
    private int _vertexBufferSize = DEFAULT_VERTEX_COUNT;
    private DynamicIndexBuffer _indexBuffer;
    private int _indexBufferSize = DEFAULT_INDEX_COUNT;

    /// <summary>
    /// Create a GuiBatch instance that will be using a custom shader.
    /// </summary>
    /// <param name="device">GraphicDevice of the running Game</param>
    /// <param name="uiShader">Custom shader</param>
    // public GuiBatch(GraphicsDevice device, Effect uiShader)
    // {
    //     _device = device;
    //     _uiShader = uiShader;
    //     _gameTime = new GameTime();
    // }
    /// <summary>
    /// Create a GuiBatch instance.
    /// </summary>
    /// <param name="device">GraphicDevice of the running Game</param>
    /// <param name="uiShader">Custom shader</param>
    public GuiBatch(GraphicsDevice device)
    {
        _device = device;
        _uiShader = GreyGui.Shader;
        _gameTime = new GameTime();

        _vertexBuffer = new(device, UiVertex.VertexDeclaration, DEFAULT_VERTEX_COUNT, BufferUsage.WriteOnly);
        _indexBuffer = new DynamicIndexBuffer(device, IndexElementSize.SixteenBits, DEFAULT_INDEX_COUNT, BufferUsage.WriteOnly);
    }

    /// <summary>
    /// Receive the GameTime information of the drawing frame.
    /// </summary>
    /// <remarks>The elapsed time of GameTime will be extracted and inject into RenderContext</remarks>
    /// <param name="gameTime">GameTime instance, typically from Game.Draw</param>
    public void ReceiveFrameInfo(GameTime gameTime)
    {
        _gameTime = gameTime;
    }

    /// <summary>
    /// Collect drawing information from a GUI tree of a given GreyGuiElement root.
    /// </summary>
    /// <remarks>
    /// Like SpriteBatch.Draw, this method does NOT immediately draw to the current render target, instead, it add draw commands into the RenderContext.<br/> Use GuiBatch.Flush() to actually render the pixels to the render target.
    /// </remarks>
    /// <param name="root">The GreyGuiElement and its children you want to draw</param>
    /// <param name="context">Drawing RenderContext</param>
    /// <param name="position">Drawing position</param>
    public void Draw(GreyGuiElement root, RenderContext context, Point position)
    {
        context.ElapsedTimeSecond = _gameTime.ElapsedGameTime.TotalSeconds;
        Rectangle screenScissor = _device.Viewport.Bounds;
        root.ResolveSizeDirty();
        root.Draw(position, context, screenScissor);
        if (root is IContainer container)
        {
            container.DrawChildren(position, context, screenScissor);
        }
    }

    /// <summary>
    /// Consume accumulated draw commands of a RenderContext and render to the screen 
    /// </summary>
    /// <param name="context"></param>
    public void Flush(RenderContext context)
    {
        // Check if there are generated SDF bitmaps that haven't be drawn to the atlas
        GreyGui.TextSystem.SetGeneratedSdfBitmapToAtlas();

        if (context.Batches.Count == 0)
        {
            return;
        }

        EnsureBufferCapacity(context.VertexCount, context.IndexCount);
        _vertexBuffer.SetData(context.Vertices, 0, context.VertexCount, SetDataOptions.Discard);
        _indexBuffer.SetData(context.Indices, 0, context.IndexCount, SetDataOptions.Discard);
        _device.SetVertexBuffer(_vertexBuffer);
        _device.Indices = _indexBuffer;

        Matrix projection = Matrix.CreateOrthographicOffCenter(0, _device.Viewport.Width, _device.Viewport.Height, 0, 0, 1);

        _device.BlendState = BlendState.NonPremultiplied;
        _device.RasterizerState = ScissorState;

        _uiShader.Parameters["WorldViewProjection"].SetValue(projection);
        foreach (DrawBatch batch in context.Batches)
        {
            if (batch.IndexCount == 0) continue;

            _device.SamplerStates[0] = batch.Texture == GreyGui.Atlas ? SamplerState.LinearClamp : SamplerState.PointClamp;
            _device.ScissorRectangle = batch.Scissor;
            _uiShader.Parameters["Texture"].SetValue(batch.Texture);

            foreach (EffectPass pass in _uiShader.CurrentTechnique.Passes)
            {
                pass.Apply();
                _device.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    batch.IndexOffset,
                    batch.IndexCount / 3
                );
            }
        }
        context.Clear();
    }

    private void EnsureBufferCapacity(int vertexCount, int indexCount)
    {
        if (vertexCount > _vertexBufferSize)
        {
            int newSize = _vertexBufferSize;
            while (newSize < vertexCount)
            {
                newSize *= 2;
            }
            _vertexBuffer?.Dispose();
            _vertexBuffer = new DynamicVertexBuffer(_device, UiVertex.VertexDeclaration, newSize, BufferUsage.WriteOnly);
            _vertexBufferSize = newSize;
            Console.WriteLine($"Resized vertex buffer to {newSize}");
        }
        if (indexCount > _indexBufferSize)
        {

            int newSize = _indexBufferSize;
            while (newSize < indexCount)
            {
                newSize *= 2;
            }
            _indexBuffer?.Dispose();
            _indexBuffer = new DynamicIndexBuffer(_device, IndexElementSize.SixteenBits, newSize, BufferUsage.WriteOnly);
            _indexBufferSize = newSize;
            Console.WriteLine($"Resized index buffer to {newSize}");
        }
    }
}