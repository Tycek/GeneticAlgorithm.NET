using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.StockInGA;
public interface IEntity
{
    public int[] Values { get; set; }
    public decimal Fitness { get; set; }
    public int NumberOfProducts { get; set; }
    public List<Product> Products { get; set; }

    public IEntity Clone(IEntity source);
}
