using System.Diagnostics;

namespace GeneticAlgorithm.StockInGA;

public class StockInGeneticAlgorithm
{
    /* Poznámky:
     * PriorityBonus - Bude muset být normalizován do intervalu [0,1]
     * V kníze je při jedné iteraci nalezen jeden jedinec a tím je nahrazen nejhorší jedinec - otázka je, jestli toto dostatečně rychle najde optimum při větším počtu jedinců
     * Otázka, zda by měl být výběr rodičů náhodný
     * Tomuto algoritmu by mohl předcházet preprocessing v podobě vybrání počtu lokací pro každý produkt (např. 100). Algoritmus by pak vybíral z těchto lokací
     * Podle knihy, by mělo být křížení cca 80 - 90%, mutace pak okolo 5 - 10%. Čím větší šance mutace, tím se to více blíží náhodnému prohledávání
     */


    public int NumberOfIterations { get; set; }
    public int PopulationSize { get; set; }
    public double MaxIterationsWithNoChanges { get; set; }
    public double MutationProbability { get; set; }
    public double PercentageOfSelection { get; set; }
    public double RearrangeMutationProbability { get; set; }

    public List<Location> Locations { get; set; }
    public List<Product> Products { get; set; }

    public Dictionary<string, List<Location>> LocationsForProducts { get; set; } = new();

    private List<ProductsLocationsChromosome> Population = new();
    private ProductsLocationsChromosome BestFit = null;
    private Random Random;
    private int RepopulationStrategy = 1;

    public StockInGeneticAlgorithm(int numberOfIterations, int populationSize, double mutationProb, double rearrangeMutationProb, double percentageOfSelection, int maxIterationsWithNoChanges, List<Location> locations)
    {
        NumberOfIterations = numberOfIterations;
        PopulationSize = populationSize;
        MutationProbability = mutationProb;
        PercentageOfSelection = percentageOfSelection;
        MaxIterationsWithNoChanges = maxIterationsWithNoChanges;
        RearrangeMutationProbability = rearrangeMutationProb;
        Locations = locations;
        Random = new Random();
    }

    public ProductsLocationsChromosome Optimize(List<Product> products)
    {
        Products = products;

        Init(products);

        //InitAllLocationsForProducts();
        InitLimitedLocationsForProducts(100);
        //IterationSummary();

        int iteration = 1;
        int numberOfIterationsWithNoChange = 0;
        List<long> iterationTime = new List<long>();

        while (iteration <= NumberOfIterations)
        {
            
            var sw = new Stopwatch();
            sw.Start();
            if (numberOfIterationsWithNoChange == MaxIterationsWithNoChanges)
            {
                Console.WriteLine($"Stagnation count reached. Ending optimization on iteration {iteration}.");
                break;
            }

            Population = Population.OrderByDescending(x => x.Fitness).ToList();

            if (RepopulationStrategy == 1)
            {
                var selectedPopulation = Population.Take((int)(PopulationSize * PercentageOfSelection)).ToList();
                Population = RepopulateMany(selectedPopulation);
            }
            else
            {
                Population = RepopulateOne(Population);
            }

            decimal oldBestFit = BestFit.Fitness;
            BestFit = Population.Where(x => x.Fitness == Population.Select(y => y.Fitness).Max()).First();

            if (oldBestFit == BestFit.Fitness)
            {
                numberOfIterationsWithNoChange++;
            }
            else
            {
                numberOfIterationsWithNoChange = 0;
                Console.WriteLine($"Iteration {iteration}: BestFit {BestFit}");
            }

            iteration++;
            sw.Stop();
            iterationTime.Add(sw.ElapsedMilliseconds);
        }

        FinalSummary(iterationTime);
        return BestFit;
    }

    private void InitAllLocationsForProducts()
    {
        foreach (Product product in Products)
        {
            LocationsForProducts.Add(product.Code, Locations.Where(x => x.ProductFits(product)).ToList());
        }
    }

