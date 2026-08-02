namespace CM26.MeshKit;

internal static class HalfUtils
{
	public static float Unpack(ushort half)
	{
		var sign = (half & 0x8000) != 0 ? -1f : 1f;
		var exponent = (half >> 10) & 0x1F;
		var mantissa = half & 0x3FF;
		return exponent switch
		{
			0x00 => sign * mantissa * 5.96046448e-08f,
			0x1F => mantissa == 0
				? sign * float.PositiveInfinity
				: sign * float.NaN,
			_ => sign * MathF.Pow(2f, exponent - 15) * (1f + mantissa / 1024f)
		};
	}
}