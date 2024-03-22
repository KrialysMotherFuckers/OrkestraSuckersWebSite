using System.ComponentModel.DataAnnotations;
using System.Text;

// https://www.c-sharpcorner.com/article/dataannotations-in-depth/
namespace Krialys.Common.Validations;

public static class Extensiones
{
    public static string GetAllErrors(this IEnumerable<ValidationResult> source, string messageEmptyCollection = null)
    {
        var result = new StringBuilder();

        var validationResults = source as ValidationResult[] ?? source.ToArray();

        if (validationResults.Any())
        {
            result.AppendLine("We found the next validations errors:");
            validationResults.ToList()
                .ForEach(s => result.AppendFormat("  {0} --> {1}{2}", s.MemberNames.FirstOrDefault(), s.ErrorMessage, Environment.NewLine));
        }
        else
        {
            result.AppendLine(messageEmptyCollection ?? string.Empty);
        }

        return result.ToString();
    }

    public static IEnumerable<ValidationResult> ValidateObject(this object source)
    {
        var valContext = new ValidationContext(source, null, null);
        IList<ValidationResult> result = new List<ValidationResult>();

        Validator.TryValidateObject(source, valContext, result, true);

        return result;
    }
}

public class ControlDateTimeAttribute : ValidationAttribute
{
    private readonly DayOfWeek[] _notValidItems;
    private readonly bool _throwException;

    public ControlDateTimeAttribute(params DayOfWeek[] notValidItems)
    {
        _throwException = false;
        _notValidItems = notValidItems;
    }

    public ControlDateTimeAttribute(bool throwException, params DayOfWeek[] notValidItems)
    {
        _throwException = throwException;
        _notValidItems = notValidItems;
    }

    public override bool IsValid(object value)
    {
        return !DateTime.TryParse(value.ToString(), out var item)
            ? _throwException
                ? throw new ArgumentException("The ControlDateTimeAttribute, only validate DateTime types.")
                : false
            : _notValidItems.Contains(item.DayOfWeek);
    }
}

//public class TestCustomer
//{
//    [ControlDateTime(DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, ErrorMessage = "The {0} isn't valid")]
//    public DateTime EntryDate { get; set; }

//    public void InnerTest()
//    {
//        TestCustomer t = new TestCustomer();
//        t.EntryDate = DateTime.Now.AddDays(3);
//        var err = t.ValidateObject();
//        var errors = err.GetAllErrors();

//        t.EntryDate = DateTime.Now.AddDays(1);
//        err = t.ValidateObject();
//        errors = err.GetAllErrors();

//        //var error = errors.First().ErrorMessage; //, $"The property {nameof(textclass.ArrayInt)} doesn't have more than 2 elements");
//    }
//}