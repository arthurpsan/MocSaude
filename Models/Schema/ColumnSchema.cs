using System;
using System.Collections.Generic;
using System.Text;

namespace MocSaude.Models.Schema
{
    public class ColumnSchema
    {
        public String ColumnSpace { get; set; }
        public String ColumnName { get; set; }
        public String DataType { get; set; }
        public Int64 OrdinalPosition { get; set; }
        public Int64? MaxLength { get; set; }
        public Boolean IsNullable { get; set; }
        public Boolean IsPrimaryKey { get; set; }

        public Type DotNetType => DataType.ToLower() switch
        {
            "int" or "smallint" or "tinyint" => typeof(Int32),
            "bigint" => typeof(Int64),
            "decimal" or "numeric" or "money" => typeof(Decimal),
            "float" or "real" => typeof(Double),
            "bit" => typeof(Boolean),
            "datetime" or "datetime2" => typeof(DateTime),
            _ => typeof(String)
        };

        public Boolean IsNumeric 
            => DotNetType == typeof(int)
            || DotNetType == typeof(long)
            || DotNetType == typeof(decimal)
            || DotNetType == typeof(double);

        public Boolean IsDateTime
            => DotNetType == typeof(DateTime);
    }
}
