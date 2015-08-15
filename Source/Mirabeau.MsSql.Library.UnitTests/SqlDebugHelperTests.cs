﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Mirabeau.MsSql.Library.UnitTests.Properties;
using NUnit.Framework;

namespace Mirabeau.MsSql.Library.UnitTests
{
    [TestFixture]
    public class SqlDebugHelperTests
    {
        [Test]
        public void ShouldCreateExecutableQuery()
        {
            DateTime dt = new DateTime(2010, 12, 31, 23, 59, 59);
            float? nullable = null;
            decimal decimalValue = 123.456m;

            string sql =
                "Select * FROM [dbo].[TableX] Where Column1 > @value1 and Column2 = @value2 and Column3 = @value3 and Column4 = @value4 and Column5 = @value5 and Column6 = @value6 and Column7 = @value7";

            IList<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(0.CreateSqlParameter("value1"));
            parameters.Add("hello world".CreateSqlParameter("value2"));
            parameters.Add(nullable.CreateSqlParameter("value3"));
            parameters.Add(decimalValue.CreateSqlParameter("value4"));
            parameters.Add(true.CreateSqlParameter("value5"));
            var sqlParameter = 321.12m.CreateSqlParameter("value6");
            sqlParameter.Size = 10;
            sqlParameter.Precision = 2;
            parameters.Add(sqlParameter);
            parameters.Add(dt.CreateSqlParameter("value7", SqlDbType.DateTime));

            string executableSql = SqlDebugHelper.CreateExecutableSqlStatement(sql, parameters);
            Trace.WriteLine(executableSql);
            Assert.That(ReplaceNewLine(executableSql), Is.EqualTo(ReplaceNewLine(Resources.ExpectedSqlQuery)));
        }

        [Test]
        public void ShouldCreateExecutableStoredProcedure()
        {
            float? nullable = null;
            decimal decimalValue = 123.456m;
            string sql = "sp_test";

            IList<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(0.CreateSqlParameter("value1"));
            parameters.Add("hello world".CreateSqlParameter("value2"));
            parameters.Add(nullable.CreateSqlParameter("value3"));
            parameters.Add(decimalValue.CreateSqlParameter("value4"));
            

            string executableSql = SqlDebugHelper.CreateExecutableSqlStatement(sql, parameters);
            Trace.WriteLine(executableSql);
            Assert.That(executableSql, Is.EqualTo("EXEC sp_test @value1 = 0, @value2 = N'hello world', @value3 = null, @value4 = 123.456"));
        }

        [Test]
        public void ShouldFormatDatesInIso8601Format()
        {
            DateTime dt = new DateTime(2015, 12, 31, 23, 59, 10).AddMilliseconds(12345);
            var parameter = dt.CreateSqlParameter("param", SqlDbType.DateTime);

            string executableSql = SqlDebugHelper.CreateExecutableSqlStatement("my_sp", parameter);
            Trace.WriteLine(executableSql);
            Assert.That(executableSql, Is.EqualTo("EXEC my_sp @param = convert(datetime,'2015-12-31T23:59:22.3450000', 127)"));
        }

        [Test]
        public void ShouldNotUseNPrefixForVarchar()
        {
            var parameter = "test".CreateSqlParameter("param");
            parameter.SqlDbType = SqlDbType.VarChar;

            string executableSql = SqlDebugHelper.CreateExecutableSqlStatement("my_sp", parameter);
            Trace.WriteLine(executableSql);
            Assert.That(executableSql, Is.EqualTo("EXEC my_sp @param = 'test'"));
        }

        [Test]
        public void ShouldHandleParameterlessQuery()
        {
            var query = "select * from my_table";
            string executableSql = SqlDebugHelper.CreateExecutableSqlStatement(query, CommandType.Text, null);

            Assert.That(executableSql, Is.EqualTo(query));
        }

        [Test]
        public void ShouldHandleParameterlessStoredProcedure()
        {
            var query = "my_sp";
            string executableSql = SqlDebugHelper.CreateExecutableSqlStatement(query, CommandType.StoredProcedure, null);

            Assert.That(executableSql, Is.EqualTo("EXEC my_sp "));
        }

        [Test]
        public void ShouldIgnoreNullValuedSqlParameters()
        {
            var list = new List<SqlParameter>() {null, null, null};
            string sp = SqlDebugHelper.CreateExecutableSqlStatement("sp", list);

            Assert.That(sp, Is.EqualTo("EXEC sp "));
        }

        [Test]
        public void ShouldThrowExceptionIfSqlIsNull()
        {
            Assert.That(() => SqlDebugHelper.CreateExecutableSqlStatement(null, null),
                Throws.Exception.TypeOf<ArgumentNullException>());

            Assert.That(() => SqlDebugHelper.CreateExecutableSqlStatement(null, CommandType.StoredProcedure, null),
                Throws.Exception.TypeOf<ArgumentNullException>());
        }

        private static string ReplaceNewLine(string value)
        {
            return value.Replace(@"\n", " ").Replace(@"\r\n", " ");
        }
    }
}
