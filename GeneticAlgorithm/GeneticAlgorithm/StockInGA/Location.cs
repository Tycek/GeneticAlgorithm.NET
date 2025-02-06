using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.StockInGA;

public  class Location
{
    public int Id { get; set; }
    public string Code { get; set; }

    public decimal Volume { get; set; }

    public decimal MaxWeight { get; set; }

    public int Priority { get; set; }

    public decimal CalculateFreeVolume(Product product)
    {
        return Volume - product.Volume;
    }

    public decimal CalculateWeightPenalty(Product product)
    {
        return Math.Min(0, product.Weight - MaxWeight);
    }

    public bool ProductFits(Product product)
    {
        return product.Volume <= Volume;
    }
}

