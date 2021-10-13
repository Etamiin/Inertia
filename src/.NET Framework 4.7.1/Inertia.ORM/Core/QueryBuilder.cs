using MySql.Data.MySqlClient;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Inertia.ORM
{
    internal static class QueryBuilder
    {
        internal static string GetCreateQuery(Table table)
        {
            return UseBuilder($"CREATE TABLE IF NOT EXISTS `{ table.Identifier }` (", null, (sb) => {
                var primaryKeys = new StringBuilder();
                var fields = Table.GetFields(table.GetType());

                for (var i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    var fieldType = FieldType.GetFieldType(field.FieldType);
                    var varChar = field.GetCustomAttribute<VarChar>();
                    var isNotNull = field.GetCustomAttribute<NotNull>();
                    var isPrimaryKey = field.GetCustomAttribute<PrimaryKey>();

                    if (i > 0)
                    {
                        sb.Append(",");
                    }

                    sb.Append($"`{ field.Name }` ");

                    if (fieldType.Code == TypeCode.String && varChar != null)
                    {
                        sb.Append($"VARCHAR({ varChar.Length })");
                    }
                    else
                    {
                        sb.Append($"{ fieldType.SqlType.ToUpper() }{(fieldType.Unsigned ? " UNSIGNED" : string.Empty)}");
                        if (fieldType.Code == TypeCode.Decimal)
                        {
                            var precision = field.GetCustomAttribute<DecimalPrecision>();
                            if (precision != null)
                            {
                                sb.Append($"({ precision.FieldPrecision },{ precision.FieldScale })");
                            }
                        }
                    }

                    if (isNotNull != null)
                    {
                        sb.Append(" NOT NULL");
                        if (isNotNull.Unique)
                        {
                            sb.Append(" UNIQUE");
                        }
                    }

                    if (isPrimaryKey != null)
                    {
                        if (primaryKeys.Length > 0)
                        {
                            primaryKeys.Append(",");
                        }

                        primaryKeys.Append($"`{ field.Name }`");
                        if (isPrimaryKey.AutoIncrement)
                        {
                            sb.Append(" AUTO_INCREMENT");
                        }
                    }
                }

                if (primaryKeys.Length > 0)
                {
                    sb.Append($", primary key({ primaryKeys })");
                }

                sb.Append(")");
            });
        }
        internal static string GetInsertQuery(Table table, MySqlCommand command)
        {
            return UseBuilder($"INSERT INTO `{ table.Identifier }`", null, (sb) => {
                var fields = Table.GetFields(table.GetType());
                var names = new StringBuilder();
                var _params = new StringBuilder();
                var iparam = 0;

                for (var i = 0; i < fields.Length; i++)
                {
                    if (i > 0)
                    {
                        names.Append(",");
                        _params.Append(",");
                    }

                    var field = fields[i];
                    var pname = $"@{ iparam++ }";

                    names.Append(field.Name);
                    _params.Append(pname);

                    command.Parameters.AddWithValue(pname, field.GetValue(table));
                }

                sb.Append($" ({ names }) VALUES ({ _params })");
            });            
        }
        internal static string GetSelectQuery(Table table, SqlCondition condition, string[] columnsToSelect, bool distinct)
        {
            return UseBuilder($"SELECT { (distinct ? "DISTINCT " : string.Empty) }", condition, (sb) => {
                if (columnsToSelect.Length > 0)
                {
                    sb.Append(string.Join(",", columnsToSelect));
                }
                else
                {
                    sb.Append("*");
                }

                sb.Append($" FROM `{ table.Identifier }`");
            });
        }
        internal static string GetDeleteQuery(Table table, SqlCondition condition)
        {
            return UseBuilder($"DELETE FROM `{ table.Identifier }`", condition, (sb) => { });
        }
        internal static string GetUpdateQuery(Table table, MySqlCommand command, SqlCondition condition, string[] columns)
        {
            var fields = Table.GetFields(table.GetType());
            var iParam = condition == null ? 0 : condition.ParamIndex + 1;

            return UseBuilder($"UPDATE `{ table.Identifier }` SET ", condition, (sb) => {
                var cCount = columns.Length;
                if (cCount == 0)
                {
                    cCount--;
                }

                for (var i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    if (field.GetCustomAttribute<PrimaryKey>() != null)
                    {
                        continue;
                    }
                    if (cCount > 0)
                    {
                        if (!columns.Contains(field.Name))
                        {
                            continue;
                        }
                        else
                        {
                            cCount--;
                        }
                    }

                    var pName = $"@{ iParam++ }";

                    command.Parameters.AddWithValue(pName, field.GetValue(table));
                    sb.Append($"{ field.Name }={ pName }");

                    if (cCount == 0)
                    {
                        break;
                    }
                    if (i < fields.Length - 1)
                    {
                        sb.Append(", ");
                    }
                }
            });
        }
        internal static string GetDropQuery(Table table)
        {
            return UseBuilder($"DROP TABLE IF EXISTS `{ table.Identifier }`", null, null);
        }
        internal static string GetCountQuery(Table table, string colName, SqlCondition condition, bool distinct)
        {
            return UseBuilder($"SELECT COUNT({ (distinct ? "DISTINCT " : string.Empty) }{ (string.IsNullOrEmpty(colName) ? "*" : colName) }) FROM `{ table.Identifier }`", condition, null);
        }
        internal static string GetAvgQuery(Table table, string colName, SqlCondition condition)
        {
            return UseBuilder($"SELECT AVG({ colName }) FROM `{ table.Identifier }`", condition, null);
        }
        internal static string GetMaxQuery(Table table, string colName, SqlCondition condition)
        {
            return UseBuilder($"SELECT MAX({ colName }) FROM `{ table.Identifier }`", condition, null);
        }
        internal static string GetMinQuery(Table table, string colName, SqlCondition condition)
        {
            return UseBuilder($"SELECT MIN({ colName }) FROM `{ table.Identifier }`", condition, null);
        }
        internal static string GetSumQuery(Table table, string colName, SqlCondition condition)
        {
            return UseBuilder($"SELECT SUM({ colName }) FROM `{ table.Identifier }`", condition, null);
        }

        private static string UseBuilder(string baseStr, SqlCondition condition, BasicAction<StringBuilder> onBuilder)
        {
            var sb = new StringBuilder(baseStr);
            onBuilder?.Invoke(sb);

            if (condition != null)
            {
                sb.Append($" { condition.GetQuery() }");
            }

            return sb.ToString();
        }
    }
}