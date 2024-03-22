using Krialys.Data.EF.Univers;
using Krialys.Orkestra.WebApi.Services.Common;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;

namespace Krialys.Orkestra.WebApi.Services.Data;

public interface IJobServices : IScopedService
{
    ValueTask<List<TEM_ETAT_MASTERS>> GetAll();
    ValueTask<string> ExportJob(int jobId);
    ValueTask<bool> ImportJob(string JsonJob);
}

public class JobServices : IJobServices
{
    #region Injected services

    private readonly KrialysDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ICommonServices _commonServices;

    public JobServices(KrialysDbContext dbContext, IConfiguration configuration, ICommonServices commonServices)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _commonServices = commonServices;
    }

    public ValueTask<List<TEM_ETAT_MASTERS>> GetAll() => ValueTask.FromResult(_dbContext.TEM_ETAT_MASTERS.ToList());

    public ValueTask<string> ExportJob(int jobId)
    {
        StringBuilder result = new StringBuilder();

        var etatToExport = _dbContext.TEM_ETAT_MASTERS.FirstOrDefault(x => x.TEM_ETAT_MASTERID == jobId);
        if (etatToExport == null) throw new Exception($"Record not found [TE_ETATID: {jobId}]");

        etatToExport.TE_ETATS = _dbContext.TE_ETATS.Where(x => x.TEM_ETAT_MASTERID == jobId).ToList();

        etatToExport.TE_ETATS.ToList().ForEach(x =>
        {
            x.TEB_ETAT_BATCHS = _dbContext.TEB_ETAT_BATCHS.Where(y => y.TE_ETATID == x.TE_ETATID).ToList();
            x.TEP_ETAT_PREREQUISS = _dbContext.TEP_ETAT_PREREQUISS.Where(y => y.TE_ETATID == x.TE_ETATID).ToList();
            x.TER_ETAT_RESSOURCES = _dbContext.TER_ETAT_RESSOURCES.Where(y => y.TE_ETATID == x.TE_ETATID).ToList();
            x.TS_SCENARIOS = _dbContext.TS_SCENARIOS.Where(y => y.TE_ETATID == x.TE_ETATID).ToList();
        });

        return ValueTask.FromResult(
                            JsonConvert.SerializeObject(etatToExport, Formatting.Indented,
                                        new JsonSerializerSettings()
                                        {
                                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                        }).ToString());
    }

    public ValueTask<bool> ImportJob(string jsonJob)
    {
        //var toImport = JsonSerializer.Deserialize<TE_ETATS>(jsonJob);
        throw new NotImplementedException();
    }    

    #endregion
}
