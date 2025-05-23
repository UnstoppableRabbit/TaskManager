using Microsoft.AspNetCore.Components;
using TaskManager.Web.Services;

namespace TaskManager.Web.Pages
{
    public partial class Register
    {
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private AuthService Auth { get; set; } = default!;

        string login = string.Empty, password = string.Empty, name = string.Empty;
        bool registerFailed = false;

        async Task RegisterUser()
        {
            registerFailed = false;

            if (await Auth.Register(login, password, name))
            {
                Navigation.NavigateTo("/");
            }
            else
            {
                registerFailed = true;
            }
        }

        void GoToLogin()
        {
            Navigation.NavigateTo("/login");
        }
    }
}
