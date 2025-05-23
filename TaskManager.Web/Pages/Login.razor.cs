namespace TaskManager.Web.Pages
{
    public partial class Login
    {
        string login, password;
        bool loginFailed = false;

        async Task LoginUser()
        {
            loginFailed = false;
            if (await Auth.Login(login, password))
            {
                Navigation.NavigateTo("/");
            }
            else
            {
                loginFailed = true;
            }
        }

        void GoToRegister()
        {
            Navigation.NavigateTo("/register");
        }
    }
}
