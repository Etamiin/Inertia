using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inertia.ORM
{
    public abstract class Database
    {
        /// <summary>
        /// Returns the name of the database.
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Returns the username used for the authentification.
        /// </summary>
        public abstract string User { get; }
        /// <summary>
        /// Returns the password used for the authentification.
        /// </summary>
        public abstract string Password { get; }
        /// <summary>
        /// Returns the ip used for the connection.
        /// </summary>
        public abstract string Host { get; }
        public abstract bool AutoGenerateTable { get; }

        /// <summary>
        /// Returns the port used for the connection.
        /// </summary>
        public virtual int Port { get; } = 3306;
        /// <summary>
        /// Returns the SSL mode used by the MySql connection
        /// </summary>
        public virtual MySqlSslMode Ssl { get; } = MySqlSslMode.None;

        internal bool IsInitialized { get; set; }

        private readonly string _connectionString;

        /// <summary>
        /// Initialize a new instance of class <see cref="Database"/>
        /// </summary>
        protected Database()
        {
            _connectionString = $"server={ Host.Replace("localhost", "127.0.0.1") };uid={ User };pwd={ Password };database={ Name };port={ Port };SslMode={ Ssl }";
        }

        public abstract void OnInitialized();

        internal void TryCreateItSelf()
        {
            var dbCreaStr = $"server={ Host.Replace("localhost", "127.0.0.1") };uid={ User };pwd={ Password };port={ Port };SslMode={ Ssl }";
            using (var conn = new MySqlConnection(dbCreaStr))
            {
                conn.Open();
                using (var cmd = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{ Name }`", conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal MySqlConnection CreateConnection()
        {
            try
            {
                var conn = new MySqlConnection(_connectionString);
                conn.Open();

                return conn;
            }
            catch (Exception ex)
            {
                throw new DatabaseConnectionFailedException(this, ex);
            }
        }

        internal void ExecuteCommand(string query, BasicAction<MySqlCommand> onCommand, bool force = false)
        {
            if (IsInitialized || force)
            {
                using (var conn = CreateConnection())
                {
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        onCommand(cmd);
                    }
                }
            }
        }
        internal void ExecuteCommand(BasicAction<MySqlCommand> onCommand, bool force = false)
        {
            if (IsInitialized || force)
            {
                using (var conn = CreateConnection())
                {
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        onCommand(cmd);
                    }
                }
            }
        }

        /// <summary>
        /// Execute a custom SQL query and returns true if the query was successfully executed.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public bool TryExecuteQuery(string query)
        {
            try
            {
                ExecuteCommand(query, (cmd) => {
                    cmd.ExecuteNonQuery();
                });

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Create the specified <see cref="Table"/> and returns true if the creation (or if the table already exists) was a success.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Create<T>() where T : Table
        {
            if (SqlManager.CreateTableInstance(out T table))
            {
                return Create(table);
            }
            else
            { 
                return false;
            }
        }
        /// <summary>
        /// Drop the specified <see cref="Table"/> and returns true if the deletion was a success.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Drop<T>() where T : Table
        {
            if (SqlManager.CreateTableInstance(out T table))
            {
                ExecuteCommand(QueryBuilder.GetDropQuery(table), (cmd) => {
                    cmd.ExecuteNonQuery();
                });

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Selects the first element from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnsToSelect"></param>
        /// <returns></returns>
        public T Select<T>(params string[] columnsToSelect) where T : Table
        {
            return Select<T>(null, false, columnsToSelect);
        }
        /// <summary>
        /// Selects the first element from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnsToSelect"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public T Select<T>(bool distinct, params string[] columnsToSelect) where T : Table
        {
            return Select<T>(null, distinct, columnsToSelect);
        }
        /// <summary>
        /// Selects the first element from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnsToSelect"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public T Select<T>(SqlCondition condition, params string[] columnsToSelect) where T : Table
        {
            return Select<T>(condition, false, columnsToSelect);
        }
        /// <summary>
        /// Selects the first element from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnsToSelect"></param>
        /// <param name="condition"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public T Select<T>(SqlCondition condition, bool distinct, params string[] columnsToSelect) where T : Table
        {
            T table = null;

            ExecuteCommand((cmd) => {
                if (SqlManager.CreateTableInstance(out table))
                {
                    if (condition == null)
                    {
                        condition = new SqlCondition();
                    }

                    condition.Limit(1);

                    var fields = Table.GetFields<T>();

                    cmd.SetQuery(QueryBuilder.GetSelectQuery(table, condition, columnsToSelect, distinct), condition);
                    cmd.OnReader((reader) => {
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            var field = fields.FirstOrDefault((f) => f.Name == reader.GetName(i));
                            if (field != null)
                            {
                                object value = null;
                                if (field.FieldType == typeof(bool))
                                {
                                    value = reader.GetBoolean(i);
                                }
                                else if (!reader.IsDBNull(i))
                                {
                                    value = reader.GetValue(i);
                                }

                                field.SetValue(table, value);
                            }
                        }
                    });
                }
            });

            return table;
        }

        /// <summary>
        /// Selects all the elements from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnsToSelect"></param>
        /// <returns></returns>
        public T[] SelectAll<T>(params string[] columnsToSelect) where T : Table
        {
            return SelectAll<T>(null, false, columnsToSelect);
        }
        /// <summary>
        /// Selects all the elements from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnsToSelect"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public T[] SelectAll<T>(bool distinct, params string[] columnsToSelect) where T : Table
        {
            return SelectAll<T>(null, distinct, columnsToSelect);
        }
        /// <summary>
        /// Selects all the elements from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnsToSelect"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public T[] SelectAll<T>(SqlCondition condition, params string[] columnsToSelect) where T : Table
        {
            return SelectAll<T>(condition, false, columnsToSelect);
        }
        /// <summary>
        /// Selects all the elements from the specified <see cref="Table"/> with the specified parameters and returns an instance of the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnsToSelect"></param>
        /// <param name="condition"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public T[] SelectAll<T>(SqlCondition condition, bool distinct, params string[] columnsToSelect) where T : Table
        {
            var result = new T[0];

            ExecuteCommand((cmd) => {
                if (SqlManager.CreateTableInstance(out T table))
                {
                    var tables = new List<T>();
                    var fields = Table.GetFields<T>();

                    cmd.SetQuery(QueryBuilder.GetSelectQuery(table, condition, columnsToSelect, distinct), condition);
                    cmd.OnReader((reader) => {
                        var created = SqlManager.CreateTableInstance(out T instance);
                        if (created)
                        {
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                var field = fields.FirstOrDefault((f) => f.Name == reader.GetName(i));
                                if (field != null)
                                {
                                    object value = null;
                                    if (field.FieldType == typeof(bool))
                                    {
                                        value = reader.GetBoolean(i);
                                    }
                                    else if (!reader.IsDBNull(i))
                                    {
                                        value = reader.GetValue(i);
                                    }

                                    field.SetValue(instance, value);
                                }
                            }
                        }

                        tables.Add(instance);
                    });

                    result = tables.ToArray();
                }
            });

            return result;
        }

        /// <summary>
        /// Delete all the elements from the specified <see cref="Table"/> based on the specified <see cref="SqlCondition"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <returns></returns>
        public bool Delete<T>(SqlCondition condition) where T : Table
        {
            var deleted = false;

            ExecuteCommand((cmd) => {
                if (SqlManager.CreateTableInstance(out T table))
                {
                    cmd.SetQuery(QueryBuilder.GetDeleteQuery(table, condition), condition);
                    deleted = cmd.ExecuteNonQuery() > 0;
                }
            });

            return deleted;
        }
        /// <summary>
        /// Delete all the elements from the specified <see cref="Table"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool DeleteAll<T>() where T : Table
        {
            var deleted = false;

            ExecuteCommand((cmd) => {
                if (SqlManager.CreateTableInstance(out T table))
                {
                    cmd.SetQuery(QueryBuilder.GetDeleteQuery(table, null));
                    deleted = cmd.ExecuteNonQuery() > 0;
                }
            });

            return deleted;
        }

        /// <summary>
        /// Update all the elements in the specified <see cref="Table"/> with the reference's values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reference"></param>
        /// <param name="columnsToUpdate"></param>
        /// <returns></returns>
        public bool UpdateAll<T>(T reference, params string[] columnsToUpdate) where T : Table
        {
            if (reference != null)
            {
                var updated = false;

                ExecuteCommand((cmd) => {
                    cmd.SetQuery(QueryBuilder.GetUpdateQuery(reference, cmd, null, columnsToUpdate));
                    updated = cmd.ExecuteNonQuery() > 0;
                });

                return updated;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public long Count<T>() where T : Table
        {
            return Count<T>(string.Empty, null, false);
        }
        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public long Count<T>(bool distinct) where T : Table
        {
            return Count<T>(string.Empty, null, distinct);
        }
        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <returns></returns>
        public long Count<T>(SqlCondition condition) where T : Table
        {
            return Count<T>(string.Empty, condition, false);
        }
        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public long Count<T>(SqlCondition condition, bool distinct) where T : Table
        {
            return Count<T>(string.Empty, condition, distinct);
        }
        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public long Count<T>(string columnName) where T : Table
        {
            return Count<T>(columnName, null, false);
        }
        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="distinct"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public long Count<T>(string columnName, bool distinct) where T : Table
        {
            return Count<T>(columnName, null, distinct);
        }
        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public long Count<T>(string columnName, SqlCondition condition) where T : Table
        {
            return Count<T>(columnName, condition, false);
        }
        /// <summary>
        /// Execute a COUNT query with the specified parameters and return the result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="distinct"></param>
        /// <param name="columnName"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public long Count<T>(string columnName, SqlCondition condition, bool distinct) where T : Table
        {
            long count = 0;

            ExecuteCommand((cmd) => {
                if (SqlManager.CreateTableInstance(out T table))
                {
                    cmd.SetQuery(QueryBuilder.GetCountQuery(table, columnName, condition, distinct), condition);
                    cmd.OnReader((reader) => {
                        count = reader.GetInt64(0);
                    });
                }

            });

            return count;
        }

        /// <summary>
        /// Return true if a row exist in the database based on the specified conditions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <returns></returns>
        public bool Exist<T>(SqlCondition condition) where T : Table
        {
            return Count<T>(condition, false) > 0;
        }
        /// <summary>
        /// Return true if a row exist in the database based on the specified conditions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <param name="distinct"></param>
        /// <returns></returns>
        public bool Exist<T>(SqlCondition condition, bool distinct) where T : Table
        {
            return Count<T>(condition, distinct) > 0;
        }

        public long Average<T>(string columnName) where T : Table
        {
            return Average<T>(columnName, null);
        }
        public long Average<T>(string columnName, SqlCondition condition) where T : Table
        {
            long avg = -1;

            ExecuteCommand((cmd) => {
                if (SqlManager.CreateTableInstance(out T table))
                {
                    cmd.SetQuery(QueryBuilder.GetAvgQuery(table, columnName, condition), condition);
                    cmd.OnReader((reader) => {
                        avg = reader.GetInt64(0);
                    });
                }
            });

            return avg;
        }

        public T Max<T>(string columnName) where T : Table
        {
            return Max<T>(columnName, null);
        }
        public T Max<T>(string columnName, SqlCondition condition) where T : Table
        {
            T table = null;

            ExecuteCommand((cmd) => {
                if (!SqlManager.CreateTableInstance(out table))
                {
                    cmd.SetQuery(QueryBuilder.GetMaxQuery(table, columnName, condition), condition);
                    cmd.OnReader((reader) => {
                        var field = Table.GetFields<T>().FirstOrDefault((f) => f.Name == columnName);
                        if (field != null)
                        {
                            field.SetValue(table, reader.GetValue(0));
                        }
                    });
                }                
            });

            return table;
        }
        
        public T Min<T>(string columnName) where T : Table
        {
            return Min<T>(columnName, null);
        }
        public T Min<T>(string columnName, SqlCondition condition) where T : Table
        {
            T table = null;

            ExecuteCommand((cmd) => {
                if (SqlManager.CreateTableInstance(out table))
                {
                    cmd.SetQuery(QueryBuilder.GetMinQuery(table, columnName, condition), condition);
                    cmd.OnReader((reader) => {
                        var field = Table.GetFields<T>().FirstOrDefault((f) => f.Name == columnName);
                        if (field != null)
                        {
                            field.SetValue(table, reader.GetValue(0));
                        }
                    });
                }
            });

            return table;
        }

        public decimal Sum<T>(string columnName) where T : Table
        {
            return Sum<T>(columnName, null);
        }
        public decimal Sum<T>(string columnName, SqlCondition condition) where T : Table
        {
            decimal sum = -1;
            ExecuteCommand((cmd) => {
                if (SqlManager.CreateTableInstance(out T table))
                {
                    cmd.SetQuery(QueryBuilder.GetSumQuery(table, columnName, condition), condition);
                    cmd.OnReader((reader) => {
                        if (!reader.IsDBNull(0))
                        {
                            sum = reader.GetDecimal(0);
                        }
                    });
                }
            });

            return sum;
        }

        internal bool Create(Table table)
        {
            if (table != null)
            {
                ExecuteCommand(QueryBuilder.GetCreateQuery(table), (cmd) => {
                    cmd.ExecuteNonQuery();
                }, force: true);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
