// Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
using Nest;


public class Person
{
    public int id { get; set; }
    public string url { get; set; }
    [Keyword]
    public string name { get; set; }
    public Country country { get; set; }
    public DateTime? birthday { get; set; }
    public DateTime? deathday { get; set; }
    [Keyword]
    public string gender { get; set; }
    public Image image { get; set; }
    public int updated { get; set; }
    [PropertyName("links")]
    public Links _links { get; set; }
}
public class Country
{
    [Keyword]
    public string name { get; set; }
    [Keyword]
    public string code { get; set; }
    [Keyword]
    public string timezone { get; set; }
}

public class DvdCountry
{
    [Keyword]
    public string name { get; set; }
    [Keyword]
    public string code { get; set; }
    [Keyword]
    public string timezone { get; set; }
}

public class Embedded
{
    [Nested]
    public List<Cast> cast { get; set; }
}
public class Character
{
    public int id { get; set; }
    public string url { get; set; }
    [Keyword]
    public string name { get; set; }
    public Image image { get; set; }
    [PropertyName("links")]
    public Links _links { get; set; }
}
public class Cast
{
    [Nested]
    public Person person { get; set; }
    public Character character { get; set; }
    public bool self { get; set; }
    public bool voice { get; set; }
}

public class Externals
{
    public int? tvrage { get; set; }
    public int? thetvdb { get; set; }
    [Keyword]
    public string imdb { get; set; }
}

public class Image
{
    public string medium { get; set; }
    public string original { get; set; }
}

public class Links
{
    public Self self { get; set; }
    public Show show { get; set; }
    public Previousepisode previousepisode { get; set; }
    public Nextepisode nextepisode { get; set; }
}

public class Network
{

    public int id { get; set; }
    public string name { get; set; }
    public Country country { get; set; }
    public string officialSite { get; set; }
}

public class Nextepisode
{
    public string href { get; set; }
}

public class Previousepisode
{
    public string href { get; set; }
}

public class Rating
{
    public double? average { get; set; }
}

public class ShowDetails
{

    public int id { get; set; }
    public string url { get; set; }
    [Keyword]
    public string name { get; set; }
    public int season { get; set; }
    public int? number { get; set; }
    [Keyword]
    public string type { get; set; }
    [Keyword]
    public string language { get; set; }
    public int? runtime { get; set; }
    public Rating rating { get; set; }
    public Image image { get; set; }
    [Text]
    public string summary { get; set; }
    [PropertyName("links")]
    public Links _links { get; set; }
    [Nested]
    [PropertyName("embedded")]
    public Embedded _embedded { get; set; }

    public List<string> genres { get; set; }
    public string status { get; set; }
    public int? averageRuntime { get; set; }
    public DateTime? premiered { get; set; }
    public DateTime? ended { get; set; }
    public string officialSite { get; set; }
    public Schedule schedule { get; set; }
    public int weight { get; set; }
    public Network network { get; set; }
    public WebChannel webChannel { get; set; }
    public DvdCountry dvdCountry { get; set; }
    public Externals externals { get; set; }
    public int updated { get; set; }
}

public class Schedule
{
    [Keyword]
    public string time { get; set; }
    public List<string> days { get; set; }
}

public class Self
{
    public string href { get; set; }
}

public class Show
{

    public int id { get; set; }
    public string href { get; set; }
    public string url { get; set; }
    [Keyword]
    public string name { get; set; }
    [Keyword]
    public string type { get; set; }
    [Keyword]
    public string language { get; set; }
    public List<string> genres { get; set; }
    [Keyword]
    public string status { get; set; }
    public int? runtime { get; set; }
    public int? averageRuntime { get; set; }
    public DateTime? premiered { get; set; }
    public DateTime? ended { get; set; }
    public string officialSite { get; set; }
    public Schedule schedule { get; set; }
    public Rating rating { get; set; }
    public int weight { get; set; }
    public Network network { get; set; }
    public WebChannel webChannel { get; set; }
    public DvdCountry dvdCountry { get; set; }
    public Externals externals { get; set; }
    public Image image { get; set; }
    [Text]
    public string summary { get; set; }
    public int updated { get; set; }
    [PropertyName("links")]
    public Links _links { get; set; }
}

public class WebChannel
{


    public int id { get; set; }
    [Keyword]
    public string name { get; set; }
    public Country country { get; set; }
    public string officialSite { get; set; }
}

