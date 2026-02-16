using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace ChibitsLink.main.cs.service;

public class BluetoothService
{
    private readonly IBluetoothLE _ble;
    private readonly IAdapter _adapter;
    public ObservableCollection<IDevice> DiscoveredDevices { get; } = new();

    public BluetoothService()
    {
        _ble = CrossBluetoothLE.Current;
        _adapter = CrossBluetoothLE.Current.Adapter;

        _adapter.DeviceDiscovered += (s, a) =>
        {
            if (!DiscoveredDevices.Any(d => d.Id == a.Device.Id))
                DiscoveredDevices.Add(a.Device);
        };
    }

    public async Task<bool> RequestBluetoothPermissions()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }
        return status == PermissionStatus.Granted;
    }

    public async Task ScanDevicesAsync()
    {
        if (!_ble.IsOn) return;

        DiscoveredDevices.Clear();
        await _adapter.StartScanningForDevicesAsync();
    }

    public async Task<bool> ConnectToDeviceAsync(IDevice device)
    {
        try
        {
            await _adapter.ConnectToDeviceAsync(device);
            return true;
        }
        catch (DeviceConnectionException)
        {
            return false;
        }
    }
}
