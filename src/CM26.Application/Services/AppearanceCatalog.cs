namespace CM26.Application.Services;

/// <summary>CM16-style generic appearance catalogue (ported from Creation Master 16
/// GenericHead.cs / PlayerForm.cs): named options for the players Face tab fields
/// (hair model + color, head model, skin/eyes/eyebrows/facial hair). Single source
/// of truth shared by the legacy canvas and the WPF studio.</summary>
public static class AppearanceCatalog
{
    /// <summary>CM16 domainHairColor items; index = haircolorcode.</summary>
    public static readonly string[] HairColors =
    [
        "Blonde", "Black", "Ash Blonde", "Dark Brown", "Platinum Blonde", "Light Brown",
        "Brown", "Red", "White", "Gray", "Green", "Violet", "Intense Red"
    ];

    /// <summary>CM16 domainFacialHair items; index = facialhairtypecode.</summary>
    public static readonly string[] FacialHairTypes =
    [
        "None", "Chin Stubble", "Chin Strap", "Goatee", "Casual Beard", "Partial Goatee",
        "Stubble", "Tuft", "Full Beard", "Light Goatee", "Mustache", "Light Chin Curtain",
        "Full Goatee", "Chin Curtain", "Beard", "Patchy Beard", "Light Goatee 2",
        "Light Goatee 3", "Patchy Beard 2", "Beard 2", "Chin Stubble 2", "Chin Stubble 3",
        "Full Goatee 2", "Goatee 2", "Casual Beard 2", "Partial Goatee 2", "Stubble 3",
        "Chin Curtain 2", "Full Berad 2", "Light Goatee 4", "Mustache 2", "Light Chin Curtain 2"
    ];

    /// <summary>CM16 comboFacialHairColor items; index = facialhaircolorcode.</summary>
    public static readonly string[] FacialHairColors =
    [
        "Black", "Light Blonde", "Dark Brown", "Light Brown", "Red", "Dark Blonde"
    ];

    /// <summary>CM16 comboSkintype items; index = skintypecode.</summary>
    public static readonly string[] SkinTypes =
    [
        "Clean", "Freckled", "Rough", "Type Female 3", "Type Female 4",
        "Type Female 5", "Type Female 6", "Type Female 7"
    ];

    /// <summary>CM16 skin-tone labels; codes are one-based. Values outside
    /// this standard palette remain untouched until explicitly changed.</summary>
    public static readonly string[] SkinTones =
    [
        "Light Pink", "Pink", "Dark Pink", "Light Yellow", "Medium Yellow",
        "Dark Yellow", "Very Light Brown", "Light Brown", "Medium Brown", "Dark Brown"
    ];

    /// <summary>CM16 comboEyeBow items; index = eyebrowcode.</summary>
    public static readonly string[] EyebrowTypes =
    [
        "Normal", "Big", "Thin", "Type Female 3", "Type Female 4", "Type Female 5", "Type Female 6"
    ];

    /// <summary>CM16 comboEyescolor items; index = eyecolorcode - 1 (codes start at 1).</summary>
    public static readonly string[] EyeColors =
    [
        "Dark Blue", "Light Blue", "Dark Brown", "Light Brown", "Brown and Green",
        "Dark Green", "Light Green", "Gray", "Black", "Dark Gray"
    ];

    /// <summary>CM16 comboFaceposer items; index = faceposercode.</summary>
    public static readonly string[] FacePosers =
    [
        "Default", "Variant 1", "Variant 2", "Variant 3"
    ];

    /// <summary>CM16 comboSideburns items; index = sideburnscode.</summary>
    public static readonly string[] Sideburns =
    [
        "No", "Yes"
    ];

    /// <summary>CM16 hair model sets (radio group per set, combo of model ids inside).</summary>
    public static readonly HairModelSet[] HairModelSets =
    [
        new("Shaven", [0, 25, 1, 43, 41, 46, 120]),
        new("Very Short", [26, 29, 47, 72, 92, 16, 28, 31, 37, 40, 45, 65, 77, 90, 117]),
        new("Short", [2, 21, 22, 30, 38, 54, 57, 70, 75, 78, 82, 97, 101, 102, 104, 105, 106, 107, 108, 111, 112, 113, 115, 114, 118, 121, 122, 124]),
        new("Modern", [17, 18, 19, 24, 39, 60, 61, 63, 64, 86, 88, 89, 94, 123, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 203, 213]),
        new("Medium", [36, 74, 13, 35, 42, 59, 69, 73, 85, 93, 32, 66, 67, 68, 14, 20, 23, 58, 62, 83, 95, 22, 52, 87, 98, 99, 100, 103, 116, 119]),
        new("Long", [8, 9, 15, 44, 84, 34, 10, 33, 12, 80, 11, 50, 51, 79, 53, 7]),
        new("Afro", [71, 4, 27, 5, 6, 96, 3, 109, 110]),
        new("Headbend", [55, 56, 76, 81, 49, 91, 48]),
        new("Female Hair", [500, 501, 502, 503, 504, 505, 506, 507, 508, 509, 510, 511, 512, 513, 514, 515, 516, 517, 518, 519, 520, 521, 522, 523, 524, 525, 526, 527, 528, 529, 530, 531, 532, 533, 534, 535, 536, 537, 538, 539, 540, 541, 542, 543, 544, 545, 546, 547, 548, 549, 550, 551, 552, 553, 554, 555, 556, 557, 558, 559, 560, 561, 562, 563, 564, 565, 566, 567, 568, 569, 570, 571, 572, 573, 574, 575, 576, 577, 578, 579, 580, 581, 160])
    ];

