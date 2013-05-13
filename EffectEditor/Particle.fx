#include "header.fx"

float4x4 Projection;
float4x4 ViewProjection;
float Time;
float2 TargetSize;//描画先のwidth,height
float4 Color;
float2 Offset;//スクリーン座標で加えるオフセット
sampler texsampler : register(s0);

struct VertexShaderInput
{
	float3 Pos : POSITION0;
	float3 Vel : POSITION1;
	float3 Acc : POSITION2;
	float3 Alpha : COLOR0;
	float R : PSIZE0;
	float Time : BLENDWEIGHT0;
	//float Index : TEXCOORD0;
	float2 Tex : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Pos : POSITION0;
	float Alpha : COLOR0;
	float2 tex : TEXCOORD0;

};

VertexShaderOutput VS2D(VertexShaderInput input)
{
	VertexShaderOutput output;
	float dtime = Time - input.Time;
	float2 p;
	output.Alpha = input.Alpha.x + input.Alpha.y * dtime + 0.5 * input.Alpha.z * dtime * dtime;
	output.tex = input.Tex;
	p = input.Pos.xy + input.Vel.xy * dtime + 0.5 * input.Acc.xy * dtime * dtime + Offset;//中心点のスクリーン座標演算
	p += (output.tex * 2 - 1) * input.R;//正方形の各頂点の座標
	p /= TargetSize * 0.5;//3D座標 0~2
	p -= 1;//-1~+1
	output.Pos = float4(p.x, -p.y, 0, output.Alpha > 0);//Alpha <= 0ならwを0にして表示しない
	
	//input.Pos.xy -= TargetSize.xy * 0.5;
	//input.Pos.y *= -1;
	//output.Pos = mul(float4(input.Pos, 1), ViewProjection);
	//output.Pos.xy += (output.tex.xy * 2 - 1) * input.R * float2(Projection._m00, Projection._m11) * 0.5;
	return output;
}

VertexShaderOutput VS3D(VertexShaderInput input)
{
	VertexShaderOutput output;
	float dtime = Time - input.Time;

	output.tex = input.Tex;
	output.Alpha = input.Alpha.x + input.Alpha.y * dtime + 0.5 * input.Alpha.z * dtime * dtime;
	//input.R *= output.Alpha > 0;
	input.Pos = input.Pos + input.Vel * dtime + 0.5 * input.Acc * dtime * dtime;
	//input.Pos.x -= TargetSize.x * 0.5;
	//input.Pos.y -= TargetSize.y * 0.5;
	//input.Pos.y *= -1;
	//output.Pos.xy += (output.tex.xy * 2 - 1) * input.R * 0.5;
	output.Pos = mul(float4(input.Pos, 1), ViewProjection);
	output.Pos.xy += (output.tex.xy * 2 - 1) * input.R * float2( Projection._m00, Projection._m11) * 0.5;	
	
	return output;
}

float4 PixelShaderFunction(float alpha : COLOR0, float2 tex : TEXCOORD0) : COLOR0
{
	float4 t = tex2D(texsampler, tex);
	t *= alpha;
	return  t * Color;
}

technique Technique1
{
	pass Game2D
	{
		VertexShader = compile VS VS2D();
		PixelShader = compile PS PixelShaderFunction();
	}

	pass Back3D
	{
		VertexShader = compile VS VS3D();
		PixelShader = compile PS PixelShaderFunction();
	}
}
