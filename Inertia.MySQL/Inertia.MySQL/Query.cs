using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.MySQL
{
    public static class Query
    {
        public abstract class QueryBase : IDisposable
        {
            public bool HasCondition
            {
                get
                {
                    return Conditions.Count > 0;
                }
            }
            public bool HasLimit
            {
                get
                {
                    return Limit > 0;
                }
            }

            protected readonly string Table;
            protected readonly MySqlCommand Command;

            private List<string> Conditions;
            private readonly uint Limit = 0;

            public QueryBase(string Table) : this(Table, 0)
            {
            }
            public QueryBase(string Table, uint Limit)
            {
                this.Table = Table;
                this.Limit = Limit;
                Command = MySqlModule.Module.CreateCommand();
                Conditions = new List<string>();
            }

            public QueryBase AddCondition(string field, object value, QueryConditionType conditionType = QueryConditionType.AND)
            {
                var _id = "@" + field + conditionType.ToString();
                var _cond = field + "=" + _id;
                if (HasCondition)
                    _cond += " " + conditionType.ToString() + " ";

                Command?.Parameters.AddWithValue(_id, value);
                Conditions.Add(_cond);

                return this;
            }

            public string GetCompleteQuery()
            {
                if (Command == null)
                    throw new QueryBrokenException(this);

                var query = GetBaseQuery();

                if (HasCondition) {
                    query += " WHERE ";
                    foreach (var condition in Conditions)
                        query += condition;
                }

                if (HasLimit)
                    query += " LIMIT " + Limit;

                query += ";";

                return query;
            }
            public abstract string GetBaseQuery();

            public void Dispose()
            {
                Conditions.Clear();
                Conditions = null;
                try {
                    Command.Dispose();
                }
                catch { }
            }
        }

        public class Delete : QueryBase
        {
            public Delete(string Table) : this(Table, 0)
            {
            }
            public Delete(string Table, uint Limit) : base(Table, Limit)
            {
            }

            public Delete AddCondition(string field, object value)
            {
                return AddCondition(field, value, QueryConditionType.AND);
            }
            public new Delete AddCondition(string field, object value, QueryConditionType conditionType)
            {
                base.AddCondition(field, value, conditionType);
                return this;
            }

            public int Execute()
            {
                if (!HasCondition)
                    throw new DeleteQuerySecureException(this, true);

                return Deletion();
            }
            public void ExecuteAsync(Action<int> OnExecuted)
            {
                RuntimeModule.ExecuteAsync(() => OnExecuted(Execute()));
            }
            public int ExecuteSecure()
            {
                if (HasCondition)
                    throw new DeleteQuerySecureException(this, false);

                return Deletion();
            }
            public void ExecuteSecureAsync(Action<int> OnExecuted)
            {
                RuntimeModule.ExecuteAsync(() => OnExecuted(ExecuteSecure()));
            }

            private int Deletion()
            {
                try
                {
                    Command.CommandText = GetCompleteQuery();
                    return Command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    return -1;
                }
            }

            public override string GetBaseQuery()
            {
                return "DELETE FROM `" + Table + "`";
            }
        }
        public class Update : QueryBase
        {
            public bool HasSetter
            {
                get
                {
                    return Setters.Count > 0;
                }
            }

            private List<string> Setters;

            public Update(string Table) : this(Table, 0)
            {
            }
            public Update(string Table, uint Limit) : base(Table, Limit)
            {
                Setters = new List<string>();
            }

            public Update AddCondition(string field, object value)
            {
                base.AddCondition(field, value);
                return this;
            }
            public new Update AddCondition(string field, object value, QueryConditionType conditionType)
            {
                base.AddCondition(field, value, conditionType);
                return this;
            }

            public Update Set(string field, object value)
            {
                Command?.Parameters.AddWithValue("@" + field, value);
                Setters.Add((HasSetter ? "," : string.Empty) + field + "=@" + field);

                return this;
            }
            public Update SetRange(string[] fields, object[] values)
            {
                if (fields.Length != values.Length)
                    throw new QueryAddRangeInvalidCountException(this);

                for (var i = 0; i < fields.Length; i++)
                    Set(fields[i], values[i]);
                return this;
            }

            public int Execute()
            {
                if (!HasSetter)
                    throw new UpdateQueryNoSetterException(this);

                try {
                    Command.CommandText = GetCompleteQuery();
                    return Command.ExecuteNonQuery();
                }
                catch (Exception ex) {
                    Logger.Error(ex);
                    return -1;
                }
            }
            public void ExecuteAsync(Action<int> OnExecuted)
            {
                RuntimeModule.ExecuteAsync(() => OnExecuted(Execute()));
            }

            public override string GetBaseQuery()
            {
                var query = "UPDATE `" + Table + "` SET ";
                foreach (var setter in Setters)
                    query += setter;
                return query;
            }

            public new void Dispose()
            {
                base.Dispose();
                Setters.Clear();
                Setters = null;
            }
        }
        public class Insert : QueryBase
        {
            public bool HasValues
            {
                get
                {
                    return values.Count > 0;
                }
            }

            private Dictionary<string, object> values;

            public Insert(string Table) : this(Table, 0)
            {
            }
            public Insert(string Table, uint Limit) : base(Table, Limit)
            {
                values = new Dictionary<string, object>();
            }

            public object this[string field]
            {
                set
                {
                    if (values.ContainsKey(field)) {
                        Command?.Parameters.RemoveAt("@" + field);
                        values[field] = value;
                    }
                    else
                        values.Add(field, value);

                    Command?.Parameters.AddWithValue("@" + field, value);
                }
            }

            public Insert AddRange(string[] fields, object[] values)
            {
                if (fields.Length != values.Length)
                    throw new QueryAddRangeInvalidCountException(this);

                for (var i = 0; i < fields.Length; i++) {
                    this[fields[i]] = values[i];
                }

                return this;
            }

            public long Execute()
            {
                if (!HasValues)
                    throw new InsertQueryNoValueException(this);

                try
                {
                    Command.CommandText = GetCompleteQuery();
                    Command.ExecuteNonQuery();

                    return Command.LastInsertedId;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    return 0;
                }
            }
            public void ExecuteAsync(Action<long> OnExecuted)
            {
                RuntimeModule.ExecuteAsync(() => OnExecuted(Execute()));
            }

            public override string GetBaseQuery()
            {
                var keys = values.Keys.ToArray();

                var query = "INSERT INTO `" + Table + "`(";
                for (var i = 0; i < keys.Length; i++) {
                    query += keys[i];
                    if (i < keys.Length - 1)
                        query += ",";
                }
                query += ") VALUES(";
                for (var i = 0; i < keys.Length; i++)
                {
                    query += "@" + keys[i];
                    if (i < keys.Length - 1)
                        query += ",";
                }
                
                return query + ")";
            }
        }
        public class Select : QueryBase
        {
            public bool HasFields
            {
                get
                {
                    return fields.Count > 0;
                }
            }

            private List<string> fields;

            public Select(string Table) : this(Table, 0)
            {
            }
            public Select(string Table, uint Limit) : base(Table, Limit)
            {
                fields = new List<string>();
            }

            public Select AddCondition(string field, object value)
            {
                base.AddCondition(field, value);
                return this;
            }
            public new Select AddCondition(string field, object value, QueryConditionType conditionType)
            {
                base.AddCondition(field, value, conditionType);
                return this;
            }

            public Select AddFields(params string[] fields)
            {
                this.fields.AddRange(fields);
                return this;
            }

            public SelectQueryResult Execute()
            {
                try
                {
                    Command.CommandText = GetCompleteQuery();
                    var reader = Command.ExecuteReader();

                    return new SelectQueryResult(reader);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    return null;
                }
            }
            public void ExecuteAsync(Action<SelectQueryResult> OnExecuted)
            {
                RuntimeModule.ExecuteAsync(() => OnExecuted(Execute()));
            }

            public override string GetBaseQuery()
            {
                var query = "SELECT ";

                if (HasFields) {
                    for (var i = 0; i < fields.Count; i++) {
                        query += fields[i];
                        if (i < fields.Count - 1)
                            query += ",";
                    }
                }
                else
                    query += "*";

                return query + " FROM `" + Table + "`";
            }
        }
    }
}
