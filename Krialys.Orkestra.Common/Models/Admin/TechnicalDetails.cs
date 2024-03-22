namespace Krialys.Orkestra.Common.Models.Admin;

public class TechnicalDetails
{
    public TechnicalDetails()
    {
        DatabaseInfoList = new();      
    }

    public List<DatabaseInfo> DatabaseInfoList { get; set; }
}

public class DatabaseInfo
{
    public string DbPath { get; set; }
}
