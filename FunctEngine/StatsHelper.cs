namespace FunctEngine;

public class StatsHelper
{
    
    public class Bin
    {
        public double LimiteInferior { get; set; }
        public double LimiteSuperior { get; set; }
        public int Frecuencia { get; set; }
        public double PuntoMedio => (LimiteInferior + LimiteSuperior) / 2;
        
        public override string ToString()
        {
            return $"[{LimiteInferior:F2}, {LimiteSuperior:F2}): {Frecuencia}";
        }
    }
}