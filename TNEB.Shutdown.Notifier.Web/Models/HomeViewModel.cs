using TNEB.Shutdown.Notifier.Web.Data.Models;

namespace TNEB.Shutdown.Notifier.Web.Models
{
    public class HomeViewModel
    {
        public Circle[] Circles { get; }

        public HomeViewModel(Circle[] circles)
        {
            Circles = circles;
        }
    }
}
