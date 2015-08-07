using System;
using System.Text;

namespace ServiceStack.OrmLite.SqlServer
{
	public class SqlServerExpression<T> : SqlExpression<T>
	{
        public SqlServerExpression(IOrmLiteDialectProvider dialectProvider)
            : base(dialectProvider) {}

        public override string ToUpdateStatement(T item, bool excludeDefaults = false)
        {
            var setFields = new StringBuilder();

            foreach (var fieldDef in ModelDef.FieldDefinitions)
            {
                if (fieldDef.ShouldSkipUpdate()) continue;
                if (fieldDef.IsRowVersion) continue;
                if (UpdateFields.Count > 0 
                    && !UpdateFields.Contains(fieldDef.Name) 
                    || fieldDef.AutoIncrement) continue; // added

                var value = fieldDef.GetValue(item);
                if (excludeDefaults
                    && (value == null || (!fieldDef.IsNullable && value.Equals(value.GetType().GetDefaultValue()))))
                    continue;

                fieldDef.GetQuotedValue(item, DialectProvider);

                if (setFields.Length > 0) 
                    setFields.Append(", ");

                setFields
                    .Append(DialectProvider.GetQuotedColumnName(fieldDef.FieldName))
                    .Append("=")
                    .Append(DialectProvider.GetQuotedValue(value, fieldDef.FieldType));
            }

            if (setFields.Length == 0)
                throw new ArgumentException("No non-null or non-default values were provided for type: " + typeof(T).Name);

            return string.Format("UPDATE {0} SET {1} {2}",
                base.DialectProvider.GetQuotedTableName(ModelDef), setFields, WhereExpression);
        }

	    public override string GetSubstringSql(object quotedColumn, int startIndex, int? length = null)
	    {
            return length != null
                ? string.Format("substring({0}, {1}, {2})", quotedColumn, startIndex, length.Value)
                : string.Format("substring({0}, {1}, LEN({0}) - {1} + 1)", quotedColumn, startIndex );
        }
    }
}