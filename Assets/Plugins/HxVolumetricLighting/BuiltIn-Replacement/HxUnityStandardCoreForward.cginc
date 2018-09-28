#ifndef UNITY_STANDARD_CORE_FORWARD_INCLUDED
#define UNITY_STANDARD_CORE_FORWARD_INCLUDED

#if defined(UNITY_NO_FULL_STANDARD_SHADER)
#	define UNITY_STANDARD_SIMPLE 1
#endif


#include "UnityStandardConfig.cginc"
#include "HxVolumetricCore.cginc"

#if UNITY_VERSION >= 201810
#if UNITY_VERSION >= 201826
#include "HxUnityStandardCore201826.cginc"
#else
#include "HxUnityStandardCore2018.cginc"
#endif
#else
#include "HxUnityStandardCore55.cginc"
#endif

VertexOutputForwardBase vertBase(VertexInput v) { return vertForwardBase(v); }
VertexOutputForwardAdd vertAdd(VertexInput v) { return vertForwardAdd(v); }
half4 fragBase(VertexOutputForwardBase i) : SV_Target{ return fragForwardBase(i); }
half4 fragAdd(VertexOutputForwardAdd i) : SV_Target{ return fragForwardAdd(i); }


#endif
