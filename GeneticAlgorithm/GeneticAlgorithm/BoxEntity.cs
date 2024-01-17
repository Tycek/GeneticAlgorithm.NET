using System.Diagnostics;

namespace GeneticAlgorithm;

public class BoxEntity : IEntity, IComparable<BoxEntity>
{
    public int[] Values { get; set; }
    public double Fitness { get; set; }
    public int MinimalValue { get; set; }
    public int MaximalValue { get; set; }

    private int[] Capacities { get; set; }
    private int TotalCount { get; set; }
    private double W1 { get; set; }
    private double W2 { get; set; }
    
    public BoxEntity(/*int dimensions, int maxValue, int[] capacities, int totalCount, double w1, double w2, Random random*/)
    {
        int dimensions = 4;
        MinimalValue = 0;
        MaximalValue = 4;
        Capacities = new int[] { 100, 80, 40, 10 };
        Values = new int[4];
        TotalCount = 195;
        W1 = 2;
        W2 = 0.5;


        Random random = new Random();
        for (int dimension = 0; dimension < dimensions; dimension++)
        {
            Values[dimension] = random.Next(0, MaximalValue + 1);
        }
    }

    public IEntity Clone(IEntity sourceEntity)
    {
        BoxEntity result = new BoxEntity();

        for (int i = 0; i < Values.Length; i++)
        {
            result.Values[i] = sourceEntity.Values[i];
        }

        return result;
    }

    public void Evaluate()
    {
        double totalNumberOfBoxes = Values.Sum();
        int entityCapacity = 0;

        for(int i = 0; i < Values.Length; i++)
        {
            entityCapacity += Capacities[i] * Values[i];
        }

        if (entityCapacity < TotalCount)
        {
            Fitness = double.MinValue;
            return;
        }

        double fullness = (double)TotalCount / (double)entityCapacity;
        Fitness = (W1 * fullness) - (W2 * (double)totalNumberOfBoxes);
    }

    public int CompareTo(BoxEntity? other)
    {
        return Fitness.CompareTo(other?.Fitness);
    }

    public override string ToString()
    {
        return $"Fitness: {Fitness} Values: ({string.Join(',', Values)})";
    }
}

