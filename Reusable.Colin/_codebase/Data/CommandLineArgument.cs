using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Reusable.Colin.Collections;

namespace Reusable.Colin.Data
{
    public class CommandLineArgument : List<string>, IGrouping<ImmutableNameSet, string>
    {
        internal CommandLineArgument(string key) => Key = ImmutableNameSet.Create(key);

        internal CommandLineArgument(ImmutableNameSet key) => Key = key;

        public ImmutableNameSet Key { get; }
    }
}