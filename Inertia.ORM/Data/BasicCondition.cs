using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    public class BasicCondition
    {
        public string FieldName { get; set; }
        public object FieldValue { get; set; }
        public ConditionOperator Operator { get; set; }
        public ConditionType Type { get; set; }
        public string Pattern { get; set; }

        internal bool IsPatternCondition => !string.IsNullOrEmpty(Pattern);

        public BasicCondition(string fieldName, object value, ConditionOperator conditionOperator, ConditionType type = ConditionType.And)
        {
            FieldName = fieldName;
            FieldValue = value;
            Operator = conditionOperator;
            Type = type;
        }
        public BasicCondition(string pattern, ConditionType type = ConditionType.And)
        {
            Pattern = pattern;
            Type = type;
        }
    }
}
