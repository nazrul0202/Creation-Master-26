using System.Collections.Generic;

namespace FifaLibrary;

public static class Fc26FormationNames
{
	private static readonly Dictionary<int, string> Names = new Dictionary<int, string>
	{
		{ 1, "4-1-3-2" }, { 2, "4-1-4-1" }, { 3, "4-2-3-1 Narrow" },
		{ 4, "4-2-3-1 Wide" }, { 5, "4-2-4" }, { 6, "4-3-1-2" },
		{ 7, "4-3-2-1" }, { 8, "4-3-3 Flat" }, { 9, "4-3-3 Holding" },
		{ 10, "4-3-3 Defend" }, { 11, "4-3-3 Attack" }, { 13, "4-2-2-2" },
		{ 14, "4-1-2-1-2 Wide" }, { 15, "4-1-2-1-2 Narrow" },
		{ 16, "4-4-2 Flat" }, { 17, "4-4-2 Holding" }, { 18, "4-4-1-1 Midfield" },
		{ 20, "4-5-1 Flat" }, { 21, "4-5-1 Attack" }, { 22, "3-1-4-2" },
		{ 23, "3-4-1-2" }, { 24, "3-4-2-1" }, { 25, "3-4-3 Flat" },
		{ 27, "3-5-2" }, { 29, "5-2-1-2" }, { 30, "5-2-3" },
		{ 31, "5-3-2 Holding" }, { 33, "5-4-1 Flat" }, { 36, "4-2-1-3" }
	};

	public static string Get(int id, string fallback)
	{
		string name;
		return Names.TryGetValue(id, out name) ? name : fallback;
	}
}
