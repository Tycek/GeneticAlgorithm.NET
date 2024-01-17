namespace GeneticAlgorithm;

public interface IEntity
{
    public int[] Values { get; set; }
    public double Fitness { get; set; }
    public int MinimalValue { get; set; }
    public int MaximalValue { get; set; }

    public void Evaluate();

    public IEntity Clone(IEntity source);
}

