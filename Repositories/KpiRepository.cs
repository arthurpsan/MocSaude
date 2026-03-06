using Dapper;
using MocSaude.Infraestructure;
using MocSaude.Models;

namespace MocSaude.Repositories
{
    public class KpiRepository
    {
        private readonly DatabaseConnection _db;
        public KpiRepository(DatabaseConnection db) => _db = db;

        public async Task<KpiSummary> GetSummaryAsync(int? ano = null, int? mes = null)
        {
            // monta cláusula WHERE dinamicamente
            var where = BuildWhere(ano, mes);

            var sql = $@"
                SELECT
                    COUNT(*)                        AS TotalInternacoes,
                    SUM(CAST(MORTE AS INT))          AS TotalObitos,
                    AVG(CAST(DIAS_PERM AS FLOAT))    AS MediaDiasPerm,
                    AVG(CAST(UTI_INT_TO AS FLOAT))   AS MediaDiasUti,
                    SUM(CAST(VAL_TOT AS FLOAT))      AS ValorTotalGasto
                FROM [dbo].[SIH_EIXO_1]
                {where}";

            using var conn = _db.CreateConnection();
            var row = await conn.QueryFirstOrDefaultAsync(sql, new { Ano = ano, Mes = mes });

            if (row == null) return new KpiSummary();

            var d = (IDictionary<string, object>)row;
            return new KpiSummary
            {
                TotalInternacoes = Convert.ToInt64(d["TotalInternacoes"] ?? 0),
                TotalObitos      = Convert.ToInt64(d["TotalObitos"]      ?? 0),
                MediaDiasPerm    = Convert.ToDouble(d["MediaDiasPerm"]   ?? 0),
                MediaDiasUti     = Convert.ToDouble(d["MediaDiasUti"]    ?? 0),
                ValorTotalGasto  = Convert.ToDouble(d["ValorTotalGasto"] ?? 0),
            };
        }

        public async Task<List<(string Label, long Contagem)>> GetTop5DoencasAsync(int? ano = null, int? mes = null)
        {
            var where = BuildWhere(ano, mes);

            var sql = $@"
                SELECT TOP 5
                    DIAG_PRINC          AS Label,
                    COUNT(*)            AS Contagem
                FROM [dbo].[SIH_EIXO_1]
                {where}
                GROUP BY DIAG_PRINC
                ORDER BY Contagem DESC";

            using var conn = _db.CreateConnection();
            var rows = await conn.QueryAsync(sql, new { Ano = ano, Mes = mes });

            return rows.Select(r =>
            {
                var d = (IDictionary<string, object>)r;
                return (
                    Label:    d["Label"]?.ToString()          ?? "N/A",
                    Contagem: Convert.ToInt64(d["Contagem"]   ?? 0)
                );
            }).ToList();
        }

        public async Task<List<(string Label, long Contagem)>> GetInternacoesPorMesAsync(int? ano = null)
        {
            string where = ano.HasValue
                ? "WHERE ANO_CMPT = @Ano"
                : "";

            var sql = $@"
                SELECT
                    CAST(MES_CMPT AS VARCHAR(2))    AS Label,
                    COUNT(*)                        AS Contagem
                FROM [dbo].[SIH_EIXO_1]
                {where}
                GROUP BY MES_CMPT
                ORDER BY MES_CMPT";

            using var conn = _db.CreateConnection();
            var rows = await conn.QueryAsync(sql, new { Ano = ano });

            return rows.Select(r =>
            {
                var d = (IDictionary<string, object>)r;
                // converte número do mês para nome
                var mesNum = d["Label"]?.ToString() ?? "";
                var mesNome = mesNum switch
                {
                    "1"  => "Jan", "2"  => "Fev", "3"  => "Mar",
                    "4"  => "Abr", "5"  => "Mai", "6"  => "Jun",
                    "7"  => "Jul", "8"  => "Ago", "9"  => "Set",
                    "10" => "Out", "11" => "Nov", "12" => "Dez",
                    _    => mesNum
                };
                return (Label: mesNome, Contagem: Convert.ToInt64(d["Contagem"] ?? 0));
            }).ToList();
        }

        public async Task<List<(string Label, long Contagem)>> GetModalidadeInternacaoAsync(int? ano = null, int? mes = null)
        {
            var where = BuildWhere(ano, mes);

            var sql = $@"
                SELECT
                    CASE CAR_INT
                        WHEN '1' THEN 'Eletiva'
                        WHEN '2' THEN 'Urgência'
                        WHEN '3' THEN 'Acidente no Trabalho'
                        WHEN '4' THEN 'Acidente Trajeto'
                        WHEN '5' THEN 'Outras'
                        ELSE 'Não Informado'
                    END AS Label,
                    COUNT(*) AS Contagem
                FROM [dbo].[SIH_EIXO_1]
                {where}
                GROUP BY CAR_INT
                ORDER BY Contagem DESC";

            using var conn = _db.CreateConnection();
            var rows = await conn.QueryAsync(sql, new { Ano = ano, Mes = mes });

            return rows.Select(r =>
            {
                var d = (IDictionary<string, object>)r;
                return (
                    Label:    d["Label"]?.ToString()        ?? "N/A",
                    Contagem: Convert.ToInt64(d["Contagem"] ?? 0)
                );
            }).ToList();
        }

        public async Task<List<int>> GetAnosDisponiveisAsync()
        {
            const string sql = @"
                SELECT DISTINCT ANO_CMPT
                FROM [dbo].[SIH_EIXO_1]
                ORDER BY ANO_CMPT DESC";

            using var conn = _db.CreateConnection();
            var rows = await conn.QueryAsync(sql);
            return rows.Select(r =>
            {
                var d = (IDictionary<string, object>)r;
                return Convert.ToInt32(d["ANO_CMPT"] ?? 0);
            }).Where(a => a > 0).ToList();
        }

        // ------------------------------------------------------------------
        private static string BuildWhere(int? ano, int? mes)
        {
            if (ano.HasValue && mes.HasValue)
                return "WHERE ANO_CMPT = @Ano AND MES_CMPT = @Mes";
            if (ano.HasValue)
                return "WHERE ANO_CMPT = @Ano";
            return "";
        }
    }

    public class KpiSummary
    {
        public long   TotalInternacoes { get; set; }
        public long   TotalObitos      { get; set; }
        public double MediaDiasPerm    { get; set; }
        public double MediaDiasUti     { get; set; }
        public double ValorTotalGasto  { get; set; }
    }
}
