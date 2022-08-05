﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Catalog.Bootstrapper.Postgres;

public class PostgresBootstrapperService : IBootstrapperService
{

    public System.Net.NetworkCredential SysAdminUser { get; set; }
    public System.Net.DnsEndPoint ServerEndpoint { get; set; }
    public System.Net.NetworkCredential AppUser { get; set; }
    public string DatabaseToCreate { get; set; }
    public string InitialDatabase { get; set; }


    public Task InitializeAsync()
    {
        if (this.SysAdminUser is null)
            throw new InvalidOperationException("SysAdminUser can't be null");

        if (this.ServerEndpoint is null)
            throw new InvalidOperationException("ServerEndpoint can't be null");

        if (this.AppUser is null)
            throw new InvalidOperationException("AppUser can't be null");

        if (this.DatabaseToCreate is null)
            throw new InvalidOperationException("DatabaseToCreate can't be null");

        if (this.InitialDatabase is null)
            throw new InvalidOperationException("InitialDatabase can't be null");

        return Task.CompletedTask;
    }

    public async Task ExecuteAsync()
    {
        using var connection = new NpgsqlConnection($"server={this.ServerEndpoint?.Host ?? "localhost"};Port={this.ServerEndpoint?.Port};Database={this.InitialDatabase};User Id={this.SysAdminUser?.UserName};Password={this.SysAdminUser?.Password};");
        await connection.OpenAsync();
        try
        {
            await this.CreateAppUser(connection);
            await this.CreateDatabase(connection);
        }
        finally
        {
            if (connection != null)
                await connection.CloseAsync();
        }
    }


    private async Task CreateAppUser(NpgsqlConnection connection)
    {
        using var command = connection.CreateCommand();

        command.CommandText = @$"SELECT count(rolname) FROM pg_catalog.pg_roles WHERE  rolname = '{this.AppUser.UserName}'";

        long qtd = (long) await command.ExecuteScalarAsync();

        if (qtd == 0)
        {

            command.CommandText = @$"CREATE ROLE {this.AppUser.UserName} WITH
	                    LOGIN
	                    NOSUPERUSER
	                    NOCREATEDB
	                    NOCREATEROLE
	                    INHERIT
	                    NOREPLICATION
	                    CONNECTION LIMIT -1
	                    PASSWORD '{this.AppUser.Password}';";

            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task CreateDatabase(NpgsqlConnection connection)
    {
        using var command = connection.CreateCommand();

        command.CommandText = @$"SELECT count(datname) FROM pg_database WHERE datname = '{this.DatabaseToCreate}'";

        long qtd = (long) await command.ExecuteScalarAsync();

        if (qtd == 0)
        {
            command.CommandText = @$"CREATE DATABASE {this.DatabaseToCreate} 
                            WITH 
                            OWNER = {this.AppUser.UserName}
                            ENCODING = 'UTF8'
                            CONNECTION LIMIT = -1;";

            await command.ExecuteNonQueryAsync();
        }

    }

}
