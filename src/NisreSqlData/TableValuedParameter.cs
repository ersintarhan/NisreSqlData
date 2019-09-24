using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Dapper;
using Microsoft.SqlServer.Server;

namespace NisreSqlData
{
    public static class TableValuedParameter
    {
        private static readonly Dictionary<Type, SqlDbType> Types = new Dictionary<Type, SqlDbType>
        {
            {typeof(bool), SqlDbType.Bit},
            {typeof(bool?), SqlDbType.Bit},
            {typeof(byte), SqlDbType.TinyInt},
            {typeof(byte?), SqlDbType.TinyInt},
            {typeof(string), SqlDbType.NVarChar},
            {typeof(DateTime), SqlDbType.DateTime2},
            {typeof(DateTime?), SqlDbType.DateTime2},
            {typeof(short), SqlDbType.SmallInt},
            {typeof(short?), SqlDbType.SmallInt},
            {typeof(int), SqlDbType.Int},
            {typeof(int?), SqlDbType.Int},
            {typeof(long), SqlDbType.BigInt},
            {typeof(long?), SqlDbType.BigInt},
            {typeof(decimal), SqlDbType.Decimal},
            {typeof(decimal?), SqlDbType.Decimal},
            {typeof(double), SqlDbType.Float},
            {typeof(double?), SqlDbType.Float},
            {typeof(float), SqlDbType.Real},
            {typeof(float?), SqlDbType.Real},
            {typeof(TimeSpan), SqlDbType.Time},
            {typeof(Guid), SqlDbType.UniqueIdentifier},
            {typeof(Guid?), SqlDbType.UniqueIdentifier},
            {typeof(byte[]), SqlDbType.Binary},
            {typeof(byte?[]), SqlDbType.Binary},
            {typeof(char[]), SqlDbType.Char},
            {typeof(char?[]), SqlDbType.Char}
        };

        private static readonly ConcurrentDictionary<Type, SqlMetaData[]> Tipler = new ConcurrentDictionary<Type, SqlMetaData[]>();
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Props = new ConcurrentDictionary<Type, PropertyInfo[]>();

        public static SqlDbType GetSqlDbType(this Type systype)
        {
            Types.TryGetValue(systype, out var resulttype);
            return resulttype;
        }

        public static IEnumerable<SqlDataRecord> ConvertToTvp<T>(this IEnumerable<T> data) where T : new()
        {
            SqlMetaData[] record;
            PropertyInfo[] properties;

            if (Tipler.ContainsKey(typeof(T)))
            {
                record = Tipler[typeof(T)];
                if (Props.ContainsKey(typeof(T)))
                {
                    properties = Props[typeof(T)];
                }
                else
                {
                    properties = typeof(T).GetTypeInfo().GetProperties();
                    Props.TryAdd(typeof(T), properties);
                }
            }
            else
            {
                var records = new List<SqlMetaData>();
                if (Props.ContainsKey(typeof(T)))
                {
                    properties = Props[typeof(T)];
                }
                else
                {
                    properties = typeof(T).GetTypeInfo().GetProperties();
                    Props.TryAdd(typeof(T), properties);
                }

                foreach (var prop in properties)
                {
                    var pt = prop.PropertyType;
                    var sdbtyp = prop.PropertyType.GetSqlDbType();
                    records.Add(pt.Name.Equals("String")
                        ? new SqlMetaData(prop.Name, sdbtyp, 4000)
                        : new SqlMetaData(prop.Name, sdbtyp));
                }

                record = records.ToArray();
                Tipler.TryAdd(typeof(T), record);
            }

            var ret = new SqlDataRecord(record);
            foreach (var d in data)
            {
                for (var i = 0; i < properties.Length; i++)
                {
                    var im = properties[i];
                    ret.SetValue(i, properties[i].GetValue(d, null));
                }

                yield return ret;
            }
        }

        public static SqlMapper.ICustomQueryParameter AsTableValuedParameter<T>(this IEnumerable<T> enumerable,
            string typeName) where T : new()
        {

            var table = enumerable.ConvertToTvp();
            return new Tvp(table, typeName);
        }

        
    }
}