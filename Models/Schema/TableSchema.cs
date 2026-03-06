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
                    // views de gestao
                    "VW_MODALIDADE_INTERNACAO" => "Indicadores: Modalidade (Urgência vs Eletiva)",
                    "VW_INDICADORES_COBERTURA" => "Indicadores: Cobertura Populacional",
                    "VW_PAINEL_EPIDEMIOLOGICO" => "Indicadores: Perfil de Doenças (Epidemiologia)",
                    "VW_PERFILINTERNACAO" => "Indicadores: Análise de Internações",

                    // tabelas de dados
                    "SIH_EIXO_1" => "Internações Hospitalares (Base Principal)",
                    "IBGE_EIXO_1" => "Censo Populacional (IBGE)",
                    "CID-10-CATEGORIAS" => "Dicionário de Categorias CID-10",
                    "ISCAP" => "Dicionário de Descrições CID",
                    _ => TableName
                };
            }
        }

        public override String ToString() => DisplayName;
    }
}
