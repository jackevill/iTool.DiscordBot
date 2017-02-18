using System.Collections.Generic;
using System.Xml.Serialization;


[XmlRootAttribute("playerstats")]
public class UserStatsForGame
{

    [XmlElementAttribute("steamID")]
    public long SteamID { get; set; }

    [XmlElementAttribute("gameName")]
    public string GameName { get; set; }

    [XmlArray("stats")]
    [XmlArrayItem("stat")]
    public List<Stat> Stats { get; set; } = new List<Stat>();

    [XmlArray("achievements")]
    [XmlArrayItem("achievement")]
    public List<Achievement> Achievements { get; set; } = new List<Achievement>();
}
