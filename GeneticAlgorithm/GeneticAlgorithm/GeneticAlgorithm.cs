namespace GeneticAlgorithm;

public class GeneticAlgorithm<T> where T : class, IEntity, new()
{
    public int NumberOfIterations { get; set; }
    public int PopulationSize { get; set; }
    public double CrosbreedProbability { get; set; }
    public double MutationProbability { get; set; }

    private List<T> Population = new();
    private T BestFit = null;
    private Random Random;

    public GeneticAlgorithm(int numberOfIterations, int populationSize, double crossbreedProb, double mutationProb)
    {
        NumberOfIterations = numberOfIterations;
        PopulationSize = populationSize;
        CrosbreedProbability = crossbreedProb;
        MutationProbability = mutationProb;
        Random = new Random();
    }

    public T Optimize()
    {
        Init();
        Summary();

        for (int i = 0; i < NumberOfIterations; i++)
        {
            (var parent1, var parent2) = ChooseParents();

            T newEntity = new();
            newEntity.Clone(parent1);

            double crossbreedRand = Random.NextDouble();
            if (crossbreedRand < CrosbreedProbability)
                Crossover(newEntity, parent2);

            double mutationRand = Random.NextDouble();
            if (mutationRand < MutationProbability)
                Mutate(newEntity);

            newEntity.Evaluate();
            Population.Sort();
            
            if (Population.ElementAt(0).Fitness < newEntity.Fitness)
            {
                Population.RemoveAt(0);
                Population.Add(newEntity);
            };

            if (newEntity.Fitness > BestFit.Fitness)
            {
                BestFit = newEntity;
                Console.WriteLine($"Iterace {i}: BestFit {BestFit}");
            }
        }

        Summary();
        return null;
    }

    private void Init()
    {
        for (int i = 0; i < PopulationSize; i++)
        {
            var newEntity = new T();
            newEntity.Evaluate();
            
            if (BestFit == null)
                BestFit = newEntity;

            else if (newEntity.Fitness > BestFit.Fitness)
                BestFit = newEntity;

            Population.Add(newEntity);
        }
    }

    private (T Parent1, T Parent2) ChooseParents()
    {
        T parent1 = Population.Where(x => x.Fitness == Population.Max(y => y.Fitness)).First();
        T parent2 = Population.ElementAt(Random.Next(0, PopulationSize));

        return (parent1, parent2);
    }

    private void Crossover(T newEntity, T parent2)
    {
        int indexOfCrossover = Random.Next(1, newEntity.Values.Length);
        for (int i = indexOfCrossover; i < newEntity.Values.Length; i++)
        {
            newEntity.Values[i] = parent2.Values[i];
        }
    }

    private void Mutate(T newEntity)
    {
        int indexOfCrossover = Random.Next(0, newEntity.Values.Length);
        double directionOfMutation = Random.NextDouble();

        if (directionOfMutation < 0.5 && newEntity.Values[indexOfCrossover] > newEntity.MinimalValue)
        {
            newEntity.Values[indexOfCrossover]--;
        }
        else if (directionOfMutation >= 0.5 && newEntity.Values[indexOfCrossover] < newEntity.MaximalValue)
        {
            newEntity.Values[indexOfCrossover]++;
        }
    }

    private void Summary()
    {
        Console.WriteLine($"Best fit: {BestFit}");
        Console.WriteLine($"Avg fitness: {Population.Average(x => x.Fitness)}");
    }
}

