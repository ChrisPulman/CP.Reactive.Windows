// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Software;
#else
namespace CP.ReactiveUI.Primitives.Windows.Desktop.Software;
#endif
/// <summary>Contains known fields that describe installed software.</summary>
public class SoftwareDetails
{
    /// <summary>Gets or sets the application's product code GUID.</summary>
    public Guid? Id { get; set; }

    /// <summary>Gets or sets the ARPAUTHORIZEDCDFPREFIX property.</summary>
    public string AuthorizedCDFPrefix { get; set; }

    /// <summary>Gets or sets the ARPCONTACT property.</summary>
    public string Contact { get; set; }

    /// <summary>Gets or sets comments provided to the Add or Remove Programs control panel.</summary>
    public string Comments { get; set; }

    /// <summary>Gets or sets the ProductName property.</summary>
    public string DisplayName { get; set; }

    /// <summary>Gets or sets the display version derived from the ProductVersion property.</summary>
    public string DisplayVersion { get; set; }

    /// <summary>Gets or sets the estimated size determined by the Windows Installer.</summary>
    public int EstimatedSize { get; set; }

    /// <summary>Gets or sets the ARPHELPLINK property.</summary>
    public string HelpLink { get; set; }

    /// <summary>Gets or sets the ARPHELPTELEPHONE property.</summary>
    public string HelpTelephone { get; set; }

    /// <summary>Gets or sets the last time this product received service.</summary>
    public string InstallDate { get; set; }

    /// <summary>Gets or sets a value indicating whether the software is a system component.</summary>
    public bool SystemComponent { get; set; }

    /// <summary>Gets or sets the ARPINSTALLLOCATION property.</summary>
    public string InstallLocation { get; set; }

    /// <summary>Gets or sets the SourceDir property.</summary>
    public string InstallSource { get; set; }

    /// <summary>Gets or sets the ProductLanguage property.</summary>
    public int Language { get; set; }

    /// <summary>Gets or sets the modify path determined by the Windows Installer.</summary>
    public string ModifyPath { get; set; }

    /// <summary>Gets or sets the Manufacturer property advertised for the product.</summary>
    public string Publisher { get; set; }

    /// <summary>Gets or sets the readme path or URL.</summary>
    public string Readme { get; set; }

    /// <summary>Gets or sets the installed size.</summary>
    public long Size { get; set; }

    /// <summary>Gets or sets the uninstall string determined by Windows Installer.</summary>
    public string UninstallString { get; set; }

    /// <summary>Gets or sets the ARPURLINFOABOUT property.</summary>
    public string URLInfoAbout { get; set; }

    /// <summary>Gets or sets the ARPURLUPDATEINFO property.</summary>
    public string URLUpdateInfo { get; set; }

    /// <summary>Gets or sets the version derived from the ProductVersion property.</summary>
    public int Version { get; set; }

    /// <summary>Gets or sets the major version derived from the ProductVersion property.</summary>
    public int VersionMajor { get; set; }

    /// <summary>Gets or sets the minor version derived from the ProductVersion property.</summary>
    public int VersionMinor { get; set; }

    /// <summary>Gets or sets a value indicating whether Windows Installer manages the software.</summary>
    public bool WindowsInstaller { get; set; }

    /// <summary>Gets or sets a value indicating whether modify operations are disabled.</summary>
    public bool NoModify { get; set; }

    /// <summary>Gets or sets a value indicating whether repair operations are disabled.</summary>
    public bool NoRepair { get; set; }

    /// <inheritdoc />
    public override string ToString() => $"{Id.GetValueOrDefault()}: {DisplayName} - {DisplayVersion}";
}
