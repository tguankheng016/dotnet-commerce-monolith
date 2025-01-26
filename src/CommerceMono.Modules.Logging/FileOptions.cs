namespace CommerceMono.Modules.Logging;

public class FileOptions
{
    public bool Enabled { get; set; }

    public string Path { get; set; }

    public string Interval { get; set; }

    public FileOptions()
    {
        Path = "";
        Interval = "";
    }
}
