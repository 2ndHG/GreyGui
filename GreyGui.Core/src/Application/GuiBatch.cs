using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreyGui;

public class GuiBatch
{
    private readonly GraphicsDevice _device;
    private readonly Effect _uiShader;
    private static readonly RasterizerState ScissorState = new() { ScissorTestEnable = true };

    public GuiBatch(GraphicsDevice device, Effect uiShader)
    {
        _device = device;
        _uiShader = uiShader;
    }
    public GuiBatch(GraphicsDevice device)
    {
        _device = device;
        _uiShader = GreyGui.Shader;
    }

    public void Draw(GreyGuiElement root, RenderContext context, Point position)
    {
        root.ResolveSizeDirty();

        Rectangle screenScissor = _device.Viewport.Bounds;
        root.Draw(position, context, screenScissor);
    }

    public void Flush(RenderContext context)
    {
        // Check if there are generated SDF bitmaps that haven't be drawn to the atlas
        GreyGui.TextSystem.SetGeneratedSdfBitmapToAtlas();

        if (context.Batches.Count == 0)
        {
            return;
        }
        Matrix projection = Matrix.CreateOrthographicOffCenter(0, _device.Viewport.Width, _device.Viewport.Height, 0, 0, 1);

        _device.BlendState = BlendState.NonPremultiplied;
        _device.RasterizerState = ScissorState;

        _uiShader.Parameters["WorldViewProjection"].SetValue(projection);
        foreach (DrawBatch batch in context.Batches)
        {
            if (batch.IndexCount == 0) continue;

            _device.ScissorRectangle = batch.Scissor;
            _uiShader.Parameters["Texture"].SetValue(batch.Texture);

            foreach (EffectPass pass in _uiShader.CurrentTechnique.Passes)
            {
                pass.Apply();
                _device.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    context.Vertices,
                    0,
                    context.VertexCount,
                    context.Indices,
                    batch.IndexOffset,
                    batch.IndexCount / 3
                );
            }
        }
        context.Clear();
    }
}