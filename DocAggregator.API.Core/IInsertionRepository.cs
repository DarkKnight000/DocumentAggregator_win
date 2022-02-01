using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public interface IInsertionRepository
    {
        public StringInsertion GetInsertion(string name);
    }

    public interface IAttributeRepository : IInsertionRepository
    {
        public StringInsertion GetInsertion(int id);
    }

    public interface IReferenceRepository : IInsertionRepository
    {
    }
}
