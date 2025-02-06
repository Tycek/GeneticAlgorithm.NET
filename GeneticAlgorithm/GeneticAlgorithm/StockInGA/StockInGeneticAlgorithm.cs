namespace GeneticAlgorithm.StockInGA;

public class StockInGeneticAlgorithm
{
    public int NumberOfIterations { get; set; }
    public int PopulationSize { get; set; }
    public double MaxIterationsWithNoChanges { get; set; }
    public double MutationProbability { get; set; }
    public double PercentageOfSelection { get; set; }

    public List<Location> Locations { get; set; }
    public List<Product> Products { get; set; }

    private List<ProductsLocationsChromosome> Population = new();
    private ProductsLocationsChromosome BestFit = null;
    private Random Random;

    public StockInGeneticAlgorithm(int numberOfIterations, int populationSize, double mutationProb, double percentageOfSelection, int maxIterationsWithNoChanges, List<Location> locations)
    {
        NumberOfIterations = numberOfIterations;
        PopulationSize = populationSize;
        MutationProbability = mutationProb;
        PercentageOfSelection = percentageOfSelection;
        MaxIterationsWithNoChanges = maxIterationsWithNoChanges;
        Locations = locations;
        Random = new Random();
    }

    public ProductsLocationsChromosome Optimize(List<Product> products)
    {
        Products = products;

        Init(products);
        Summary();

        int iteration = 1;
        int numberOfIterationsWithNoChange = 0;

        while (iteration <= NumberOfIterations)
        {
            if (numberOfIterationsWithNoChange == MaxIterationsWithNoChanges)
            {
                Console.WriteLine("Stagnation count reached. Ending optimization.");
                break;
            }

            Population = Population.OrderBy(x => x.Fitness).ToList();

            var selectedPopulation = Population.Take((int)(PopulationSize * PercentageOfSelection)).ToList();

            Population = Repopulate(selectedPopulation);

            decimal oldBestFit = BestFit.Fitness;
            BestFit = Population.Where(x => x.Fitness == Population.Select(y => y.Fitness).Min()).First();

            if (oldBestFit == BestFit.Fitness)
                numberOfIterationsWithNoChange++;

            Console.WriteLine($"Iterace {iteration}: BestFit {BestFit}");
            //Console.ReadLine();

            iteration++;
        }

        Summary();
        return BestFit;
    }

    private List<ProductsLocationsChromosome> Repopulate(List<ProductsLocationsChromosome> selectedPopulation)
    {
        var newPopulation = selectedPopulation.Select(x => new ProductsLocationsChromosome(x.Products).Clone(x)).ToList();

        while (newPopulation.Count < PopulationSize)
        {
            (var parent1, var parent2) = ChooseParents(selectedPopulation);

            (var child1, var child2) = Crossover(parent1, parent2);

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

            entity.Fitness += location.CalculateFreeVolume(product) + 10 * location.CalculateWeightPenalty(product) + location.Priority - product.CalculateHomeLocationBonus(location);
            entity.ProductFits[i] = location.ProductFits(product);
        }

        entity.Fitness *= -1;
    }

    private (ProductsLocationsChromosome child1, ProductsLocationsChromosome child2) Crossover(ProductsLocationsChromosome parent1, ProductsLocationsChromosome parent2)
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

        // Two-point crossover
        for (int i = firstIndex; i < secondIndex; i++)
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

        for (int i = 0; i < entity.Values.Length; i++)
        {
            if (!result.ProductFits.ElementAt(i)) 
            {
                var suitableLocation = Locations.FirstOrDefault(x => x.ProductFits(entity.Products.ElementAt(i)) && !entity.Values.Contains(x.Id));
                result.Values[i] = suitableLocation == null ? result.Values[i] : suitableLocation.Id;
                result.ProductFits[i] = suitableLocation == null ? result.ProductFits[i] : suitableLocation.ProductFits(entity.Products.ElementAt(i));
            }
        }

        return result;
    }

    private void Summary()
    {
        Console.WriteLine($"Best fit: {BestFit}");
        Console.WriteLine($"Avg fitness: {Population.Average(x => x.Fitness)}");
    }
}

