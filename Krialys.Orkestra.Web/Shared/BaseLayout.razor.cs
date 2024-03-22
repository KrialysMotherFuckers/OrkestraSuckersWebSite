using Blazored.LocalStorage;
using Krialys.Orkestra.Common.Constants;
using Krialys.Orkestra.Common.Models;
using Krialys.Orkestra.Common.Models.Admin;
using Krialys.Orkestra.Web.Common.ApiClient;
using Krialys.Orkestra.Web.Infrastructure.Preferences;
using Krialys.Orkestra.Web.Infrastructure.Theme;
using Krialys.Orkestra.Web.Module.ADM.Components.Dialogs;
using Krialys.Orkestra.Web.Module.Common.DI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;

namespace Krialys.Orkestra.Web.Shared;

public partial class BaseLayout : IDisposable
{
    [Inject] private HubConnection _hubConnection { get; set; }
    [Inject] private ISfToastServices Toast { get; set; }
    [Inject] private NavigationManager Nav { get; set; }
    [Inject] private IUserClient _iUserClient { get; set; }
    [Inject] private ILicenseClient _iLicenseClient { get; set; }
    [Inject] private ILocalStorageService _localStorage { get; set; }

    [CascadingParameter] private Task<AuthenticationState> _authentificationState { get; set; }

    private IDisposable _hubCallbackTrackedCrud;
    private const int _maxFailures = 4;
    private int _hubCounterFailure;

    private ClientPreference _themePreference = default!;
    private MudTheme _currentTheme = new LightTheme();
    private bool _rightToLeft;

    /// <summary>
    /// Method called by the framework when the component has received the parameter
    /// from parent component.
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        var userIdentity = (await _authentificationState).User.Identity;
        if (userIdentity is { IsAuthenticated: true })
            if (_hubConnection?.State != HubConnectionState.Connected)
                await HubConnectAsync();
    }

    protected override async Task OnInitializedAsync()
    {
        Common.ApiClient.Licence lic = null;

        await Task.WhenAll(
                    new List<Task>()
                        {
                            Task.Run(async () => { lic = await _iLicenseClient.IsActualLicenseValidAsync(); })
                        });

        //Licence checking
        if (lic == null || !lic.IsActive)
        {
            var parameters = new DialogParameters { { "RegisterOnlyMode", true } };

            await DialogService.ShowAsync<LicenseDetailsAndRegister>(
                Trad.Keys["COMMON:Register"],
                parameters,
                new DialogOptions { DisableBackdropClick = true, CloseOnEscapeKey = false, CloseButton = false, MaxWidth = MaxWidth.Small, FullWidth = true });
        }

        if (lic != null && lic is { IsActive: true, LicenseMessage: not null }) Snackbar.Add(lic.LicenseMessage, Severity.Error);

        _themePreference ??= new ClientPreference();
        SetCurrentTheme(_themePreference);
    }

    private void SetCurrentTheme(ClientPreference themePreference)
    {
        _currentTheme = themePreference.IsDarkMode ? new DarkTheme() : new LightTheme();
        _currentTheme.Palette.Primary = themePreference.PrimaryColor;
        _currentTheme.Palette.Secondary = themePreference.SecondaryColor;
        _currentTheme.LayoutProperties.DefaultBorderRadius = $"{themePreference.BorderRadius}px";
        _currentTheme.LayoutProperties.DefaultBorderRadius = $"{themePreference.BorderRadius}px";
        _rightToLeft = themePreference.IsRtl;
    }

    private void DarkModeToggle(bool isDarkmode)
        => _currentTheme = isDarkmode ? new DarkTheme() : new LightTheme();

    #region Bus Communication

    private async Task HubConnectAsync()
    {
        // Launch the signalR connection in the background.
        if (await HubConnectWithRetryAsync())
        {
            // Receive Tracked Entities Message from CRUD
            _hubCallbackTrackedCrud ??= _hubConnection.On("OnReceiveTrackedMessage", (IList<TrackedEntity> trackedEntities) =>
            {
                if (_hubConnection.ConnectionId is not null && trackedEntities.Any())
                {
                    Bus.Publish(trackedEntities);
                }
            });
        }
    }

    /// <summary>
    /// Hub connect/deconnect factory
    /// </summary>
    /// <returns></returns>
    private async Task<bool> HubConnectWithRetryAsync()
    {
        try
        {
            // Check Hub status
            if (_hubConnection?.State == HubConnectionState.Disconnected)
            {
                using var cts = new CancellationTokenSource();
                await _hubConnection.StartAsync(cts.Token);

                // We can return success as we are now connected
                if (_hubConnection?.State == HubConnectionState.Connected)
                {
                    if (_hubCounterFailure is > 0 and < (_maxFailures + 1))
                    {
                        _hubConnection.Closed -= OnClosed;
#if RELEASE // => allow to be comfortable while debugging
							await Toast.DisplaySuccessAsync(Trad.Keys["Auth:ServerOnline"], $"[{DateTime.Now:T}] {Trad.Keys["Auth:ServerOnlineMessage"]}", true);
#endif
                    }

                    // Subscribe events
                    _hubConnection.Closed += OnClosed;

                    // Reset counter failure
                    _hubCounterFailure = 0;

                    return true;
                }
            }
        }
        catch
        {
            // Failure counter after 1 minute (4 x 15 seconds)
            if (_hubCounterFailure < _maxFailures)
            {
                _hubCounterFailure++;
                await Task.Delay(15_000);
                await HubConnectWithRetryAsync();
            }
            else if (_hubCounterFailure == _maxFailures)
            {
                // UnSubscribe event
                if (_hubConnection != null)
                {
                    _hubConnection.Closed -= OnClosed;
                }

                _hubCounterFailure = _maxFailures + 1;

#if RELEASE // => allow to be comfortable while debugging
				await Toast.DisplayInfoAsync(Trad.Keys["Auth:ServerOfflineTimeout"], $"[{DateTime.Now:T}] {Trad.Keys["Auth:ServerOfflineTimeoutMessage"]}", true);
                //Nav.NavigateTo("logout");
#endif
                return false;
            }
        }

        return _hubConnection?.State == HubConnectionState.Connected;
    }

    #endregion

    private async Task OnClosed(Exception ex)
    {
#if RELEASE // => allow to be comfortable while debugging
			await Toast.DisplayWarningAsync(Trad.Keys["Auth:ServerOfflineResilent"],
				$"[{DateTime.Now:T}] {Trad.Keys["Auth:ServerOfflineResilentMessage"]}", true);
#endif
        _hubCounterFailure = 1;

        await HubConnectWithRetryAsync();
    }

    public void Dispose()
    {
        _hubCallbackTrackedCrud?.Dispose();
        _hubConnection.Closed -= OnClosed;
    }
}