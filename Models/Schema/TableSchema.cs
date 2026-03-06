using System;
using System.Collections.Generic;
using System.Text;

namespace MocSaude.Models.Schema
{
    public class TableSchema
    {
        public String SchemaName { get; set; }
        public String TableName { get; set; }
        public List<ColumnSchema> Columns { get; set; }

        public String FullName => $"{SchemaName}.{TableName}";

        // exibição do nome da tabela
        public override String ToString() => FullName;
    }
}
