struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : TEXCOORD3;
    float4 BorderColor : TEXCOORD4;
    float2 TexCoord : TEXCOORD0;
    float2 LocalCoord : TEXCOORD1;
    float4 RectParams : TEXCOORD2; // x:w, y:h, z:radius, w:borderWidth
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : TEXCOORD3;
    float4 BorderColor: TEXCOORD4;
    float2 TexCoord : TEXCOORD0;
    float2 LocalCoord : TEXCOORD1;
    float4 RectParams : TEXCOORD2;
};

float4x4 WorldViewProjection;
Texture2D Texture;
sampler TextureSampler = sampler_state { Texture = <Texture>; };

float RoundedRectSDF(float2 p, float2 b, float r)
{
    float2 q = abs(p) - b + r;
    return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r;
}

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, WorldViewProjection);
    output.Color = input.Color;
    output.BorderColor = input.BorderColor;
    output.TexCoord = input.TexCoord;
    output.LocalCoord = input.LocalCoord;
    output.RectParams = input.RectParams;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 fillColor = input.Color / 255.0; 
    float4 borderColor = input.BorderColor / 255.0;

    float2 size = input.RectParams.xy;
    float radius = input.RectParams.z;
    float borderWidth = input.RectParams.w;
    
    float2 halfSize = size * 0.5;
    float2 pixelPos = (input.LocalCoord - 0.5) * size;

    float dist = RoundedRectSDF(pixelPos, halfSize, radius);

    float edgeSoftness = 1.0; 
    float alpha = smoothstep(edgeSoftness, 0.0, dist);

    float borderAlpha = smoothstep(borderWidth + edgeSoftness, borderWidth, -dist);
    
    if(borderWidth <= 0) borderAlpha = 1.0;

    float4 finalColor = lerp(borderColor, fillColor, borderAlpha);
    
    float4 texColor = tex2D(TextureSampler, input.TexCoord);
    
    return finalColor * texColor * alpha;
}

technique SpriteBatch
{
    pass P0
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 MainPS();
    }
};