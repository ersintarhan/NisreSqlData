using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using NisreSqlData;

namespace NisrePgData
{
    public class DbRepo
    {
        private readonly SqlConnectionStringBuilder _connectionString;

        public DbRepo(SqlConnectionStringBuilder csb)
        {
            _connectionString = csb;
        }

        #region Private Methods

        public async Task Execute(string procedure,bool readOnly, SqlParam parameter)
        {
            try
            {
                var cs = _connectionString;
                _connectionString.ApplicationIntent = readOnly ? ApplicationIntent.ReadOnly : ApplicationIntent.ReadWrite;
                using var db = new SqlConnection(cs.ToString());
                await db.OpenAsync();
                await db.ExecuteAsync(procedure, parameter, commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                var exception = new NisreDbException(ex, parameter);
                throw exception;
            }
            catch (Exception e)
            {
                throw new NisreDbException(e);
            }
        }

        public async Task Execute(string procedure, bool readOnly)
        {
            try
            {
                var cs = _connectionString;
                _connectionString.ApplicationIntent = readOnly ? ApplicationIntent.ReadOnly : ApplicationIntent.ReadWrite;
                using var db = new SqlConnection(cs.ToString());
                await db.OpenAsync();
                await db.ExecuteAsync(procedure, commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                var exception = new NisreDbException(ex);
                throw exception;
            }
            catch (Exception e)
            {
                throw new NisreDbException(e);
            }
        }

        public async Task<IEnumerable<T>> Query<T>(string procedure, bool readOnly)
        {
            try
            {
                var cs = _connectionString;
                _connectionString.ApplicationIntent = readOnly ? ApplicationIntent.ReadOnly : ApplicationIntent.ReadWrite;
                using var db = new SqlConnection(cs.ToString());
                await db.OpenAsync();
                return await db.QueryAsync<T>(procedure, commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                var exception = new NisreDbException(ex);
                throw exception;
            }
            catch (Exception e)
            {
                throw new NisreDbException(e);
            }
        }

        public async Task<IEnumerable<T>> Query<T>(string procedure, bool readOnly, SqlParam parameter)
        {
            try
            {
                var cs = _connectionString;
                _connectionString.ApplicationIntent = readOnly ? ApplicationIntent.ReadOnly : ApplicationIntent.ReadWrite;
                using var db = new SqlConnection(cs.ToString());
                await db.OpenAsync();
                return await db.QueryAsync<T>(procedure, parameter, commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                var exception = new NisreDbException(ex, parameter);
                throw exception;
            }
            catch (Exception e)
            {
                throw new NisreDbException(e);
            }
        }

        public async Task<T> QuerySingle<T>(string procedure, bool readOnly, SqlParam parameter)
        {
            try
            {
                var cs = _connectionString;
                _connectionString.ApplicationIntent = readOnly ? ApplicationIntent.ReadOnly : ApplicationIntent.ReadWrite;
                using var db = new SqlConnection(cs.ToString());
                await db.OpenAsync();
                return await db.QuerySingleOrDefaultAsync<T>(procedure, parameter,
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                var exception = new NisreDbException(ex);
                throw exception;
            }
            catch (Exception e)
            {
                throw new NisreDbException(e);
            }
        }

        public async Task<T> QuerySingle<T>(string procedure, bool readOnly)
        {
            try
            {
                var cs = _connectionString;
                _connectionString.ApplicationIntent = readOnly ? ApplicationIntent.ReadOnly : ApplicationIntent.ReadWrite;
                using var db = new SqlConnection(cs.ToString());
                await db.OpenAsync();
                return await db.QuerySingleOrDefaultAsync<T>(procedure, commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                var exception = new NisreDbException(ex);
                throw exception;
            }
            catch (Exception e)
            {
                throw new NisreDbException(e);
            }
        }

        #endregion
    }
}