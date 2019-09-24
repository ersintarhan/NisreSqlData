using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace NisreSqlData
{
    public class SqlParam : SqlMapper.IDynamicParameters
    {
        private static readonly Dictionary<SqlMapper.Identity, Action<IDbCommand, object>> ParamReaderCache =
            new Dictionary<SqlMapper.Identity, Action<IDbCommand, object>>();

        private readonly Dictionary<string, ParamInfo> _parameters = new Dictionary<string, ParamInfo>();
        private List<object> _templates;

        /// <summary>
        ///     construct a dynamic parameter bag
        /// </summary>
        public SqlParam()
        {
        }

        /// <summary>
        ///     construct a dynamic parameter bag
        /// </summary>
        /// <param name="template">can be an anonymous type or a DynamicParameters bag</param>
        public SqlParam(object template)
        {
            AddDynamicParams(template);
        }

        /// <summary>
        ///     All the names of the param in the bag, use Get to yank them out
        /// </summary>
        public IEnumerable<string> ParameterNames
        {
            get { return _parameters.Select(p => p.Key); }
        }

        void SqlMapper.IDynamicParameters.AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            AddParameters(command, identity);
        }


        public Dictionary<string, object> GetParamatersInfo()
        {
            var res = new Dictionary<string, object>();
            foreach (var p in _parameters)
                if (p.Key != "p_password")
                    res.Add(p.Key, p.Value.Value);
            return res;
        }

        /// <summary>
        ///     Append a whole object full of params to the dynamic
        ///     EG: AddDynamicParams(new {A = 1, B = 2}) // will add property A and B to the dynamic
        /// </summary>
        /// <param name="param"></param>
        public void AddDynamicParams(dynamic param)
        {
            if (param is object obj)
            {
                if (!(obj is SqlParam subDynamic))
                {
                    if (!(obj is IEnumerable<KeyValuePair<string, object>> dictionary))
                    {
                        _templates ??= new List<object>();
                        _templates.Add(obj);
                    }
                    else
                    {
                        foreach (var kvp in dictionary) Add(kvp.Key, kvp.Value);
                    }
                }
                else
                {
                    if (subDynamic._parameters != null)
                        foreach (var kvp in subDynamic._parameters)
                            _parameters.Add(kvp.Key, kvp.Value);

                    if (subDynamic._templates != null)
                    {
                        _templates ??= new List<object>();
                        foreach (var t in subDynamic._templates) _templates.Add(t);
                    }
                }
            }
        }

        /// <summary>
        ///     Add a parameter to this dynamic parameter list
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        /// <param name="direction"></param>
        /// <param name="size"></param>
        public void Add(string name, object value = null, SqlDbType? dbType = null,ParameterDirection? direction = null, int? size = null)
        {
            _parameters[name] = new ParamInfo
            {
                Name = name, Value = value,
                ParameterDirection = direction ?? ParameterDirection.Input,
                DbType = dbType,
                Size = size
            };
        }


        /// <summary>
        ///     Add all the parameters needed to the command just before it executes
        /// </summary>
        /// <param name="command">The raw command prior to execution</param>
        /// <param name="identity">Information about the query</param>
        protected void AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            if (_templates != null)
                foreach (var template in _templates)
                {
                    var newIdent = identity.ForDynamicParameters(template.GetType());
                    Action<IDbCommand, object> appender;
                    lock (ParamReaderCache)
                    {
                        if (!ParamReaderCache.TryGetValue(newIdent, out appender))
                        {
                            appender = SqlMapper.CreateParamInfoGenerator(newIdent, false, true);
                            ParamReaderCache[newIdent] = appender;
                        }
                    }

                    appender(command, template);
                }

            foreach (var param in _parameters.Values)
            {
                var add = !((SqlCommand) command).Parameters.Contains(param.Name);
                SqlParameter p;
                if (add)
                {
                    p = ((SqlCommand) command).CreateParameter();
                    p.ParameterName = param.Name;
                }
                else
                {
                    p = ((SqlCommand) command).Parameters[param.Name];
                }

                var val = param.Value;
                p.Value = val ?? DBNull.Value;
                p.Direction = param.ParameterDirection;
                if (param.Size != null) p.Size = param.Size.Value;
                if (param.DbType != null) p.SqlDbType = param.DbType.Value;
                if (add) command.Parameters.Add(p);
                param.AttachedParam = p;
            }
        }

        /// <summary>
        ///     Get the value of a parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns>The value, note DBNull.Value is not returned, instead the value is returned as null</returns>
        public T Get<T>(string name)
        {
            var val = _parameters[name].AttachedParam.Value;
            if (val == DBNull.Value)
            {
                if (default(T) != null)
                    throw new ApplicationException("Attempting to cast a DBNull to a non nullable type!");
                return default;
            }

            return (T) val;
        }

        protected class ParamInfo
        {
            public string Name { get; set; }

            public object Value { get; set; }

            public ParameterDirection ParameterDirection { get; set; }

            public SqlDbType? DbType { get; set; }

            public int? Size { get; set; }

            public IDbDataParameter AttachedParam { get; set; }
        }
    }
}