    /// <summary>CM16 head model sets (radio group per ethnicity, combo of model ids inside).</summary>
    public static readonly HeadModelSet[] HeadModelSets =
    [
        new("Caucasic", [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 2000, 2001, 2002, 2003, 2004, 2005, 2006, 2007, 2008, 2009, 2010, 2011, 2012, 2013, 2014, 2015, 2016, 2017, 2019, 2020, 2021, 2022, 2023, 2024, 2025, 2026, 2027, 2028, 2029, 2030, 3500, 3501, 3502, 3503, 3504, 3505, 4000, 4001, 4002, 4003]),
        new("Asiatic", [500, 501, 502, 503, 504, 505, 506, 507, 508, 509, 510, 511, 512, 513, 514, 515, 516, 517, 518, 519, 520, 521, 522, 523, 524, 525, 526, 527, 528, 529, 530, 531, 532]),
        new("African", [1000, 1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009, 1010, 1011, 1012, 1013, 1014, 1015, 1016, 1017, 1018, 1019, 1020, 1021, 1022, 1023, 1024, 1025, 1026, 1027, 3000, 3001, 3002, 3003, 3004, 3005, 4500, 4501, 4502, 4525, 5000, 5001, 5002, 5003]),
        new("Latin", [1500, 1501, 1502, 1503, 1504, 1505, 1506, 1507, 1508, 1509, 1510, 1511, 1512, 1513, 1514, 1515, 1516, 1517, 1518, 1519, 1520, 1521, 1522, 1523, 1524, 1525, 1526, 1527, 1528, 1529, 2500, 2501, 2502, 2503, 2504, 2505, 2506, 2507, 2508, 2509, 2510, 2511, 2512, 2513, 2514, 2515, 2516, 2517, 2518]),
        new("Female", [5500, 5501, 5502, 6000, 6001, 6002, 6500, 6501, 6502, 7000, 7001, 7002, 7500, 7501, 7502, 8000, 8001, 8002, 8500, 8501, 8502, 9000, 9001, 9002, 9500, 9501, 9502, 10000, 10001, 10002, 10500, 10501, 10502])
    ];

    /// <summary>One CM16 hair model set: a radio group name + the model ids it offers.</summary>
    public sealed record HairModelSet(string Name, int[] Models);

    /// <summary>One CM16 head model set: an ethnicity name + the model ids it offers.</summary>
    public sealed record HeadModelSet(string Name, int[] Models);

    /// <summary>Finds the CM16 hair set containing the given model id, or null.</summary>
    public static HairModelSet? FindHairSet(int modelId)
    {
        foreach (var set in HairModelSets)
        {
            if (Array.IndexOf(set.Models, modelId) >= 0) return set;
        }
        return null;
    }

    /// <summary>Finds the CM16 head set containing the given model id, or null.</summary>
    public static HeadModelSet? FindHeadSet(int modelId)
    {
        foreach (var set in HeadModelSets)
        {
            if (Array.IndexOf(set.Models, modelId) >= 0) return set;
        }
        return null;
    }

    /// <summary>Flattens a hair model set list into display labels ("SetName id").</summary>
    public static string[] Flatten(IEnumerable<HairModelSet> sets)
        => sets.SelectMany(s => s.Models.Select(id => $"{s.Name} {id}")).ToArray();

    /// <summary>Flattens a head model set list into display labels ("SetName id").</summary>
    public static string[] Flatten(IEnumerable<HeadModelSet> sets)
        => sets.SelectMany(s => s.Models.Select(id => $"{s.Name} {id}")).ToArray();

    /// <summary>Flattened hair models: display labels plus their parallel model ids.</summary>
    public static (string[] Names, int[] Values) FlattenModels(IEnumerable<HairModelSet> sets)
        => (Flatten(sets), sets.SelectMany(s => s.Models).ToArray());

    /// <summary>Flattened head models: display labels plus their parallel model ids.</summary>
    public static (string[] Names, int[] Values) FlattenModels(IEnumerable<HeadModelSet> sets)
        => (Flatten(sets), sets.SelectMany(s => s.Models).ToArray());
}
