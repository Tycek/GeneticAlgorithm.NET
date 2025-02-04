using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.StockInGA;

public class Product
{
    public string Code { get; set; }
    public decimal Volume { get; set; }
    public List<int> HomeLocations { get; set; }
    public decimal Weight { get; set; }

    public int CalculateHomeLocationBonus(Location location)
    {
        return HomeLocations.Contains(location.Id) ? 2 : 0;
    }
}

