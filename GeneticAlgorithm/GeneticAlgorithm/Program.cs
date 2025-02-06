using System;
using GeneticAlgorithm.BasicGA;
using GeneticAlgorithm.StockInGA;

namespace GeneticAlgorithm;

internal class Program
{
    static void Main(string[] args)
    {
        /*BasicGeneticAlgorithm<BoxEntity> ga = new BasicGeneticAlgorithm<BoxEntity>(500, 10, 0.8, 0.1);
        ga.Optimize();*/

        List<Location> locations = GenerateLocations(1000);
        List<Product> products = GenerateProducts(7, locations);

        //Nyní všechna řešení ve výsledku zkonvergují k jedné hodnotě. Bude potřeba přihodit kompletní rekombinaci chormozomů s určitou pravděpodobností.

        StockInGeneticAlgorithm a = new StockInGeneticAlgorithm(1000, 400, 0.2, 0.5, 200, locations);
        a.Optimize(products);
    }

    private static List<Location> GenerateLocations(int numberOfLocations)
    {   
        Random random = new Random();
        List<Location> result = new();
        int locationNumber = 1;

        while (result.Count < numberOfLocations)
        {
            result.Add(new Location
            {
                Code = $"L{locationNumber}",
                Id = locationNumber,
                MaxWeight = random.Next(10, 50),
                Priority = random.Next(1, 5), 
                Volume = random.Next(10,500)
            });

            locationNumber++;
        }

        return result;
    }

    private static List<Product> GenerateProducts(int numberOfProducts, List<Location> locations)
    {
        Random random = new Random();
        List<Product> result = new();
        int productNumber = 1;

        while (result.Count < numberOfProducts)
        {
            var newProduct = new Product
            {
                Code = $"P{productNumber}",
                Weight = random.Next(5, 20),
                Volume = random.Next(5, 100),
                HomeLocations = new List<int>()
            };

            int numberOfHomeLocations = random.Next(1, 10);

            for (int i = 0; i < numberOfHomeLocations; i++)
            {
                newProduct.HomeLocations.Add(locations.ElementAt(random.Next(0, locations.Count)).Id);
            }

            result.Add(newProduct);

            productNumber++;
        }

        return result;
    }
}
