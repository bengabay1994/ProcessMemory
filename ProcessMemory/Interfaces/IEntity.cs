using System.Collections.Generic;

namespace ProcessMemory.Interfaces
{
    public interface IEntity
    {
        public long offset { get; set; }

        public string Name { get; set; }

    }
}
