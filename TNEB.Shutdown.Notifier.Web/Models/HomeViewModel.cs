using TNEB.Shutdown.Notifier.Web.Data.Models;

namespace TNEB.Shutdown.Notifier.Web.Models
{
    public class HomeViewModel
    {
        public CircleEntry[] Circles { get; }

        public HomeViewModel(CircleEntry[] circles)
        {
            Circles = circles;
        }
    }
}
