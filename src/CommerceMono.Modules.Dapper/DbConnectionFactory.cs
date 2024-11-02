using System;
using System.Data;
using Npgsql;

namespace CommerceMono.Modules.Dapper;

public class NpgSqlDbConnectionFactory : IDbConnectionFactory
{
	private readonly string _connectionString;

	public NpgSqlDbConnectionFactory(string connectionString)
	{
		_connectionString = connectionString;
	}

	public async Task<IDbConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
	{
		var connection = new NpgsqlConnection(_connectionString);
		await connection.OpenAsync(cancellationToken);
		return connection;
	}
}

public interface IDbConnectionFactory
{
	Task<IDbConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
}
