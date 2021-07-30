using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.ORM;

namespace InertiaTests.ORM
{
    public class LocalDatabase : Database
    {
        public override string Name => "local";
        public override string User => "root";
        public override string Password => "root";
        public override string Host => "localhost";

        public void DoStuff()
        {
            //drop table
            Drop<UserTable>();

            //create table
            if (Create<UserTable>())
            {
                Console.WriteLine("User table created!");

                long lastInsertedId = 0;
                var count = Count<UserTable>();
                if (count < 100)
                {
                    var r = new Random();
                    var insertCount = 55;

                    //insert new elements
                    for (var i = 0; i < insertCount; i++)
                    {
                        var v = i;
                        var rv = (decimal)r.NextDouble() * r.Next(150, 300);
                        lastInsertedId = new UserTable()
                        {
                            username = $"Etamiin{ v }",
                            password = $"Eta{ i }",
                            birthday = DateTime.Now.AddDays(-1),
                            randomValue = rv,
                            randomDouble = (double)rv,
                            randomFloat = (float)rv
                        }.Insert();

                        Console.WriteLine("New User Inserted (ID:{0})...", lastInsertedId);
                    }

                    Console.WriteLine($"Inserted users x{ insertCount }");
                }

                //select with condition
                using (var cd = new SqlCondition())
                {
                    cd.AddBetween("id", lastInsertedId - 1, lastInsertedId + 1, ConditionType.OR);
                    
                    var user = Select<UserTable>(cd, "birthday");

                    Console.WriteLine("Last User birthday selected: {0}", user.birthday);
                }

                //select all
                foreach (var elem in SelectAll<UserTable>(true, "birthday"))
                {
                    Console.WriteLine("{0} -> {1} -> {2} -> {3} -> {4}", elem.id, elem.birthday, elem.randomValue, elem.randomDouble, elem.randomFloat);
                }

                //execute custom query
                //TryExecuteQuery("ALTER TABLE users ADD age int");

                //delete all elements with the specified conditions
                using (var cd = new SqlCondition())
                {
                    cd.Add("id", 5, ConditionOperator.Less);
                    cd.AddStringPattern("id > 12 AND id <= 15", ConditionType.OR);

                    Delete<UserTable>(cd);
                }

                //delete all
                //DeleteAll<UserTable>();

                //update
                var upUser = new UserTable()
                {
                    username = "Okil",
                    password = "Ah",
                    birthday = DateTime.Now.AddDays(2)
                };

                using (var sb = new SqlCondition())
                {
                    sb.Add("id", lastInsertedId, ConditionOperator.Equal);

                    upUser.Update(sb);
                }

                //UpdateAll(upUser);

                var distinctCount = Count<UserTable>("birthday", distinct: true);
                var average = Average<UserTable>("id");
                var max = Max<UserTable>("id");
                var min = Min<UserTable>("id");
                var sum = Sum<UserTable>("randomValue");

                Console.WriteLine($"Distinct Count of 'birthday' on 'users': { distinctCount }");
                Console.WriteLine($"Average of 'id' on 'users': { average }");
                Console.WriteLine($"Max of 'id' on 'users': { max.id }");
                Console.WriteLine($"Min of 'id' on 'users': { min.id }");
                Console.WriteLine($"Sum of 'randomValue' on 'users': { sum }");
            }
            else
                Console.WriteLine("User table not created");
        }
    }
}
