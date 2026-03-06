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
        public String DisplayName
        {
            get
            {
                return ColumnName.ToUpper() switch
                {
                    // SIH_EIXO_1
                    "DT_INTER" => "Data de Internação",
                    "DT_SAIDA" => "Data de Alta/Saída",
                    "UF_ZI" => "UF de Gestão",
                    "CNES" => "Código CNES do Hospital",

                    // IBGE_EIXO_1
                    "POP" => "População Total",
                    "ID_MUNICIP" => "Código Município (IBGE)",
                    "ORIGEM" => "Fonte dos Dados",

                    // CID-10 e ISCAP
                    "CAT" => "Categoria CID",
                    "CLASSIF" => "Classificação",
                    "DESCRICAO" => "Descrição Completa",
                    "DESCRABREV" => "Descrição Abreviada",
                    "REFER" => "Referência",
                    "EXCLUIDOS" => "Critérios de Exclusão",
                    "DESC" => "Descrição da Doença",

                    // colunas de indicadores

                    "TAXA_POR_MIL_HAB" => "Taxa por Mil Habitantes",
                    "TOTAL_INTERNACOES" => "Total de Internações",
                    "POPULACAO_MUNICIPAL" => "População Municipal",
                    "CATEGORIA_DOENCA" => "Descrição da Doença (CID)",
                    "QUANTIDADE_CASOS" => "Quantidade de Casos",
                    "MEDIA_DIAS_UTI" => "Média de Permanência em UTI",
                    "ANO_CMPT" => "Ano de Competência",
                    "VAL_TOT" => "Valor Total (R$)",
                    "MORTE" => "Óbito Confirmado",
                    "MES_CMPT" => "Mês de Competência",
                    _ => ColumnName
                };
            }
        }

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

        public override String ToString() => DisplayName;
    }
}
