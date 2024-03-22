using System.Dynamic;
using System.Reflection;

namespace Krialys.Common;
/*
    dynamic pu = new Dynamic();
    pu.Name = "ParallelU";
    pu.Values = new[]
        {
            new {Category = "Bot", Informations = "No ParallelU Cient registered/running yet!" },
        };
    pu.AsString = (Func<string>)(() => $"{pu.Name}");
    var result = pu.AsString();
    result = pu.RemoveMember("AsString").ToString();
    result = pu.ContainsKey("AsString");
    var dictionary = (Dictionary<string, object>)pu;
 */

public class Dynamic : DynamicObject
{
    private readonly Dictionary<string, object> _members = new();

    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        if (_members.TryGetValue(binder.Name, out object member))
        {
            result = member;

            return true;
        }

        result = null;

        return false;
    }

    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
        _members[binder.Name] = value;

        return true;
    }

    public bool RemoveMember(string name)
    {
        return _members.ContainsKey(name) && _members.Remove(name);
    }

    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
    {
        try
        {
            var type = typeof(Dictionary<string, object>);

            result = type.InvokeMember(binder.Name, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, _members, args);

            return true;
        }
        catch
        {
            result = null;

            return false;
        }
    }

    public static explicit operator Dictionary<string, object>(Dynamic instance)
    {
        return instance._members;
    }

    public override bool TryConvert(ConvertBinder binder, out object result)
    {
        if (binder.Type.IsInstanceOfType(_members))
        {
            result = _members;

            return true;
        }

        result = null;

        return false;
    }
}