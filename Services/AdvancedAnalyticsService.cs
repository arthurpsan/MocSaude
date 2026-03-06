using System.Linq;
using System.Collections.Generic;

namespace MocSaude.Services
{
    public class AdvancedAnalyticsService
    {
        // detecçoes de anomalia usando Z-Score: valores que estao a mais de 'threshold' desvios padrao da media sao considerados anomalias
        public List<string> DetectarAnomalias(List<(string Label, double Valor)> dados, double threshold = 2.0)
        {
            if (!dados.Any()) return new List<string>();

            double media = dados.Average(d => d.Valor);
            double somaQuadrados = dados.Sum(d => Math.Pow(d.Valor - media, 2));
            double desvioPadrao = Math.Sqrt(somaQuadrados / dados.Count);

            var anomalias = new List<string>();

            foreach (var item in dados)
            {
                if (desvioPadrao == 0) continue;

                double zScore = Math.Abs((item.Valor - media) / desvioPadrao);
                if (zScore > threshold)
                {
                    anomalias.Add($"{item.Label} (Z-Score: {zScore:F2})");
                }
            }

            return anomalias;
        }
    }
}