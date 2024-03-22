using NJsonSchema;
using NJsonSchema.Validation;
using System.Text;

namespace Krialys.Shared.Contracts;

/// <summary>
/// Generic contract validator based on a "declarative contract"
/// The contract needs to be converted to json schema first, then json schema will be used to verify json data entry
/// A json schema can be converted to a contract, and a contract can also be converted to a json schema
/// </summary>
public static class ContractValidator
{
    /// <summary>
    /// Get json schema from a contract
    /// > Header is: Id(name of the contract) + Type(object) + AllowAdditionalProperties(true if we accept extra fields not mentioned within the contract)
    /// > Body part: Field(name of the field) + Type(of the field) + Format(sub type) + MinLength + MaxLength + IsRequired(when field is mandatory)
    /// </summary>
    /// <param name="contract"></param>
    public static string GetJsonSchema(string contract)
    {
#pragma warning disable CS0219 // The variable 'errors' is assigned but its value is never used
        ICollection<ValidationError> errors = null;
#pragma warning restore CS0219 // The variable 'errors' is assigned but its value is never used

        // Slice all slots
        var allSlots = contract.Split(';', StringSplitOptions.RemoveEmptyEntries);

        // Get Header
        var header = allSlots[0].Split('|');

        // Declare a new schema, then set properties
        var schema = new JsonSchema
        {
            Id = header[0], // Equivalent to foreign key TTL_LOGS
            Type = (JsonObjectType)Convert.ToInt32(header[1]),
            AllowAdditionalProperties = Convert.ToBoolean(header[2])
        };

        // Get Body parts
        for (int i = 1; i < allSlots.Length; i++)
        {
            string[] fields = allSlots[i].Split('|');

            schema.Properties.Add(fields[0], new JsonSchemaProperty());

            var props = schema.Properties[fields[0]];

            props.Type = (JsonObjectType)Convert.ToInt32(fields[1]);

            if (!string.IsNullOrEmpty(fields[2]))
                props.Format = fields[2];

            if (!string.IsNullOrEmpty(fields[3]))
                props.MinLength = Convert.ToInt32(fields[3]);

            if (!string.IsNullOrEmpty(fields[4]))
                props.MaxLength = Convert.ToInt32(fields[4]);

            props.IsRequired = Convert.ToBoolean(fields[5]);
        }

        // Json schema
        var schemaData = schema.ToJson();

        return schemaData;
    }

    /// <summary>
    /// Get contract from json schema
    /// </summary>
    /// <param name="jsonSchema">Json schema</param>
    /// <returns></returns>
    public static string GetContractFromJsonSchema(string jsonSchema)
    {
        var sb = new StringBuilder();

        // Convert json to schema
        var schema = JsonSchema.FromJsonAsync(jsonSchema).Result;

        // Header
        sb.Append($"{schema.Id}|{(int)schema.Type}|{schema.AllowAdditionalProperties};");

        // Body
        foreach (var key in schema.Properties.Keys)
        {
            if (schema.Properties.TryGetValue(key, out var value))
            {
                sb.Append($"{value.Name}|{(int)value.Type}|{value.Format}|{value.MinLength}|{value.MaxLength}|{value.IsRequired};");
            }
        }

        // Contract
        var contract = sb.ToString();

        return contract;
    }

    /// <summary>
    /// Validate json data against json schema
    /// </summary>
    /// <param name="jsonSchema"></param>
    /// <param name="jsonData"></param>
    /// <returns>An array of errors</returns>
    public static string ValidateJsonSchemaFromJsonData(string jsonSchema, string jsonData)
    {
        var sb = new StringBuilder();

        // Convert json to schema
        var schema = JsonSchema.FromJsonAsync(jsonSchema).Result;

        // Check json against schema
        var errors = schema.Validate(jsonData);

        if (errors.Any())
        {
            int count = 1;
            foreach (var error in errors)
            {
                sb.AppendLine($"{count++:00} - Property: {error.Property} [{error.Kind} at line: {error.LineNumber}, position: {error.LinePosition}]");
            }
        }

        return sb.Length > 0 ? sb.ToString() : null;
    }

    /// <summary>
    /// Test case
    /// </summary>
    public static void Test()
    {
        // Contract header is: Id(name of the contract) + Type(object) + AllowAdditionalProperties(true if we accept extra fields not mentioned within the contract)
        var header = "CTX_TEST_v001|32|False;";

        // Contract body part: Field(name of the field) + Type(of the field) + Format(sub type) + MinLength + MaxLength + IsRequired(when field is mandatory)
        var bodypt = "TR_TESTID|16|int64|||False;TR_AGE|4|int32|||False;TR_NAME|64||2|16|True;TR_EMAIL|64|email|5|16|True;TR_DATE|64|date-time|||True;";

        // Json data
        var jsonData = @"{""TR_NAME"":""G\u00E9r\u00F4me"", ""TR_EMAIL"":""z@z.zZ"", ""TR_AGE"":49, ""TR_DATE"":""2020-07-03T09:30:00+02:00""}";

        // Define contract (header + body parts)
        var contract = header + bodypt;

        // Get schema from contract
        var jsonSchema = GetJsonSchema(contract);

        // Validate json schema using json data
        var errors = ValidateJsonSchemaFromJsonData(jsonSchema, jsonData);

        // Print error when any
        if (errors is not null)
            Console.WriteLine($"> Errors detected:\n{errors}");

        // Get contract from schema
        contract = GetContractFromJsonSchema(jsonSchema);
    }
}