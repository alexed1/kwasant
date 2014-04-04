using System;
using Shnexy.DataAccessLayer.StructureMap;

namespace Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            StructureMapBootStrapper.ConfigureDependencies(String.Empty);
        }
    }

}
