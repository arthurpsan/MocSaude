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
        public String DisplayName
        {
            get
            {
                return TableName.ToUpper() switch
                {
                    "SIH_EIXO_1" => "Internações - Dados Principais (SIH)",
                    "SIH_EIXO_2" => "Internações - Dados Detalhados (SIH)",
                    "VW_INTERNACOES_ANALISE" => "Análise Consolidada de Internações",
                    "INTERNACOES" => "Base Bruta de Internações",
                    "CNES_ESTABELECIMENTOS" => "Estabelecimentos de Saúde (CNES)",
                    "IBGE_POPULACAO" => "Dados Populacionais (IBGE)",
                    _ => TableName
                };
            }
        }

        public override String ToString() => DisplayName;
    }
}
