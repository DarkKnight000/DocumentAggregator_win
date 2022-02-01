using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public interface IInsertionRepository
    {
        public Insertion GetInsertion(string name);
    }

    public interface IAttributeRepository : IInsertionRepository
    {
        public Insertion GetInsertion(int id);
    }

    public interface IReferenceRepository : IInsertionRepository
    {
    }
}
