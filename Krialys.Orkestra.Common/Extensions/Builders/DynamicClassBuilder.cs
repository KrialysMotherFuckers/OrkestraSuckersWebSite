using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Krialys.Orkestra.Common.Extensions.Builders;

/// <summary>
/// Dynamic class generator
/// </summary>
public static class DynamicClassBuilder
{
    public static void Test()
    {
        IList<string> propertyNames = new List<string> { "Name", "Age", "IsAdult" };
        IList<Type> propertyTypes = new List<Type> { typeof(string), typeof(int), typeof(bool) };

        var dynaType = CreateDynamicClass("Person", propertyNames, propertyTypes);

        // Create instances of the dynamic class
        IList<object> personsList = new List<object> {
                Activator.CreateInstance(dynaType, "John", 30, true),
                Activator.CreateInstance(dynaType, "Boby", 25, false),
        };

        // Serialize and deserialize the instance using System.Text.Json.JsonSerializer
        var jsonByte = SerializeJsonToUtf8Bytes(personsList);

        // Deserialize
        IEnumerable<object> deserializedPersonsList = DeserializeJson(jsonByte, dynaType);
        foreach (var person in deserializedPersonsList)
        {
            var ttt = $"Deserialized Person: {GetPropertyValue(person, "Name")}";
        }
    }

    public static Type CreateDynamicClass(string className, IList<string> propertyNames, IList<Type> propertyTypes)
    {
        if (propertyNames.Count != propertyTypes.Count)
            throw new ArgumentException("Number of property names must be equal to the number of property types.");

        AssemblyName assemblyName = new AssemblyName("DynamicAssembly");
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
        TypeBuilder typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public);

        // Add a parameterless constructor
        typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

        // Create fields for the properties
        IDictionary<string, FieldBuilder> propertyFields = new Dictionary<string, FieldBuilder>();

        for (int i = 0; i < propertyNames.Count; i++)
        {
            FieldBuilder fieldBuilder = typeBuilder.DefineField("_" + propertyNames[i], propertyTypes[i], FieldAttributes.Private);
            propertyFields.Add(propertyNames[i], fieldBuilder);
        }

        // Create constructor
        ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, propertyTypes.ToArray());
        ILGenerator constructorIl = constructorBuilder.GetILGenerator();
        constructorIl.Emit(OpCodes.Ldarg_0);
        constructorIl.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));

        for (int i = 0; i < propertyNames.Count; i++)
        {
            constructorIl.Emit(OpCodes.Ldarg_0);
            constructorIl.Emit(OpCodes.Ldarg_S, (byte)(i + 1));
            constructorIl.Emit(OpCodes.Stfld, propertyFields[propertyNames[i]]);
        }

        constructorIl.Emit(OpCodes.Ret);

        // Create getter and setter methods for each property
        for (int i = 0; i < propertyNames.Count; i++)
        {
            CreatePropertyGetterSetter(typeBuilder, propertyNames[i], propertyTypes[i], propertyFields[propertyNames[i]]);
        }

        Type dynamicType = typeBuilder.CreateType();

        return dynamicType;
    }

    private static void CreatePropertyGetterSetter(TypeBuilder typeBuilder, string propertyName, Type propertyType, FieldBuilder fieldBuilder)
    {
        // Generate the getter method
        MethodBuilder getMethodBuilder = typeBuilder.DefineMethod("get_" + propertyName,
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            propertyType, Type.EmptyTypes);

        ILGenerator getIl = getMethodBuilder.GetILGenerator();
        getIl.Emit(OpCodes.Ldarg_0);
        getIl.Emit(OpCodes.Ldfld, fieldBuilder);
        getIl.Emit(OpCodes.Ret);

        // Associate the getter method with the property
        PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
        propertyBuilder.SetGetMethod(getMethodBuilder);

        // Generate the setter method
        MethodBuilder setMethodBuilder = typeBuilder.DefineMethod("set_" + propertyName,
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            null, new[] { propertyType });

        ILGenerator setIl = setMethodBuilder.GetILGenerator();
        setIl.Emit(OpCodes.Ldarg_0);
        setIl.Emit(OpCodes.Ldarg_1);
        setIl.Emit(OpCodes.Stfld, fieldBuilder);
        setIl.Emit(OpCodes.Ret);

        // Associate the setter method with the property
        propertyBuilder.SetSetMethod(setMethodBuilder);
    }

    public static object AddParameters(this Type dynamicType, object[] paramArray)
        => Activator.CreateInstance(dynamicType, paramArray);

    public static IEnumerable<object> AddParametersList(this Type dynamicType, params object[] paramArray)
    {
        foreach (var el in paramArray)
            yield return Activator.CreateInstance(dynamicType, paramArray);
    }

    public static string SerializeDataContractJson(object obj)
    {
        var serializer = new DataContractSerializer(obj.GetType());

        using (var stream = new MemoryStream())
        {
            serializer.WriteObject(stream, obj);

            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }

    public static object DeserializeDataContractJson(string json, Type type)
    {
        var serializer = new DataContractSerializer(type);

        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
        {
            return serializer.ReadObject(stream);
        }
    }
    public static object GetPropertyValue(object obj, string propertyName)
        => obj.GetType().GetProperty(propertyName)?.GetValue(obj);

    public static string SerializeJson(object obj)
        => JsonSerializer.Serialize(obj);

    public static byte[] SerializeJsonToUtf8Bytes(object obj)
        => JsonSerializer.SerializeToUtf8Bytes(obj);

    public static IEnumerable<object> DeserializeJson(string json, Type itemType)
    {
        foreach (JsonElement el in JsonSerializer.Deserialize<IEnumerable<JsonElement>>(json))
            yield return JsonSerializer.Deserialize(el.GetRawText(), itemType);
    }

    public static IEnumerable<object> DeserializeJson(byte[] json, Type itemType)
    {
        foreach (JsonElement el in JsonSerializer.Deserialize<IEnumerable<JsonElement>>(json))
            yield return JsonSerializer.Deserialize(el.GetRawText(), itemType);
    }
}
