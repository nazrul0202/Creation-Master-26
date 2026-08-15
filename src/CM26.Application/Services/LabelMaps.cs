namespace CM26.Application.Services;

/// <summary>Human-friendly labels for real FC26 field names (no FIFA16 names carried over).</summary>
public static class LabelMaps
{
    public static readonly Dictionary<string, string> Nations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["nationid"] = "Nation ID",
        ["nationname"] = "Nation Name",
        ["confederation"] = "Confederation",
        ["isocountrycode"] = "ISO Code",
        ["groupid"] = "Group ID",
        ["top_tier"] = "Top Tier",
        ["nationstartingfirstletter"] = "Starting Letter",
    };

    public static readonly Dictionary<string, string> Leagues = new(StringComparer.OrdinalIgnoreCase)
    {
        ["leagueid"] = "League ID",
        ["leaguename"] = "League Name",
        ["countryid"] = "Country",
        ["level"] = "Level",
        ["leaguetype"] = "League Type",
        ["iswomencompetition"] = "Women's Competition",
        ["isinternationalleague"] = "International",
        ["iswithintransferwindow"] = "Within Transfer Window",
    };

    public static readonly Dictionary<string, string> Teams = new(StringComparer.OrdinalIgnoreCase)
    {
        ["teamid"] = "Team ID",
        ["teamname"] = "Team Name",
        ["overallrating"] = "Overall Rating",
        ["attackrating"] = "Attack Rating",
        ["midfieldrating"] = "Midfield Rating",
        ["defenserating"] = "Defence Rating",
        ["captainid"] = "Captain",
        ["penaltytakerid"] = "Penalty Taker",
        ["freekicktakerid"] = "Free-Kick Taker",
        ["leftcornerkicktakerid"] = "Left Corner Taker",
        ["rightcornerkicktakerid"] = "Right Corner Taker",
        ["domesticprestige"] = "Domestic Prestige",
        ["internationalprestige"] = "International Prestige",
        ["foundationyear"] = "Founded",
        ["clubworth"] = "Club Worth",
        ["popularity"] = "Popularity",
        ["youthdevelopment"] = "Youth Development",
        ["ballid"] = "Home Ball",
    };

    public static readonly Dictionary<string, string> Players = new(StringComparer.OrdinalIgnoreCase)
    {
        ["playerid"] = "Player ID",
        ["firstnameid"] = "First Name (ref)",
        ["lastnameid"] = "Last Name (ref)",
        ["commonnameid"] = "Common Name (ref)",
        ["overallrating"] = "Overall",
        ["potential"] = "Potential",
        ["preferredposition1"] = "Preferred Position",
        ["preferredposition2"] = "Position 2",
        ["preferredposition3"] = "Position 3",
        ["preferredposition4"] = "Position 4",
        ["birthdate"] = "Date of Birth",
        ["height"] = "Height (cm)",
        ["weight"] = "Weight (kg)",
        ["nationality"] = "Nationality",
        ["preferredfoot"] = "Preferred Foot",
        ["weakfootabilitytypecode"] = "Weak Foot",
        ["skillmoves"] = "Skill Moves",
        ["attackworkrate"] = "Attacking Work Rate",
        ["defensiveworkrate"] = "Defensive Work Rate",
        ["contractvaliduntil"] = "Contract Until",
        ["playerjointeamdate"] = "Joined Team",
        // pace/shooting/passing/dribbling/defence/physical
        ["acceleration"] = "Acceleration",
        ["sprintspeed"] = "Sprint Speed",
        ["finishing"] = "Finishing",
        ["shotpower"] = "Shot Power",
        ["longshots"] = "Long Shots",
        ["volleys"] = "Volleys",
        ["penalties"] = "Penalties",
        ["shortpassing"] = "Short Passing",
        ["longpassing"] = "Long Passing",
        ["curve"] = "Curve",
        ["dribbling"] = "Dribbling",
        ["ballcontrol"] = "Ball Control",
        ["agility"] = "Agility",
        ["balance"] = "Balance",
        ["reactions"] = "Reactions",
        ["composure"] = "Composure",
        ["interceptions"] = "Interceptions",
        ["defensiveawareness"] = "Defensive Awareness",
        ["standingtackle"] = "Standing Tackle",
        ["slidingtackle"] = "Sliding Tackle",
        ["jumping"] = "Jumping",
        ["stamina"] = "Stamina",
        ["strength"] = "Strength",
        ["aggression"] = "Aggression",
        ["headingaccuracy"] = "Heading Accuracy",
        ["gkdiving"] = "GK Diving",
        ["gkhandling"] = "GK Handling",
        ["gkkicking"] = "GK Kicking",
        ["gkpositioning"] = "GK Positioning",
        ["gkreflexes"] = "GK Reflexes",
    };

    public static readonly Dictionary<string, string> Managers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["managerid"] = "Manager ID",
        ["firstname"] = "First Name",
        ["surname"] = "Last Name",
        ["commonname"] = "Common Name",
        ["teamid"] = "Team",
        ["nationality"] = "Nationality",
        ["birthdate"] = "Date of Birth",
        ["height"] = "Height (cm)",
        ["weight"] = "Weight (kg)",
        ["starrating"] = "Star Rating",
    };

    public static readonly Dictionary<string, string> Stadiums = new(StringComparer.OrdinalIgnoreCase)
    {
        ["stadiumid"] = "Stadium ID",
        ["name"] = "Stadium Name",
        ["capacity"] = "Capacity",
        ["hometeamid"] = "Home Team",
        ["countrycode"] = "Country",
        ["cityid"] = "City",
        ["stadiumpitchlength"] = "Pitch Length",
        ["stadiumpitchwidth"] = "Pitch Width",
        ["defaultweather"] = "Default Weather",
        ["defaultseason"] = "Default Season",
        ["defaulttime"] = "Default Time",
    };

    public static readonly Dictionary<string, string> Referees = new(StringComparer.OrdinalIgnoreCase)
    {
        ["refereeid"] = "Referee ID",
        ["firstname"] = "First Name",
        ["surname"] = "Last Name",
        ["nationalitycode"] = "Nationality",
        ["leagueid"] = "League",
        ["birthdate"] = "Date of Birth",
        ["height"] = "Height (cm)",
        ["weight"] = "Weight (kg)",
        ["cardstrictness"] = "Card Strictness",
        ["foulstrictness"] = "Foul Strictness",
    };

    public static readonly Dictionary<string, string> Formations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["formationid"] = "Formation ID",
        ["formationname"] = "Formation Name",
        ["teamid"] = "Team",
        ["attackers"] = "Attackers",
        ["midfielders"] = "Midfielders",
        ["defenders"] = "Defenders",
        ["offensiverating"] = "Offensive Rating",
    };

    public static readonly Dictionary<string, string> Kits = new(StringComparer.OrdinalIgnoreCase)
    {
        ["teamkitid"] = "Kit ID",
        ["teamtechid"] = "Team",
        ["teamkittypetechid"] = "Kit Type",
        ["year"] = "Year",
        ["teamcolorprimr"] = "Primary R",
        ["teamcolorprimg"] = "Primary G",
        ["teamcolorprimb"] = "Primary B",
        ["teamcolorsecr"] = "Secondary R",
        ["teamcolorsecg"] = "Secondary G",
        ["teamcolorsecb"] = "Secondary B",
        ["jerseytemplateindex"] = "Jersey Template",
        ["numberfonttype"] = "Number Font",
        ["shortstemplateindex"] = "Shorts Template",
        ["sockstemplateindex"] = "Socks Template",
    };

    public static readonly Dictionary<string, string> Tournaments = new(StringComparer.OrdinalIgnoreCase)
    {
        ["competitionid"] = "Competition ID",
        ["competitionimportance"] = "Importance",
        ["isrealcompetition"] = "Real Competition",
        ["iswomencompetition"] = "Women's Competition",
        ["country_lock"] = "Country Lock",
        ["languageregion"] = "Language Region",
        ["subsonbench"] = "Subs on Bench",
        ["goalscoringrecord"] = "Goalscoring Record",
        ["crowdregion"] = "Crowd Region",
        ["competitionchampionid"] = "Current Champion",
        ["finalstadium"] = "Final Stadium",
        ["ballid"] = "Ball",
        ["finalballid"] = "Final Ball",
        ["winterballid"] = "Winter Ball",
        ["competitioncolor1r"] = "Color 1 R",
        ["competitioncolor1g"] = "Color 1 G",
        ["competitioncolor1b"] = "Color 1 B",
        ["competitioncolor2r"] = "Color 2 R",
        ["competitioncolor2g"] = "Color 2 G",
        ["competitioncolor2b"] = "Color 2 B",
        ["badge_rs"] = "Badge RS",
        ["badge_ls"] = "Badge LS",
        ["badge_chest"] = "Badge Chest",
        ["badge_rs_champions"] = "Badge RS Champions",
        ["badge_ls_champions"] = "Badge LS Champions",
        ["badge_chest_champions"] = "Badge Chest Champions",
        ["has_var"] = "Has VAR",
        ["hasmascot"] = "Has Mascot",
        ["hasstandingcrowd"] = "Has Standing Crowd",
        ["haslargeflag"] = "Has Large Flag",
        ["hasvikingclap"] = "Has Viking Clap",
        ["hasmediarope"] = "Has Media Rope",
        ["has_mediarope"] = "Has Media Rope",
        ["replay360degree"] = "360 Replays",
        ["authenticwipe"] = "Authentic Wipe",
        ["authenticwipeid"] = "Authentic Wipe ID",
        ["goaljingle"] = "Goal Jingle",
        ["introconfetti"] = "Intro Confetti",
        ["introanthemidle"] = "Intro Anthem Idle",
        ["introteamportrait"] = "Intro Team Portrait",
        ["pitchbranding"] = "Pitch Branding",
        ["onpitchgraphics"] = "On-Pitch Graphics",
        ["stadiumcrowdmap"] = "Stadium Crowd Map",
        ["inflatables"] = "Inflatables",
        ["goalnetads"] = "Goalnet Ads",
    };

    public static readonly Dictionary<string, string> Balls = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ballid"] = "Ball ID",
        ["balltype"] = "Ball Type",
        ["islicensed"] = "Licensed",
        ["isavailableinstore"] = "Visible in Game Menu",
        ["isembargoed"] = "Embargoed",
        ["isrewardable"] = "Rewardable",
        ["powid"] = "POW ID",
    };

    public static readonly Dictionary<string, string> Shoes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["shoetype"] = "Shoes Id",
        ["shoedesign"] = "Design",
        ["shoecolor1"] = "Color 1",
        ["shoecolor2"] = "Color 2",
        ["manufacturerid"] = "Manufacturer",
        ["isadidas"] = "Adidas",
        ["ishighboot"] = "High Boot",
        ["issmallsided"] = "Small Sided",
        ["islegacy"] = "Legacy",
        ["islicensed"] = "Licensed",
        ["isavailableinstore"] = "Is Available",
        ["isembargoed"] = "Embargoed",
        ["isrewardable"] = "Rewardable",
        ["islocked"] = "Locked",
        ["gender"] = "Shoes for Woman",
        ["powid"] = "POW ID",
    };

    public static readonly Dictionary<string, string> Gloves = new(StringComparer.OrdinalIgnoreCase)
    {
        ["gloveid"] = "Gloves Id",
        ["islicensed"] = "Licensed",
        ["isavailableinstore"] = "Is Available",
        ["isembargoed"] = "Embargoed",
    };

    public static readonly Dictionary<string, string> Sponsors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["adsponserid"] = "Sponsor Id",
        ["name"] = "Name",
        ["isea"] = "EA Sponsor",
        ["isfut"] = "FUT Sponsor",
        ["basecolour"] = "Base Colour",
        ["length"] = "Length",
    };
}