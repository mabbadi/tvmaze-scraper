public class ELKConfiguration
{
    public ELKConfiguration() { }
    public string url { get; set; }
    public string index { get; set; }
}

public class ApplicationLogging
{
    public ApplicationLogging() { }
    public bool logElastic { get; set; }
    public bool logBackgroundJobService { get; set; }
    public bool logHangfire { get; set; }
    public bool logConsumer { get; set; }
    public bool logElasticFull { get; set; }    
    public bool logBackgroundJobServiceUrls { get; set; }        
}

public class ApplicationInfo
{
    public ApplicationInfo() { }
    public bool IsMainApplication { get; set; }
}

public class ApiKey
{
    public ApiKey() { }
    public string Key { get; set; }
}