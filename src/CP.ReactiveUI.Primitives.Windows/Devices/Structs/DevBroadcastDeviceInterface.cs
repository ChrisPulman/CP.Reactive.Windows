// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using CP.ReactiveUI.Primitives.Windows.PolyFills;
using Microsoft.Win32;

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Devices.Structs;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Devices.Structs;
#endif
/// <summary>
/// Contains information about a class of devices.
/// See <a href="https://www.pinvoke.net/default.aspx/Structures.DEV_BROADCAST_DEVICEINTERFACE">DEV_BROADCAST_DEVICEINTERFACE</a>
/// And <a href="https://docs.microsoft.com/en-us/windows/win32/api/dbt/ns-dbt-dev_broadcast_deviceinterface_w">DEV_BROADCAST_DEVICEINTERFACE_W structure</a>
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct DevBroadcastDeviceInterface : IEquatable<DevBroadcastDeviceInterface>
{
    /// <summary>The number of leading characters to trim from a device path before display conversion.</summary>
    private const int DevicePathPrefixLength = 3;

    /// <summary>The expected number of regex groups for single-value identifiers.</summary>
    private const int IdentifierGroupCount = 2;

    /// <summary>The expected number of regex groups for vendor identifiers.</summary>
    private const int VendorGroupCount = 3;

    /// <summary>The number of registry-path segments required for a device path.</summary>
    private const int MinimumRegistryPathParts = 3;

    /// <summary>The prefix length for a device registry path segment.</summary>
    private const int DeviceRegistryPrefixLength = 2;

    /// <summary>The timeout for device identifier regular expressions.</summary>
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1.0);

    /// <summary>The vendor identifier regular expression.</summary>
    private static readonly Regex VendorExpression = new("V(ID|EN)_([0-9A-F]{4})", RegexOptions.IgnoreCase, RegexTimeout);

    /// <summary>The device identifier regular expression.</summary>
    private static readonly Regex DeviceIdExpression = new("DEV_([0-9A-F]{4})", RegexOptions.IgnoreCase, RegexTimeout);

    /// <summary>The product identifier regular expression.</summary>
    private static readonly Regex ProductExpression = new("PID_([0-9A-F]{4})", RegexOptions.IgnoreCase, RegexTimeout);

    /// <summary>The device type regular expression.</summary>
    private static readonly Regex DeviceTypeExpression = new("\\\\\\?\\\\([A-Z]+)#", RegexOptions.IgnoreCase, RegexTimeout);

    /// <summary>The structure size.</summary>
    private int _size;

    /// <summary>The device type.</summary>
    private DeviceBroadcastDeviceType _deviceType;

    /// <summary>The device class GUID.</summary>
    private Guid _classGuid;

    /// <summary>The device name.</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
    private string _name;

    /// <summary>
    /// Gets or sets the GUID for the interface device class.
    /// The GUID for the interface device class.
    /// This is the Device Interface Class GUID that comes from the device notification message.
    /// For the Device Setup Class GUID shown in Device Manager, use <see cref="P:CP.ReactiveUI.Primitives.Windows.Desktop.Devices.Structs.DevBroadcastDeviceInterface.DeviceSetupClassGuid" /> instead.
    /// </summary>
    public Guid DeviceClassGuid
    {
        get => _classGuid;
        set => _classGuid = value;
    }

    /// <summary>Gets the name of the device.</summary>
    public readonly string Name => _name;

    /// <summary>Gets the display name of the device.</summary>
    public string DisplayName
    {
        get
        {
            string displayName = _name.Substring(3);
            displayName = displayName.Substring(0, displayName.LastIndexOf("#{", StringComparison.Ordinal));
            return displayName.Replace('#', '\\');
        }
    }

    /// <summary>Gets returns a more friendly name for the device.</summary>
    public readonly string FriendlyDeviceName => GetFriendlyDeviceName();

    /// <summary>
    /// Gets returns the Device Setup Class GUID from the Windows registry.
    /// This is the Class GUID shown in Device Manager (e.g., {4d36e968-e325-11ce-bfc1-08002be10318} for Display adapters).
    /// This is different from <see cref="P:CP.ReactiveUI.Primitives.Windows.Desktop.Devices.Structs.DevBroadcastDeviceInterface.DeviceClassGuid" /> which is the Device Interface Class GUID from the notification message.
    /// Returns null if the registry key cannot be accessed or the ClassGUID value is not found.
    /// </summary>
    public readonly Guid? DeviceSetupClassGuid
    {
        get
        {
            using (RegistryKey key = TryOpenDeviceRegistryKey(_name))
            {
                if (key?.GetValue("ClassGUID") is string classGuidString && Guid.TryParse(classGuidString, out var classGuid))
                {
                    return classGuid;
                }
            }

            return null;
        }
    }

    /// <summary>Gets returns the device type, e.g. USB or HID.</summary>
    public string DeviceType
    {
        get
        {
            Match match = DeviceTypeExpression.Match(_name);
            if (match.Groups.Count == 2)
            {
                return match.Groups[1].Value;
            }

            return null;
        }
    }

    /// <summary>Gets is this a USB device?</summary>
    public bool IsUsb => "USB".Equals(DeviceType, StringComparison.OrdinalIgnoreCase);

    /// <summary>Gets is this a PCI device?</summary>
    public bool IsPci => "PCI".Equals(DeviceType, StringComparison.OrdinalIgnoreCase);

    /// <summary>Gets returns the Device ID of the device.</summary>
    public readonly string DeviceId
    {
        get
        {
            Match match = DeviceIdExpression.Match(_name);
            if (match.Groups.Count == 2)
            {
                return match.Groups[1].Value;
            }

            return null;
        }
    }

    /// <summary>Gets returns the Vendor ID of the device.</summary>
    public readonly string VendorId
    {
        get
        {
            Match match = VendorExpression.Match(_name);
            if (match.Groups.Count == 3)
            {
                return match.Groups[2].Value;
            }

            return null;
        }
    }

    /// <summary>Gets returns the Vendor ID of the device.</summary>
    public readonly string ProductId
    {
        get
        {
            Match match = ProductExpression.Match(_name);
            if (match.Groups.Count == 2)
            {
                return match.Groups[1].Value;
            }

            return null;
        }
    }

    /// <summary>Gets try to generate a Uri which might include more information about the device.</summary>
    public readonly Uri UsbDeviceInfoUri => new($"https://www.the-sz.com/products/usbid/index.php?v=0x{VendorId}&p={ProductId}");

    /// <summary>Gets or sets use an enum for handling the device class, instead of guid.</summary>
    public DeviceInterfaceClass DeviceClass
    {
        get => GetDeviceClass();
        set
        {
            DescriptionAttribute descriptionAttribute = GetDescriptionAttribute(value);
            if (!string.IsNullOrEmpty(descriptionAttribute?.Description))
            {
                _classGuid = Guid.Parse(descriptionAttribute.Description);
            }
        }
    }

    /// <summary>Factory for an empty, but initialized, DevBroadcastDeviceInterface.</summary>
    /// <returns>A DevBroadcastDeviceInterface value.</returns>
    public static DevBroadcastDeviceInterface Create() => new DevBroadcastDeviceInterface
        {
            _deviceType = DeviceBroadcastDeviceType.DeviceInterface,
            _size = Marshal.SizeOf<DevBroadcastDeviceInterface>()
        };

    /// <summary>Used for testing.</summary>
    /// <param name="deviceName">string</param>
    /// <returns>DevBroadcastDeviceInterface.</returns>
    public static DevBroadcastDeviceInterface Test(string deviceName) => Test(deviceName, DeviceInterfaceClass.Unknown);

    /// <summary>Used for testing.</summary>
    /// <param name="deviceName">string</param>
    /// <param name="deviceClass">DeviceInterfaceClass</param>
    /// <returns>DevBroadcastDeviceInterface.</returns>
    public static DevBroadcastDeviceInterface Test(string deviceName, DeviceInterfaceClass deviceClass)
    {
        Guid deviceClassGuid = default;
        DescriptionAttribute descriptionAttribute = GetDescriptionAttribute(deviceClass);
        if (!string.IsNullOrEmpty(descriptionAttribute?.Description))
        {
            deviceClassGuid = Guid.Parse(descriptionAttribute.Description);
        }

        return new DevBroadcastDeviceInterface { _name = deviceName, _deviceType = DeviceBroadcastDeviceType.DeviceInterface, _classGuid = deviceClassGuid };
    }

    /// <summary>Determines whether two values are equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns><see langword="true" /> when both values are equal.</returns>
    public static bool operator ==(DevBroadcastDeviceInterface left, DevBroadcastDeviceInterface right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines whether two values are not equal.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns><see langword="true" /> when both values are not equal.</returns>
    public static bool operator !=(DevBroadcastDeviceInterface left, DevBroadcastDeviceInterface right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override readonly bool Equals(object obj)
    {
        if (obj is DevBroadcastDeviceInterface other)
        {
            return Equals(other);
        }

        return false;
    }

    /// <inheritdoc />
    public readonly bool Equals(DevBroadcastDeviceInterface other)
    {
        if (_size == other._size && _deviceType == other._deviceType && _classGuid == other._classGuid)
        {
            return string.Equals(_name, other._name, StringComparison.Ordinal);
        }

        return false;
    }

    /// <inheritdoc />
    public override readonly int GetHashCode() => typeof(DevBroadcastDeviceInterface).GetHashCode();

    /// <summary>
    /// Helper method to parse the device name and open the corresponding registry key.
    /// Returns null if the device name format is invalid or the registry key cannot be opened.
    /// </summary>
    /// <param name="name">The device name to parse</param>
    /// <returns>RegistryKey or null. The caller is responsible for disposing the returned RegistryKey.</returns>
    private static RegistryKey TryOpenDeviceRegistryKey(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        string[] parts = name.Split('#');
        if (parts.Length < 3)
        {
            return null;
        }

        int startIndex = parts[0].IndexOf("?\\", StringComparison.Ordinal);
        checked
        {
            if (startIndex < 0 || startIndex + 2 >= parts[0].Length)
            {
                return null;
            }

            string devType = parts[0].Substring(startIndex + 2);
            string deviceInstanceId = parts[1];
            string deviceUniqueId = parts[2];
            if (string.IsNullOrWhiteSpace(devType) || string.IsNullOrWhiteSpace(deviceInstanceId) || string.IsNullOrWhiteSpace(deviceUniqueId))
            {
                return null;
            }

            string regPath = $"SYSTEM\\CurrentControlSet\\Enum\\{devType}\\{deviceInstanceId}\\{deviceUniqueId}";
            return Registry.LocalMachine.OpenSubKey(regPath);
        }
    }

    /// <summary>Gets the description attribute for a device interface class value.</summary>
    /// <param name="deviceClass">The device class value.</param>
    /// <returns>The description attribute, or <c>null</c> when the value has none.</returns>
    private static DescriptionAttribute GetDescriptionAttribute(DeviceInterfaceClass deviceClass)
    {
        MemberInfo[] members = typeof(DeviceInterfaceClass).GetMember(deviceClass.ToString());
        if (members.Length != 0)
        {
            return members[0].GetCustomAttribute<DescriptionAttribute>(inherit: false);
        }

        return null;
    }

    /// <summary>Gets the device class associated with the native class GUID.</summary>
    /// <returns>The device interface class.</returns>
    private readonly DeviceInterfaceClass GetDeviceClass()
    {
        string guidToFind = _classGuid.ToString();
        DeviceInterfaceClass[] array = CP.ReactiveUI.Primitives.Windows.PolyFills.EnumValues.Get<DeviceInterfaceClass>();
        foreach (DeviceInterfaceClass deviceClass in array)
        {
            DescriptionAttribute descriptionAttribute = GetDescriptionAttribute(deviceClass);
            if (!string.IsNullOrEmpty(descriptionAttribute?.Description) && string.Equals(guidToFind, descriptionAttribute.Description, StringComparison.OrdinalIgnoreCase))
            {
                return deviceClass;
            }
        }

        return DeviceInterfaceClass.Unknown;
    }

    /// <summary>Gets a friendly device name from the registry when one is available.</summary>
    /// <returns>The friendly device name.</returns>
    private readonly string GetFriendlyDeviceName()
    {
        using (RegistryKey key = TryOpenDeviceRegistryKey(_name))
        {
            if (key?.GetValue("FriendlyName") is string result)
            {
                return result;
            }

            if (key?.GetValue("DeviceDesc") is string result2)
            {
                int semiColonIndex = result2.LastIndexOf(';');
                return (semiColonIndex >= 0) ? result2.Substring(checked(semiColonIndex + 1)) : result2;
            }
        }

        return _name;
    }
}
