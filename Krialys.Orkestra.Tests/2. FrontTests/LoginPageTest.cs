using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System.Text.RegularExpressions;

namespace Krialys.Orkestra.Tests.FrontTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class LoginPageTest : PageTest //BlazorTestSetup
{
    [Test]
    public async Task LoginTest()
    {
        //await Page.GotoAsync(RootUri.AbsoluteUri);

        await Page.GotoAsync("https://localhost:7192/login");

        await Page.Locator("input[type=\"text\"]").ClickAsync();

        await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^Identifiant$") }).First.ClickAsync();

        await Page.Locator("input[type=\"text\"]").FillAsync("");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Se Connecter" }).ClickAsync();
    }
}
