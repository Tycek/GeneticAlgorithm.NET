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
    public List<Location> HomeLocations { get; set; }
    public decimal Weight { get; set; }

    public bool IsHomeLocation(Location location)
    {
        return HomeLocations.Select(x => x.Id).Contains(location.Id);
    }
}

