namespace MocSaude.Models
{
    public class KpiCard
    {
        public string Titulo      { get; set; } = "";
        public string Valor       { get; set; } = "";
        public string Icone       { get; set; } = "";  // emoji/texto curto
        public Color  Cor         { get; set; } = Color.FromArgb(0, 150, 136);
    }
}
