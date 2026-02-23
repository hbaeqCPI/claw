using System;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace R10.Core.Helpers
{
    public class ReportSchedulerHelper
    {

        #region "SQL Utils"
        private readonly IConfiguration _configuration;

        public ReportSchedulerHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetServerName()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(
                       connectionString))
            {
                string servername = connection.DataSource.ToString();
                //if (servername.Contains("."))
                //    return servername.Substring(0, servername.IndexOf("."));
                //else
                    return servername;
            }
        }

        public string GetServerName(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(
                       connectionString))
            {
                string servername = connection.DataSource.ToString();
                //if (servername.Contains("."))
                //    return servername.Substring(0, servername.IndexOf("."));
                //else
                    return servername;
            }
        }


        public string GetDatabaseName()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(
                       connectionString))
            {
                return connection.Database.ToString();
            }
        }

        public string GetDatabaseName(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(
                       connectionString))
            {
                return connection.Database.ToString();
            }
        }

        #endregion



        public object CPiExecute(string strClassName, string strMethodName, params object[] objParams)
        {
            // objParams must be an Array consisting of the actual parameters of the method
            Type svcType = Type.GetType(strClassName);
            //tplDataSource.TypeName & ", CPi.Business.Services"
            object svc = Activator.CreateInstance(svcType);
            List<System.Type> lstTyp = new List<System.Type>();
            foreach (object obj in objParams)
            {
                lstTyp.Add(obj.GetType());
            }
            MethodInfo method = svcType.GetMethod(strMethodName, lstTyp.ToArray(), null);
            //tplDataSource.SelectMethod
            if (method != null)
            {
                return method.Invoke(svc, BindingFlags.InvokeMethod, null, objParams, System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                return null;
            }
        }

        public DateTime GetFixedFromDate(string fixedRange)
        {
            DateTime today = DateTime.Now;
            switch (fixedRange)
            {
                case "0": // 'This Week 
                    int dayOfWeek = (int)today.DayOfWeek;
                    return today.AddDays(-(int)today.DayOfWeek);
                case "1": // This Month
                    return new DateTime(today.Year, today.Month, 1);
                case "2": // This Year
                    return new DateTime(today.Year, 1, 1);
                default:
                    return DateTime.MinValue;
            }
        }

        public DateTime GetFixedToDate(DateTime fromDate, string fixedRange)
        {
            DateTime today = DateTime.Now.Date;
            switch (fixedRange)
            {
                case "0": // 'This Week                     
                    return fromDate.AddDays(6);
                case "1": // This Month
                    return new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
                case "2": // This Year
                    return new DateTime(today.Year, 12, 31);
                default:
                    return DateTime.MinValue;
            }
        }

        public DateTime GetRelativeFromDate(string dateOperator, int dateOffset, string dateUnit)
        {
            DateTime today = DateTime.Now.Date;
            dateOffset = dateOffset * (dateOperator == "-" ? -1 : 1);
            switch (dateUnit)
            {
                case "0":
                    return today.AddDays(dateOffset);
                case "1":
                    return today.AddDays(dateOffset * 7);
                case "2":
                    return today.AddMonths(dateOffset);
                case "3":
                    return today.AddYears(dateOffset);
                default:
                    return DateTime.MinValue;
            }
        }

        public DateTime GetRelativeToDate(DateTime fromDate, int dateOffset, string dateUnit)
        {
            switch (dateUnit)
            {
                case "0":
                    return fromDate.AddDays(dateOffset);
                case "1":
                    return fromDate.AddDays(dateOffset * 7);
                case "2":
                    return fromDate.AddMonths(dateOffset);
                case "3":
                    return fromDate.AddYears(dateOffset);
                default:
                    return DateTime.MinValue;
            }
        }
    }
}
