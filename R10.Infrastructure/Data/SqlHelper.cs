using System.Data;
using Microsoft.Data.SqlClient;
using System.Reflection;

namespace R10.Infrastructure.Data
{
    public static class SqlHelper
    {
        public static void FillParamValues(this SqlCommand command, object parameters)
        {
            Type type = parameters.GetType();

            foreach (SqlParameter param in command.Parameters)
            {
                if (param.ParameterName.ToLower() != "@return_value")
                {
                    var columnName = param.ParameterName.Substring(1);
                    var property = type.GetProperty(columnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (property != null)
                    {
                        if (property.PropertyType == typeof(string) && property.GetValue(parameters, null) != null)
                        {                            
                                param.Value = property.GetValue(parameters, null).ToString().Replace("*", "%").Replace("?", "_").Replace("[", "[[]");
                                continue;                           
                        }
                        param.Value = property.GetValue(parameters, null);

                    }
                }
            }
        }

        public static string BuildSql<T>(string storedProcName, T entity)
        {
            string sql = storedProcName;
            foreach (PropertyInfo property in entity.GetType().GetProperties())
            {
                sql = sql + " @" + property.Name + ",";
            }
            return sql.Substring(0, sql.Length - 1);
        }

        public static List<SqlParameter> BuildSqlParameters<T>(T entity)
        {
            var parameters = new List<SqlParameter>();

            foreach (PropertyInfo property in entity.GetType().GetProperties())
            {
                var value = property.GetValue(entity);
                var parameter = new SqlParameter
                {
                    ParameterName = "@" + property.Name
                };

                if (property.PropertyType == typeof(DateTime?))
                {
                    parameter.SqlDbType = SqlDbType.DateTime;
                }
                else if ((property.PropertyType == typeof(Int32?)) || (property.PropertyType == typeof(Int32)))
                {
                    parameter.SqlDbType = SqlDbType.Int;
                }
                else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
                {
                    parameter.SqlDbType = SqlDbType.Bit;
                }
                else
                {
                    parameter.SqlDbType = SqlDbType.NVarChar;

                    if (value != null)
                    { 
                        parameter.Size = value.ToString().Length;
                        value = value.ToString().Replace("*", "%").Replace("?", "_").Replace("[", "[[]");
                    }
                }

                parameter.Value = value ?? DBNull.Value;
                parameters.Add(parameter);
            }

            return parameters;
        }

        public static string JsonValue(string column, string path)
        {
            throw new NotSupportedException();
        }
    }
}
