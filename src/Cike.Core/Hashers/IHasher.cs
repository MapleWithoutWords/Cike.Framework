using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cike.Core.Hashers
{
    public interface IHasher
    {
        string Hash(string value);

        public string Hash(object?[] values, JsonSerializerOptions? jsonSerializerOptions = null);

        public string Hash(params string?[] values);
    }
}
