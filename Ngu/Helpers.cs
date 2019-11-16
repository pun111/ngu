using System.Threading.Tasks;
using System.Windows;

namespace Ngu
{
    public class Helpers
    {
        public static void MessageBoxNonBlocking(string textContent, string textCaption = "Message")
            => Task.Run(() => MessageBox.Show(textContent, textCaption));
    }
}
