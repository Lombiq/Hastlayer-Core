using Orchard;

namespace Hast.Synthesis.Services
{
    public interface IDeviceDriverSelector : IDependency
    {
        IDeviceDriver GetDriver(string deviceName);
    }
}
