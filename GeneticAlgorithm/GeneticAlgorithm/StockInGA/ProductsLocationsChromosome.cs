using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.StockInGA;

public class ProductsLocationsChromosome : IComparable<ProductsLocationsChromosome>
{
    public int[] Values { get; set; }
    public bool[] ProductFits { get; set; }
    public decimal Fitness { get; set; }

    public int NumberOfProducts { get; set; }
    public List<Product> Products { get; set; }

    public ProductsLocationsChromosome(List<Product> products)
    {
        Products = products;
        Values = new int[products.Count];
        ProductFits = new bool[products.Count];
    }

    public ProductsLocationsChromosome Clone(ProductsLocationsChromosome sourceEntity)
    {
        ProductsLocationsChromosome result = new (sourceEntity.Products);

        for (int i = 0; i < Values.Length; i++)
        {
            result.Values[i] = sourceEntity.Values[i];
            result.ProductFits[i] = sourceEntity.ProductFits[i];
        }

        result.Fitness = sourceEntity.Fitness;

        return result;
    }

    public int CompareTo(ProductsLocationsChromosome? other)
    {
        return Fitness.CompareTo(other?.Fitness);
    }

    public override string ToString()
    {
        return $"Fitness: {Fitness} Values: ({string.Join(',', Values)}) Fits: ({string.Join(',', ProductFits)})";
    }
}

