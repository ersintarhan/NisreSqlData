using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Dapper;
using Microsoft.SqlServer.Server;

namespace NisreSqlData
{
    public class Tvp : SqlMapper.ICustomQueryParameter
    {
        private static readonly Action<SqlParameter, string> SetTypeName;
        private readonly IEnumerable<SqlDataRecord> _table;
        private readonly string _typeName;

        
        public Tvp(IEnumerable<SqlDataRecord> table, string typeName)
        {
            this._table = table;
            this._typeName = typeName;
        }


        void SqlMapper.ICustomQueryParameter.AddParameter(IDbCommand command, string name)
        {
            var param = command.CreateParameter();
            param.ParameterName = name;
            Set(param, this._table, this._typeName);
            command.Parameters.Add(param);
        }

        private static void Set(IDataParameter parameter, IEnumerable<SqlDataRecord> table, string typeName)
        {
            parameter.Value = table;
            //parameter.Value = SqlMapper.SanitizeParameterValue(table);
            if (parameter is SqlParameter sqlParam)
            {
                SetTypeName?.Invoke(sqlParam, typeName);
                sqlParam.SqlDbType = SqlDbType.Structured;
            }
        }
    }
}
