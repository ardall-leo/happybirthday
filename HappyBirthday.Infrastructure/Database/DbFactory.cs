using HappyBirthday.Domain;
using HappyBirthday.Domain.Interfaces;
using HappyBirthday.Domain.Models.Configs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyBirthday.Infrastructure.Database
{
    public class DbFactory : IDbConnectionFactory
    {
        private readonly AppConfig _config;

        public DbFactory(AppConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// It returns new SQL connection, but under the hood ADO.NET will return the existing open connection
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public IDbConnection GetConnection(DatabaseType dbType)
        {
            return dbType switch
            {
                DatabaseType.Hbd => new SqlConnection(_config.DbConnStr),
                _ => throw new NotSupportedException("Repository not supported")
            };
        }
    }
}
