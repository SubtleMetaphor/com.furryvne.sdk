#define RAYMARCHING_STEPS 50
#define MAX_DIMENSIONS max(_Dimensions.x, max(_Dimensions.y, _Dimensions.z))
#define RAYMARCHING_EPSILON pow(1.0 / MAX_DIMENSIONS, 2)
//#define RAYMARCHING_EPSILON 1.0 / MAX_DIMENSIONS
//#define RAYMARCHING_EPSILON .002

uniform sampler3D _SDF;
uniform float3 _Dimensions;
uniform float3 _BoundsMin;
uniform float3 _BoundsMax;
uniform float _MaxExtents;
uniform int _SDFComponentIndex;

//SamplerState MyLinearClampSampler;

inline float _Raymarching_InvLerp(float from, float to, float value)
{
	return (value - from) / (to - from);
}

inline float3 _Raymarching_InvLerp3(float3 from, float3 to, float3 value)
{
	return (value - from) / (to - from);
}

inline float3 PointToNormalizedCoords(float3 p)
{
	return _Raymarching_InvLerp3(_BoundsMin, _BoundsMax, p);
	//return p;
}

//inline float3 Remap(float3 v, float3 fromMin, float3 fromMax, float3 toMin, float3 toMax)
//{
//	return (v - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
//}
//
//inline float3 LocalPosToUVW(float3 localPos)
//{
//	return Remap(localPos, _BoundsMin, _BoundsMax, 0, 1);
//}

inline float Sample(float3 coords)
{
	//return _VectorFieldSDF.SampleLevel(MyLinearClampSampler, p, 0).a;
	//return tex3D(_VectorFieldSDF, coords).a * _MaxExtents + .0005;
	//return tex3D(_VectorFieldSDF, coords).a * _MaxExtents;
	return tex3D(_SDF, coords)[_SDFComponentIndex] * _MaxExtents;
	//float dist = length(coords - .5) - .25;
	//return lerp(_BoundsMin, _BoundsMax, _sample);
	//return length(coords) - .1;
}

inline float SDFFunc(float3 p)
{
	float3 normalizedCoords = PointToNormalizedCoords(p);

	return Sample(normalizedCoords);
}

float3 RaymarchingNormal(float3 coords, float3 distance)
{
	float x = Sample(coords + float3(distance.x, 0, 0)) - Sample(coords + float3(-distance.x, 0, 0));
	float y = Sample(coords + float3(0, distance.y, 0)) - Sample(coords + float3(0, -distance.y, 0));
	float z = Sample(coords + float3(0, 0, distance.z)) - Sample(coords + float3(0, 0, -distance.z));

	float3 res = float3(x, y, z);

	return normalize(res);
}

float4 Raymarch(float3 p, float3 dir, float epsilon, int steps)
{
	float lastSign = 1;
	float lastDistance;
	float3 lastP;
	steps = min(steps, 100);

	for (int i = 0; i < steps; ++i)
	{
		float distance = SDFFunc(p);
		float newSign = sign(distance);

		if (distance < epsilon)
		{
			return float4(p, 1);
		}

		if (i > 0 && newSign != lastSign)
		{
			float t = _Raymarching_InvLerp(lastDistance, distance, 0);

			return float4(lerp(lastP, p, t), 1);
		}
		
		lastP = p;
		lastSign = newSign;
		lastDistance = distance;
		p += distance * dir;
	}

	return 0;
}

void Raymarching(in float3 p, in float3 dir, in float epsilon, in int steps, out float3 pos, out float3 normal, out float use)
{
	epsilon = epsilon < 0 ? RAYMARCHING_EPSILON : pow(1.0 / MAX_DIMENSIONS, epsilon);

	float4 res = Raymarch(p, dir, epsilon, steps);

	if (res.a > 0)
	{
		float3 normalizedCoords = PointToNormalizedCoords(res.xyz);
		normal = RaymarchingNormal(normalizedCoords, rcp(_Dimensions));
		//normal = RaymarchingNormal(normalizedCoords, .01);
		//normal = normalize(tex3D(_VectorFieldSDF, normalizedCoords).xyz);

		pos = res.xyz;
		use = res.a;

		return;
	}

	pos = 0;
	normal = 0;
	use = 0;
}

void Raymarching(in float3 p, in float3 dir, in int steps, out float3 pos, out float3 normal, out float use)
{
	Raymarching(p, dir, RAYMARCHING_EPSILON, steps, pos, normal, use);
}

inline float LinearEyeDepthToOutDepth(float z)
{
	return (1 - _ZBufferParams.w * z) / (_ZBufferParams.z * z);
}
