using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Common.Models.Admin;
using Microsoft.IdentityModel.Tokens;
using Standard.Licensing;
using Standard.Licensing.Validation;

namespace Krialys.Orkestra.WebApi.Services.Admin;

public interface ILicenceService : IScopedService
{
    ValueTask<string> GenerateLicenseKey(Licence licence);
    ValueTask<Licence> DecryptLicenseKey(string licenceKey);
    ValueTask<Licence> IsActualLicenseValid();
    ValueTask<bool> IsLicenseKeyValid(Licence licence);
    ValueTask<bool> UpdateLicence(string licence);
}

public class LicenceService : ILicence, ILicenceService
{
    private readonly KrialysDbContext _dbContext;

    public LicenceService(KrialysDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async ValueTask<string> GenerateLicenseKey(Licence licence)
    {
        if (licence == null) throw new ArgumentNullException(nameof(licence));

        /*

        <License>
          <Id>ffac2078-cb01-4f94-94e8-6fbb6c5f1bf8</Id>
          <Expiration>Thu, 20 Oct 2022 00:00:00 GMT</Expiration>
          <Customer>
            <Name>Jonathan Crozier</Name>
          </Customer>
          <Signature>MEUCIFDnFrg/lBnbXoM0hTX3TJB7j20+LUSc/7CuKqd2tjt/AiEAs6s2K0mJOQLmT6TZ2oT8kdCtWiHl+bb/Sb559roZmv0=</Signature>
        </License>

        */

        if (licence.IsTrialVersion) { licence.DefaultExpirationTimeInDays = DateTime.UtcNow.AddMonths(3).Day; }

        var newLicense = License
                        .New()
                        .WithUniqueIdentifier(Guid.NewGuid())
                        .As(licence.IsTrialVersion ? Standard.Licensing.LicenseType.Trial : Standard.Licensing.LicenseType.Standard)
                        .ExpiresAt(DateTime.Now.AddDays(licence.DefaultExpirationTimeInDays))
                        .LicensedTo((c) => c.Name = string.IsNullOrEmpty(licence.CustomerName) ? "" : licence.CustomerName.Trim().Replace(" ", "_"))
                        .CreateAndSignWithPrivateKey(privateKey, passPhrase);

        return await ValueTask.FromResult(Base64UrlEncoder.Encode(newLicense.ToString()));
    }

    public async ValueTask<Licence> DecryptLicenseKey(string licenceKey)
    {
        if (string.IsNullOrEmpty(licenceKey)) throw new ArgumentNullException(nameof(licenceKey));

        var license = License.Load(Base64UrlEncoder.Decode(licenceKey));

        return await ValueTask.FromResult(
            new Licence()
            {
                CustomerName = license.Customer.Name,
                IsTrialVersion = false,
                EndValidationDate = license.Expiration
            });
    }

    public async ValueTask<bool> IsLicenseKeyValid(Licence licence)
    {
        if (licence == null) throw new ArgumentNullException(nameof(licence));
        if (string.IsNullOrEmpty(licence.LicenseKey)) throw new ArgumentNullException(nameof(licence.LicenseKey));

        var license = License.Load(Base64UrlEncoder.Decode(licence.LicenseKey));

        return await ValueTask.FromResult(
                                    !license
                                        .Validate()
                                        .ExpirationDate()
                                        .And()
                                        .Signature(publicKey)
                                        .AssertValidLicense()
                                        .Any());
    }

    public async ValueTask<Licence> IsActualLicenseValid()
    {
        Licence result = new();

        var licence = _dbContext.TM_LIC_Licence.FirstOrDefault(x => x.lic_is_active);
        if (licence != null)
        {
            var license = License.Load(Base64UrlEncoder.Decode(licence.lic_licence_key));
            result.IsActive = !license
                        .Validate()
                        .ExpirationDate()
                        .And()
                        .Signature(publicKey)
                        .AssertValidLicense()
                        .Any();

            if (!result.IsActive)
                result.LicenseMessage = "No Valid License Key. Contact the supplier to obtain a new license key.";
            else
            {
                result.EndValidationDate = licence.lic_expiration_date.Date;
                result.CustomerName = licence.lic_issued_to?.Trim();
                result.CustomerRefCode = licence.lic_customer_code?.Trim();

                var daysToEndValidationDate = DateOnly.FromDateTime(result.EndValidationDate.Value).DayNumber - DateOnly.FromDateTime(DateTime.UtcNow).DayNumber;
                if (daysToEndValidationDate <= 30)
                    result.LicenseMessage = $"License Key will expire in {daysToEndValidationDate} days. Contact the supplier to obtain a new license key.";
            }
        }
        else
            result.LicenseMessage = "No License Key. Contact the supplier to obtain a license key.";

        return await ValueTask.FromResult(result);
    }

    public async ValueTask<bool> UpdateLicence(string licenceKey)
    {
        if (string.IsNullOrEmpty(licenceKey)) throw new ArgumentNullException(nameof(licenceKey));

        var result = false;
        await using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            try
            {
                var license = License.Load(Base64UrlEncoder.Decode(licenceKey));

                //If License Key send is valid
                if (!license
                        .Validate()
                        .ExpirationDate()
                        .And()
                        .Signature(publicKey)
                        .AssertValidLicense()
                        .Any())
                {
                    _dbContext.TM_LIC_Licence.Where(x => x.lic_is_active).ToList().ForEach(x =>
                    {
                        x.lic_is_active = false;
                    });

                    _dbContext.TM_LIC_Licence.Add(
                        new TM_LIC_Licence
                        {
                            lic_product_name = "",
                            lic_issued_to = license.Customer.Name,
                            lic_customer_code = license.Customer.Name,
                            lic_issued_date = DateTime.UtcNow,
                            lic_expiration_date = license.Expiration,
                            lic_licence_key = licenceKey,
                            lic_is_active = true
                        });

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    result = true;
                }
            }
            catch //(Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }

        return await ValueTask.FromResult(result);
    }
}
