using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WindowsUpdateDisabler
{
    public class WindowsServiceMonitor
    {
        private readonly System.ServiceProcess.ServiceController _service;

        public string ServiceName { get; private set; }

        public WindowsServiceMonitor(string serviceName)
        {
            _service = new System.ServiceProcess.ServiceController(serviceName);
            ServiceName = _service.ServiceName;
        }

        public string DisplayName
        {
            get { return _service.DisplayName; }
        }

        public void WaitForStart()
        {
            _service.WaitForStatus(ServiceControllerStatus.Running);
        }

        public bool IsRunning
        {
            get { return _service.Status == ServiceControllerStatus.Running; }
        }

        public bool IsStopped
        {
            get { return _service.Status == ServiceControllerStatus.Stopped; }
        }

        public bool IsDisabled
        {
            get
            {
                try
                {
                    var query = String.Format("SELECT * FROM Win32_Service WHERE Name = '{0}'", _service.ServiceName);
                    var querySearch = new ManagementObjectSearcher(query);

                    var services = querySearch.Get();

                    // Since we have set the servicename in the constructor we asume the first result is always
                    // the service we are looking for
                    foreach (var service in services.Cast<ManagementObject>())
                        return Convert.ToString(service.GetPropertyValue
                        ("StartMode")) == "Disabled";
                }
                catch
                {
                    return false;
                }

                return false;
            }
        }

        public void Enable()
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey
                (@"SYSTEM\CurrentControlSet\Services\" + ServiceName, true);
                if (key != null) key.SetValue("Start", 2);
            }
            catch (Exception e)
            {
                throw new Exception("Could not enable the service, error: " + e.Message);
            }
        }

        public void Disable()
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey
                (@"SYSTEM\CurrentControlSet\Services\" + ServiceName, true);
                if (key != null) key.SetValue("Start", 4);
            }
            catch (Exception e)
            {
                throw new Exception("Could not disable the service, error: " + e.Message);
            }
        }

        public void Start()
        {
            if (_service.Status != ServiceControllerStatus.Running ||
            _service.Status != ServiceControllerStatus.StartPending)
                _service.Start();

            _service.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 1, 0));
        }

        public void Stop()
        {
            _service.Stop();
            _service.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 1, 0));
        }

        public void Restart()
        {
            Stop();
            Start();
        }
    }
}
