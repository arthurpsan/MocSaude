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
                    // identificaçao e tempo
                    "ANO_CMPT" => "Ano de Competência",
                    "MES_CMPT" => "Mês de Competência",
                    "N_AIH" => "Número da AIH",
                    "CEP" => "CEP do Paciente",
                    "MUNIC_RES" => "Município de Residência",
                    "MUNIC_MOV" => "Município do Hospital",

                    // dados clinicos
                    "NASC" => "Data de Nascimento",
                    "SEXO" => "Sexo do Paciente",
                    "IDADE" => "Idade do Paciente",
                    "DIAG_PRINC" => "Diagnóstico Principal (CID-10)",
                    "DIAG_SECUN" => "Diagnóstico Secundário",
                    "MORTE" => "Ocorreu Óbito? (1=Sim, 0=Não)",
                    "CAR_INT" => "Caráter da Internação (Urgência/Eletiva)",
                    "ESPEC" => "Especialidade do Leito",

                    // indicadores hospitalares
                    "DIAS_PERM" => "Tempo de Permanência (Dias)",
                    "UTI_MES_TO" => "Dias na UTI (No Mês)",
                    "UTI_INT_TO" => "Dias na UTI (Total da Internação)",
                    "COBRANCA" => "Motivo da Saída/Alta",

                    // valores financeiros
                    "VAL_TOT" => "Valor Total da Internação (R$)",
                    "VAL_SH" => "Valor Serviços Hospitalares (R$)",
                    "VAL_SP" => "Valor Serviços Profissionais (R$)",
                    "VAL_UTI" => "Valor Gasto em UTI (R$)",

                    // casos não mapeados
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
    }
}
