using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.StockInGA;

public class ProductsLocationsChromosome : IEntity, IComparable<ProductsLocationsChromosome>
{
    public int[] Values { get; set; }
    public decimal Fitness { get; set; }

    public int NumberOfProducts { get; set; }
    public List<Product> Products { get; set; }

    public ProductsLocationsChromosome(int numberOfProducts, List<Product> products)
    {
        NumberOfProducts = numberOfProducts;
        Products = products;
        Values = new int[NumberOfProducts];
    }

    public IEntity Clone(IEntity sourceEntity)
    {
        IEntity result = new ProductsLocationsChromosome(sourceEntity.NumberOfProducts, sourceEntity.Products);

        for (int i = 0; i < Values.Length; i++)
        {
            result.Values[i] = sourceEntity.Values[i];
        }

        return result;
    }

    public int CompareTo(ProductsLocationsChromosome? other)
    {
        return Fitness.CompareTo(other?.Fitness);
    }

    public override string ToString()
    {
        return $"Fitness: {Fitness} Values: ({string.Join(',', Values)})";
    }
}

