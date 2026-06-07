using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreyGui;

public class GuiBatch
{
    public RenderContext RenderContext => _renderContext;
    private const int MAX_VERTEX_COUNT = 32768; // Limited by 16-bit index buffer
    private const int MAX_INDEX_COUNT = 65536;
    private readonly GraphicsDevice _device;
    private readonly Effect _uiShader;
    private static readonly RasterizerState ScissorState = new() { ScissorTestEnable = true };


    private RenderContext _renderContext;

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
    public GuiBatch()
    {
        _device = GreyGuiCore.GameInstance.GraphicsDevice;
        _uiShader = GreyGuiCore.Shader;

        _renderContext = new RenderContext();
    }

    /// <summary>
    /// Receive the GameTime information of the drawing frame.
    /// </summary>
    /// <remarks>The elapsed time of GameTime will be extracted and inject into RenderContext</remarks>
    /// <param name="gameTime">GameTime instance, typically from Game.Draw</param>
    public void ReceiveFrameInfo(GameTime gameTime)
    {
        _renderContext.ElapsedTimeSecond = gameTime.ElapsedGameTime.TotalSeconds;
    }

    /// <summary>
    /// Collect drawing information from a GUI tree of a given GreyGuiElement root.
    /// </summary>
    /// <remarks>
    /// Like SpriteBatch.Draw, this method does NOT immediately draw to the current render target, instead, it add draw commands into the RenderContext.<br/> Use GuiBatch.Flush() to actually render the pixels to the render target.
    /// </remarks>
    /// <param name="root">The GreyGuiElement and its children you want to draw</param>
    /// <param name="position">Drawing position</param>
    public void Draw(GreyGuiElement root, Point position)
    {
        Rectangle screenScissor = _device.Viewport.Bounds;
        root.ResolveSizeDirty();
        root.Draw(position, _renderContext, screenScissor);
        if (root is IContainer container)
        {
            container.DrawChildren(position, _renderContext, screenScissor);
        }
    }

    /// <summary>
    /// Consume accumulated draw commands of a RenderContext and render to the screen 
    /// </summary>
    public void Flush()
    {
        // Check if there are generated SDF bitmaps that haven't be drawn to the atlas
        GreyGuiCore.TextSystem.SetGeneratedSdfBitmapToAtlas();

        if (_renderContext.Batches.Count == 0 || _renderContext.VertexCount == 0 || _renderContext.IndexCount == 0)
        {
            _renderContext.Clear();
            return;
        }

        GreyGuiCore.EnsureGpuBufferCapacity(_renderContext.VertexCount, _renderContext.IndexCount);
        GreyGuiCore.VertexBuffer.SetData(_renderContext.Vertices, 0, _renderContext.VertexCount, SetDataOptions.Discard);
        GreyGuiCore.IndexBuffer.SetData(_renderContext.Indices, 0, _renderContext.IndexCount, SetDataOptions.Discard);
        _device.SetVertexBuffer(GreyGuiCore.VertexBuffer);
        _device.Indices = GreyGuiCore.IndexBuffer;

        Matrix projection = Matrix.CreateOrthographicOffCenter(0, _device.Viewport.Width, _device.Viewport.Height, 0, 0, 1);

        _device.BlendState = BlendState.NonPremultiplied;
        _device.RasterizerState = ScissorState;

        _uiShader.Parameters["WorldViewProjection"].SetValue(projection);
        foreach (DrawBatch batch in _renderContext.Batches)
        {
            if (batch.IndexCount == 0) continue;

            _device.SamplerStates[0] = batch.Texture == GreyGuiCore.Atlas ? SamplerState.LinearClamp : SamplerState.PointClamp;
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
        _renderContext.Clear();
    }


}