    private void InitLimitedLocationsForProducts(int limit)
    {
        foreach (Product product in Products)
        {
            var HomeLocationsWhereProductFits = product.HomeLocations.Where(x => x.ProductFits(product)).Take(limit).ToList();
            LocationsForProducts.Add(product.Code, HomeLocationsWhereProductFits);

            if (LocationsForProducts[product.Code].Count < limit)
            {
                LocationsForProducts[product.Code].AddRange(
                    Locations.Where(x =>
                        x.ProductFits(product) 
                        && !LocationsForProducts[product.Code].Select( x => x.Id).ToList().Contains(x.Id))
                    .Skip(LocationsForProducts[product.Code].Count)
                    .Take(limit - LocationsForProducts[product.Code].Count));
            }
        }
    }

    private List<ProductsLocationsChromosome> RepopulateMany(List<ProductsLocationsChromosome> selectedPopulation)
    {
        var newPopulation = selectedPopulation.Select(x => new ProductsLocationsChromosome(x.Products).Clone(x)).ToList();

        while (newPopulation.Count < PopulationSize)
        {
            (var parent1, var parent2) = ChooseParents(newPopulation);

            //(var child1, var child2) = TwoPointCrossover(parent1, parent2);
            (var child1, var child2) = OnePointCrossover(parent1, parent2);

            double mutationRand = Random.NextDouble();
            if (mutationRand < MutationProbability)
            {
                Mutate(child1);
                Mutate(child2);
            }

            Evaluate(child1);
            Evaluate(child2);

            newPopulation.Add(child1);
            newPopulation.Add(child2);
        }

        return newPopulation;
    }

    private List<ProductsLocationsChromosome> RepopulateOne(List<ProductsLocationsChromosome> originalPopulation)
    {      
        (var parent1, var parent2) = ChooseParents(originalPopulation);

        (var child1, var child2) = OnePointCrossover(parent1, parent2);

        double mutationRand = Random.NextDouble();
        if (mutationRand < MutationProbability)
        {
            Mutate(child1);
            Mutate(child2);
        }

        Evaluate(child1);
        Evaluate(child2);

        var worstIndividual = originalPopulation.OrderBy(x => x.Fitness).First(); // First individaul is the worst, since it has minimal fitness

        if (child1.Fitness > worstIndividual.Fitness)
        {
            originalPopulation.Remove(worstIndividual);
            originalPopulation.Add(child1);
        }

        worstIndividual = originalPopulation.OrderBy(x => x.Fitness).First(); // First individaul is the worst, since it has minimal fitness

        if (child2.Fitness > worstIndividual.Fitness)
        {
            originalPopulation.Remove(worstIndividual);
            originalPopulation.Add(child2);
        }

        return originalPopulation;
    }

    private void Init(List<Product> products)
    {
        for (int i = 0; i < PopulationSize; i++)
        {
            ProductsLocationsChromosome newEntity = new (products);
            InitValues(newEntity);
            Evaluate(newEntity);
            
            if (BestFit == null || newEntity.Fitness < BestFit.Fitness)
                BestFit = newEntity;

            Population.Add(newEntity);
        }
    }

    private void InitValues(ProductsLocationsChromosome newEntity)
    {
        for (int i = 0; i < newEntity.Values.Length; i++)
        {
            newEntity.Values[i] = Locations.ElementAt(Random.Next(0, Locations.Count)).Id;
        }
    }

    private (ProductsLocationsChromosome Parent1, ProductsLocationsChromosome Parent2) ChooseParents(List<ProductsLocationsChromosome> population)
    {
        int index1, index2;
        do
        {
            index1 = Random.Next(0, PopulationSize);
            index2 = Random.Next(0, PopulationSize);
        }
        while (index1 == index2);

        var parent1 = Population.ElementAt(index1);
        var parent2 = Population.ElementAt(index2);

        return (parent1, parent2);
    }

    private void Evaluate(ProductsLocationsChromosome entity)
    {
        entity.Fitness = 0;
        for (int i = 0; i < entity.Products.Count; i++)
        {
            var product = Products.First(x => x.Code == entity.Products[i].Code);
            var location = Locations.First(x => x.Id == entity.Values[i]);

            var baseFitness = location.CalculateFreeVolume(product) + 10 * location.CalculateWeightPenalty(product) + location.Priority;
            var homeBonus = 70 * HomeLocationBonus(baseFitness, product, location);
            entity.ProductFits[i] = location.ProductFits(product);
            entity.Fitness += baseFitness - homeBonus;
        }

        entity.Fitness *= -1;
    }

