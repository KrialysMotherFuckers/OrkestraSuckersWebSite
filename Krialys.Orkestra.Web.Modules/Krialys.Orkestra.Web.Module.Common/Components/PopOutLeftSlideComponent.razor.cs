using Microsoft.AspNetCore.Components;

namespace Krialys.Orkestra.Web.Module.Common.Components
{
    public partial class PopOutLeftSlideComponent : ComponentBase
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }
    }
}