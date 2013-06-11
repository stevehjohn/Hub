using System.Windows;

namespace Ming
{
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {
            var inst = MingApp.Instance;
            inst.Start();
        }
    }
}
