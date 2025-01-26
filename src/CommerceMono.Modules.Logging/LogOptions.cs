namespace CommerceMono.Modules.Logging;

public class LogOptions
{
	public string Level { get; set; }

	public FileOptions File { get; set; }

	public string LogTemplate { get; set; }

	public LogOptions()
	{
		File = new FileOptions();
		Level = "";
		LogTemplate = "";
	}
}