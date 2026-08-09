using System.Windows.Forms;

namespace CM26.ModManager;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        System.Windows.Forms.Application.Run(new ManagerForm());
    }
}
