namespace Krialys.Orkestra.Common.Models;

public class ResponseCore
{
    public string Message { get; set; }
    public bool HasError { get { return !string.IsNullOrEmpty(ErrorMessage); } }
    public string ErrorMessage { get; set; }
    public string ExceptionDetail { get; set; }
}

public class PagedResponse<TModel> : ResponseCore
{
    public IEnumerable<TModel> result { get; set; }
    public int count { get; set; }
}

public class PagedQueryableResponse<TModel> : ResponseCore
{
    public IQueryable<TModel> Items { get; set; }
    public int Count { get; set; }
}