    private decimal HomeLocationBonus(decimal fitness, Product product, Location location)
    {
        return product.IsHomeLocation(location) ? fitness / 100 : 0;
    }

    private (ProductsLocationsChromosome child1, ProductsLocationsChromosome child2) TwoPointCrossover(ProductsLocationsChromosome parent1, ProductsLocationsChromosome parent2)
    {
        if (parent1.Values.Length < 3)
            return (parent1, parent2);

        int index1, index2;
        do
        {
            index1 = Random.Next(1, parent1.Values.Length);
            index2 = Random.Next(1, parent1.Values.Length);
        }
        while (index1 == index2);

        int firstIndex = Math.Min(index1, index2);
        int secondIndex = Math.Max(index1, index2);

        ProductsLocationsChromosome child1 = new(parent1.Products);
        ProductsLocationsChromosome child2 = new(parent2.Products);

        child1 = child1.Clone(parent1);
        child2 = child2.Clone(parent2);

        for (int i = firstIndex; i < secondIndex; i++)
        {
            child1.Values[i] = parent2.Values[i];
            child1.ProductFits[i] = parent2.ProductFits[i];
            child2.Values[i] = parent1.Values[i];
            child2.ProductFits[i] = parent1.ProductFits[i];
        }
        return (child1, child2);
    }

    private (ProductsLocationsChromosome child1, ProductsLocationsChromosome child2) OnePointCrossover(ProductsLocationsChromosome parent1, ProductsLocationsChromosome parent2)
    {
        if (parent1.Values.Length < 3)
            return (parent1, parent2);

        int index = Random.Next(1, parent1.Values.Length);

        ProductsLocationsChromosome child1 = new(parent1.Products);
        ProductsLocationsChromosome child2 = new(parent2.Products);

        child1 = child1.Clone(parent1);
        child2 = child2.Clone(parent2);

        for (int i = index; i < parent1.Values.Length; i++)
        {
            child1.Values[i] = parent2.Values[i];
            child1.ProductFits[i] = parent2.ProductFits[i];
            child2.Values[i] = parent1.Values[i];
            child2.ProductFits[i] = parent1.ProductFits[i];
        }
        return (child1, child2);
    }


    private ProductsLocationsChromosome Mutate(ProductsLocationsChromosome entity)
    {
        ProductsLocationsChromosome result = new(entity.Products);
        result.Clone(entity);
        var random = Random.NextDouble();

        for (int i = 0; i < entity.Values.Length; i++)
        {
            if (/*!result.ProductFits.ElementAt(i) ||*/ random < RearrangeMutationProbability) 
            {
                //var suitableLocations = Locations.Where(x => x.ProductFits(entity.Products.ElementAt(i)) && !entity.Values.Contains(x.Id)).ToList();
                var suitableLocation = LocationsForProducts[Products[i].Code].ElementAt(Random.Next(0, LocationsForProducts[Products[i].Code].Count));
                //var suitableLocation = Locations.ElementAt(Random.Next(0, Locations.Count));
                result.Values[i] = suitableLocation == null ? result.Values[i] : suitableLocation.Id;
                result.ProductFits[i] = suitableLocation == null ? result.ProductFits[i] : suitableLocation.ProductFits(entity.Products.ElementAt(i));
            }
        }

        return result;
    }

    private void IterationSummary()
    {
        Console.WriteLine($"Best fit: {BestFit}");
        Console.WriteLine($"Avg fitness: {Population.Average(x => x.Fitness)}");
    }

    private void FinalSummary(List<long> iterationTimes)
    {
        for (int i = 0; i < Products.Count; i++)
        {
            var p = Products[i];
            var l = Locations.First(x => x.Id == BestFit.Values[i]);

            Console.WriteLine($"{p.Code} -> Volume: {p.Volume}, Weight: {p.Weight}");
            Console.WriteLine($"Best location: Volume: {l.Volume}, Max weight: {l.MaxWeight}, Priority: {l.Priority}, Home location? {p.HomeLocations.Select(x => x.Id).Contains(l.Id)}");
        }

        Console.WriteLine($"Average iteration time: {iterationTimes.Sum() / iterationTimes.Count} ms");
    }
}

