using System;

namespace CommerceMono.Modules.Caching;

public class RedisOptions
{
	public string Host { get; set; }

	public int Port { get; set; }

	public bool Enabled { get; set; }

	public RedisOptions()
	{
		Host = "";
		Port = 6379;
		Enabled = false;
	}
}
