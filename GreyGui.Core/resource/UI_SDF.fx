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
    float4 Position : SV_POSITION; // Normalized Device Coordinates (NDC) 
    float4 Color : TEXCOORD3;
    float4 BorderColor: TEXCOORD4;
    float2 TexCoord : TEXCOORD0;
    float2 LocalCoord : TEXCOORD1;
    float4 RectParams : TEXCOORD2;
};

float4x4 WorldViewProjection;
Texture2D Texture;
sampler TextureSampler = sampler_state { Texture = <Texture>; 
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear; 
    AddressU = Clamp; 
    AddressV = Clamp;
};

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
    float fontWeightPx = -0.5;
    if (input.RectParams.w < -0.5) 
    {
        float glyphRange = input.RectParams.z;
        float2 glyphSize = input.RectParams.xy; 

        float3 msd = tex2D(TextureSampler, input.TexCoord).rgb;
        float sigDist = max(min(msd.r, msd.g), min(max(msd.r, msd.g), msd.b));
        
        float2 screenPixelsPerLocalUnit = 1.0 / fwidth(input.LocalCoord);
        float2 msdfUnit = float2(glyphRange, glyphRange) / glyphSize;
        
        float screenPxRange = 0.5 * dot(msdfUnit, screenPixelsPerLocalUnit);
        screenPxRange = max(screenPxRange, 1.0); 

        float rawScreenPxDist = screenPxRange * (sigDist - 0.5);

        float finalScreenPxDist = rawScreenPxDist + fontWeightPx;

        float alpha = clamp(finalScreenPxDist + 0.5, 0.0, 1.0);
        
        return input.Color * alpha;
    }
    
    float2 halfSize = size * 0.5;
    float2 pixelPos = (input.LocalCoord - 0.5) * size;

    float dist = RoundedRectSDF(pixelPos, halfSize, radius);

    float edgeSoftness = 0.1; 
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