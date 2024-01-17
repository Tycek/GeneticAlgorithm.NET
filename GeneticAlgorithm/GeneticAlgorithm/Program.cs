using System;

namespace GeneticAlgorithm;

internal class Program
{
    static void Main(string[] args)
    {
        GeneticAlgorithm<BoxEntity> ga = new GeneticAlgorithm<BoxEntity>(500, 10, 0.8, 0.1);
        ga.Optimize();
    }
}

public class X
{
    public int A { get; set; }
    public int B { get; set; }
}