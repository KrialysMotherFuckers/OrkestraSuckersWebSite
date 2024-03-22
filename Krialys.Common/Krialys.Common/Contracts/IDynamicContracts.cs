using System.Dynamic;

namespace Krialys.Shared.Contracts;

public interface IDynamicContracts
{
    DynamicContracts Select(string mapping);

    ExpandoObject From(string json, StringComparison comparison = StringComparison.OrdinalIgnoreCase);

    string LastError { get; }
}