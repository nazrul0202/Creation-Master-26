namespace CM26.MeshKit;

public enum MeshSubsetCategory
{
	Opaque,
	Transparent,
	TransparentDecal,
	ZOnly,
	Shadow,
	Count
}

[Flags]
public enum MeshSubsetCategoryFlags
{
	Opaque = 1,
	Transparent = 2,
	TransparentDecal = 4,
	Normal = 7,
	ZOnly = 8,
	Shadow = 0x10,
	DynamicReflection = 0x20,
	PlanarReflection = 0x40,
	StaticReflection = 0x80,
	DistantShadowCache = 0x100,
	ShadowOverride = 0x200,
	DynamicReflectionOverride = 0x400,
	PlanarReflectionOverride = 0x800,
	StaticReflectionOverride = 0x1000,
	DistantShadowCacheOverride = 0x2000,
	ZPass = 0x4000,
	PlanarShadow = 0x8000,
	PlanarShadowOverride = 0x10000,
	BakedLighting = 0x20000,
	ForwardDepthPass = 0x40000,
	NoForwardDepthPass = 0x80000,
	All = 0xFFFFF
}