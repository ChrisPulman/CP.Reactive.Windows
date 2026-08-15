// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace CP.ReactiveUI.Primitives.Windows.Native.Kernel.Enums;

/// <summary>Windows Product types, returned by GetProductInfo.</summary>
public enum WindowsProducts : uint
{
    /// <summary>PRODUCT UNDEFINED.</summary>
    PRODUCT_UNDEFINED = 0U,
    /// <summary>The Ultimate value.</summary>
    [Description("Ultimate")]
    PRODUCT_ULTIMATE = 1U,
    /// <summary>Home Basic.</summary>
    [Description("Home Basic")]
    PRODUCT_HOME_BASIC = 2U,
    /// <summary>Home Premium.</summary>
    [Description("Home Premium")]
    PRODUCT_HOME_PREMIUM = 3U,
    /// <summary>The Enterprise value.</summary>
    [Description("Enterprise")]
    PRODUCT_ENTERPRISE = 4U,
    /// <summary>Home Basic N.</summary>
    [Description("Home Basic N")]
    PRODUCT_HOME_BASIC_N = 5U,
    /// <summary>The Business value.</summary>
    [Description("Business")]
    PRODUCT_BUSINESS = 6U,
    /// <summary>Server Standard.</summary>
    [Description("Server Standard")]
    PRODUCT_STANDARD_SERVER = 7U,
    /// <summary>Datacenter Server.</summary>
    [Description("Datacenter Server")]
    PRODUCT_DATACENTER_SERVER = 8U,
    /// <summary>Smallbusines Server.</summary>
    [Description("Smallbusines Server")]
    PRODUCT_SMALLBUSINESS_SERVER = 9U,
    /// <summary>Server Enterprise (full installation).</summary>
    [Description("Server Enterprise (full installation)")]
    PRODUCT_ENTERPRISE_SERVER = 10U,
    /// <summary>The Starter value.</summary>
    [Description("Starter")]
    PRODUCT_STARTER = 11U,
    /// <summary>Server Datacenter (core installation).</summary>
    [Description("Server Datacenter (core installation)")]
    PRODUCT_DATACENTER_SERVER_CORE = 12U,
    /// <summary>Server Standard (core installation).</summary>
    [Description("Server Standard (core installation)")]
    PRODUCT_STANDARD_SERVER_CORE = 13U,
    /// <summary>Server Enterprise (core installation).</summary>
    [Description("Server Enterprise (core installation)")]
    PRODUCT_ENTERPRISE_SERVER_CORE = 14U,
    /// <summary>Server Enterprise for Itanium-based Systems.</summary>
    [Description("Server Enterprise for Itanium-based Systems")]
    PRODUCT_ENTERPRISE_SERVER_IA64 = 15U,
    /// <summary>Business N.</summary>
    [Description("Business N")]
    PRODUCT_BUSINESS_N = 16U,
    /// <summary>Web Server (full installation).</summary>
    [Description("Web Server (full installation)")]
    PRODUCT_WEB_SERVER = 17U,
    /// <summary>HPC Edition.</summary>
    [Description("HPC Edition")]
    PRODUCT_CLUSTER_SERVER = 18U,
    /// <summary>Windows Storage Server.</summary>
    [Description("Windows Storage Server")]
    PRODUCT_HOME_SERVER = 19U,
    /// <summary>Storage Server Express.</summary>
    [Description("Storage Server Express")]
    PRODUCT_STORAGE_EXPRESS_SERVER = 20U,
    /// <summary>Storage Server Standard.</summary>
    [Description("Storage Server Standard")]
    PRODUCT_STORAGE_STANDARD_SERVER = 21U,
    /// <summary>Storage Server Workgroup.</summary>
    [Description("Storage Server Workgroup")]
    PRODUCT_STORAGE_WORKGROUP_SERVER = 22U,
    /// <summary>Enterprise Storage Server.</summary>
    [Description("Enterprise Storage Server")]
    PRODUCT_STORAGE_ENTERPRISE_SERVER = 23U,
    /// <summary>Windows Server for Windows Essential Server Solutions.</summary>
    [Description("Windows Server for Windows Essential Server Solutions")]
    PRODUCT_SERVER_FOR_SMALLBUSINESS = 24U,
    /// <summary>Small Business Server Premium.</summary>
    [Description("Small Business Server Premium")]
    PRODUCT_SMALLBUSINESS_SERVER_PREMIUM = 25U,
    /// <summary>Home Premium N.</summary>
    [Description("Home Premium N")]
    PRODUCT_HOME_PREMIUM_N = 26U,
    /// <summary>Enterprise N.</summary>
    [Description("Enterprise N")]
    PRODUCT_ENTERPRISE_N = 27U,
    /// <summary>Ultimate N.</summary>
    [Description("Ultimate N")]
    PRODUCT_ULTIMATE_N = 28U,
    /// <summary>Web Server (core installation).</summary>
    [Description("Web Server (core installation)")]
    PRODUCT_WEB_SERVER_CORE = 29U,
    /// <summary>Windows Essential Business Server Management Server.</summary>
    [Description("Windows Essential Business Server Management Server")]
    PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT = 30U,
    /// <summary>Windows Essential Business Server Security Server.</summary>
    [Description("Windows Essential Business Server Security Server")]
    PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY = 31U,
    /// <summary>Windows Essential Business Server Messaging Server.</summary>
    [Description("Windows Essential Business Server Messaging Server")]
    PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING = 32U,
    /// <summary>Server Foundation.</summary>
    [Description("Server Foundation")]
    PRODUCT_SERVER_FOUNDATION = 33U,
    /// <summary>Windows Home Server.</summary>
    [Description("Windows Home Server")]
    PRODUCT_HOME_PREMIUM_SERVER = 34U,
    /// <summary>Windows Server without Hyper-V for Windows Essential Server Solutions.</summary>
    [Description("Windows Server without Hyper-V for Windows Essential Server Solutions")]
    PRODUCT_SERVER_FOR_SMALLBUSINESS_V = 35U,
    /// <summary>Server Standard without Hyper-V.</summary>
    [Description("Server Standard without Hyper-V")]
    PRODUCT_STANDARD_SERVER_V = 36U,
    /// <summary>Server Datacenter without Hyper-V (full installation).</summary>
    [Description("Server Datacenter without Hyper-V (full installation)")]
    PRODUCT_DATACENTER_SERVER_V = 37U,
    /// <summary>Server Enterprise without Hyper-V (full installation).</summary>
    [Description("Server Enterprise without Hyper-V (full installation)")]
    PRODUCT_ENTERPRISE_SERVER_V = 38U,
    /// <summary>Server Datacenter without Hyper-V (core installation).</summary>
    [Description("Server Datacenter without Hyper-V (core installation)")]
    PRODUCT_DATACENTER_SERVER_CORE_V = 39U,
    /// <summary>Server Standard without Hyper-V (core installation).</summary>
    [Description("Server Standard without Hyper-V (core installation)")]
    PRODUCT_STANDARD_SERVER_CORE_V = 40U,
    /// <summary>Server Enterprise without Hyper-V (core installation).</summary>
    [Description("Server Enterprise without Hyper-V (core installation)")]
    PRODUCT_ENTERPRISE_SERVER_CORE_V = 41U,
    /// <summary>Microsoft Hyper-V Server.</summary>
    [Description("Microsoft Hyper-V Server")]
    PRODUCT_HYPERV = 42U,
    /// <summary>Storage Server Express (core installation).</summary>
    [Description("Storage Server Express (core installation)")]
    PRODUCT_STORAGE_EXPRESS_SERVER_CORE = 43U,
    /// <summary>Storage Server Standard (core installation).</summary>
    [Description("Storage Server Standard (core installation)")]
    PRODUCT_STORAGE_STANDARD_SERVER_CORE = 44U,
    /// <summary>Storage Server Workgroup (core installation).</summary>
    [Description("Storage Server Workgroup (core installation)")]
    PRODUCT_STORAGE_WORKGROUP_SERVER_CORE = 45U,
    /// <summary>Storage Server Enterprise (core installation).</summary>
    [Description("Storage Server Enterprise (core installation)")]
    PRODUCT_STORAGE_ENTERPRISE_SERVER_CORE = 46U,
    /// <summary>Starter N.</summary>
    [Description("Starter N")]
    PRODUCT_STARTER_N = 47U,
    /// <summary>The Professional value.</summary>
    [Description("Professional")]
    PRODUCT_PROFESSIONAL = 48U,
    /// <summary>Professional N.</summary>
    [Description("Professional N")]
    PRODUCT_PROFESSIONAL_N = 49U,
    /// <summary>Windows Small Business Server.</summary>
    [Description("Windows Small Business Server")]
    PRODUCT_SB_SOLUTION_SERVER = 50U,
    /// <summary>Server For SB Solutions.</summary>
    [Description("Server For SB Solutions")]
    PRODUCT_SERVER_FOR_SB_SOLUTIONS = 51U,
    /// <summary>Server Solutions Premium.</summary>
    [Description("Server Solutions Premium")]
    PRODUCT_STANDARD_SERVER_SOLUTIONS = 52U,
    /// <summary>Server Solutions Premium (core installation).</summary>
    [Description("Server Solutions Premium (core installation)")]
    PRODUCT_STANDARD_SERVER_SOLUTIONS_CORE = 53U,
    /// <summary>Server For SB Solutions EM.</summary>
    [Description("Server For SB Solutions EM")]
    PRODUCT_SB_SOLUTION_SERVER_EM = 54U,
    /// <summary>Server For SB Solutions EM.</summary>
    [Description("Server For SB Solutions EM")]
    PRODUCT_SERVER_FOR_SB_SOLUTIONS_EM = 55U,
    /// <summary>Windows MultiPoint Server.</summary>
    [Description("Windows MultiPoint Server")]
    PRODUCT_SOLUTION_EMBEDDEDSERVER = 56U,
    /// <summary>Windows MultiPoint Server (core installation).</summary>
    [Description("Windows MultiPoint Server (core installation)")]
    PRODUCT_SOLUTION_EMBEDDEDSERVER_CORE = 57U,
    /// <summary>Professional Embedded.</summary>
    [Description("Professional Embedded")]
    PRODUCT_PROFESSIONAL_EMBEDDED = 58U,
    /// <summary>Windows Essential Server Solution Management.</summary>
    [Description("Windows Essential Server Solution Management")]
    PRODUCT_ESSENTIALBUSINESS_SERVER_MGMT = 59U,
    /// <summary>Windows Essential Server Solution Additional.</summary>
    [Description("Windows Essential Server Solution Additional")]
    PRODUCT_ESSENTIALBUSINESS_SERVER_ADDL = 60U,
    /// <summary>Windows Essential Server Solution Management SVC.</summary>
    [Description("Windows Essential Server Solution Management SVC")]
    PRODUCT_ESSENTIALBUSINESS_SERVER_MGMTSVC = 61U,
    /// <summary>Windows Essential Server Solution Additional SVC.</summary>
    [Description("Windows Essential Server Solution Additional SVC")]
    PRODUCT_ESSENTIALBUSINESS_SERVER_ADDLSVC = 62U,
    /// <summary>Small Business Server Premium (core installation).</summary>
    [Description("Small Business Server Premium (core installation)")]
    PRODUCT_SMALLBUSINESS_SERVER_PREMIUM_CORE = 63U,
    /// <summary>Server Hyper Core V.</summary>
    [Description("Server Hyper Core V")]
    PRODUCT_CLUSTER_SERVER_V = 64U,
    /// <summary>The Embedded value.</summary>
    [Description("Embedded")]
    PRODUCT_EMBEDDED = 65U,
    /// <summary>Starter E.</summary>
    [Description("Starter E")]
    PRODUCT_STARTER_E = 66U,
    /// <summary>Home Basic E.</summary>
    [Description("Home Basic E")]
    PRODUCT_HOME_BASIC_E = 67U,
    /// <summary>Home Premium E.</summary>
    [Description("Home Premium E")]
    PRODUCT_HOME_PREMIUM_E = 68U,
    /// <summary>Professional E.</summary>
    [Description("Professional E")]
    PRODUCT_PROFESSIONAL_E = 69U,
    /// <summary>Enterprise E.</summary>
    [Description("Enterprise E")]
    PRODUCT_ENTERPRISE_E = 70U,
    /// <summary>Ultimate E.</summary>
    [Description("Ultimate E")]
    PRODUCT_ULTIMATE_E = 71U,
    /// <summary>Enterprice Evaluation.</summary>
    [Description("Enterprice Evaluation")]
    PRODUCT_ENTERPRISE_EVALUATION = 72U,
    /// <summary>Windows MultiPoint Server Standard (full installation).</summary>
    [Description("Windows MultiPoint Server Standard (full installation)")]
    PRODUCT_MULTIPOINT_STANDARD_SERVER = 76U,
    /// <summary>Windows MultiPoint Server Premium (full installation).</summary>
    [Description("Windows MultiPoint Server Premium (full installation)")]
    PRODUCT_MULTIPOINT_PREMIUM_SERVER = 77U,
    /// <summary>Server Standard (evaluation installation).</summary>
    [Description("Server Standard (evaluation installation)")]
    PRODUCT_STANDARD_EVALUATION_SERVER = 79U,
    /// <summary>Server Datacenter (evaluation installation).</summary>
    [Description("Server Datacenter (evaluation installation)")]
    PRODUCT_DATACENTER_EVALUATION_SERVER = 80U,
    /// <summary>Enterprise N Evaluation.</summary>
    [Description("Enterprise N Evaluation")]
    PRODUCT_ENTERPRISE_N_EVALUATION = 84U,
    /// <summary>Embedded Automotive.</summary>
    [Description("Embedded Automotive")]
    PRODUCT_EMBEDDED_AUTOMOTIVE = 85U,
    /// <summary>Embedded Industry A.</summary>
    [Description("Embedded Industry A")]
    PRODUCT_EMBEDDED_INDUSTRY_A = 86U,
    /// <summary>Thin PC.</summary>
    [Description("Thin PC")]
    PRODUCT_THINPC = 87U,
    /// <summary>Embedded A.</summary>
    [Description("Embedded A")]
    PRODUCT_EMBEDDED_A = 88U,
    /// <summary>Embedded Industry.</summary>
    [Description("Embedded Industry")]
    PRODUCT_EMBEDDED_INDUSTRY = 89U,
    /// <summary>Embedded E.</summary>
    [Description("Embedded E")]
    PRODUCT_EMBEDDED_E = 90U,
    /// <summary>Embedded Industry E.</summary>
    [Description("Embedded Industry E")]
    PRODUCT_EMBEDDED_INDUSTRY_E = 91U,
    /// <summary>Embedded Industry A E.</summary>
    [Description("Embedded Industry A E")]
    PRODUCT_EMBEDDED_INDUSTRY_A_E = 92U,
    /// <summary>Storage Server Workgroup (evaluation installation).</summary>
    [Description("Storage Server Workgroup (evaluation installation)")]
    PRODUCT_STORAGE_WORKGROUP_EVALUATION_SERVER = 95U,
    /// <summary>Storage Server Standard (evaluation installation).</summary>
    [Description("Storage Server Standard (evaluation installation)")]
    PRODUCT_STORAGE_STANDARD_EVALUATION_SERVER = 96U,
    /// <summary>Core ARM.</summary>
    [Description("Core ARM")]
    PRODUCT_CORE_ARM = 97U,
    /// <summary>Core N.</summary>
    [Description("Core N")]
    PRODUCT_CORE_N = 98U,
    /// <summary>Home China.</summary>
    [Description("Home China")]
    PRODUCT_CORE_COUNTRYSPECIFIC = 99U,
    /// <summary>Home Single Language.</summary>
    [Description("Home Single Language")]
    PRODUCT_CORE_SINGLELANGUAGE = 100U,
    /// <summary>The Home value.</summary>
    [Description("Home")]
    PRODUCT_CORE = 101U,
    /// <summary>Professional with Media Center.</summary>
    [Description("Professional with Media Center")]
    PRODUCT_PROFESSIONAL_WMC = 103U,
    /// <summary>The Mobile value.</summary>
    [Description("Mobile")]
    PRODUCT_MOBILE_CORE = 104U,
    /// <summary>Embedded Industry (evaluation installation).</summary>
    [Description("Embedded Industry (evaluation installation)")]
    PRODUCT_EMBEDDED_INDUSTRY_EVAL = 105U,
    /// <summary>Embedded Industry E (evaluation installation).</summary>
    [Description("Embedded Industry E (evaluation installation)")]
    PRODUCT_EMBEDDED_INDUSTRY_E_EVAL = 106U,
    /// <summary>Embedded (evaluation installation).</summary>
    [Description("Embedded (evaluation installation)")]
    PRODUCT_EMBEDDED_EVAL = 107U,
    /// <summary>Embedded E (evaluation installation).</summary>
    [Description("Embedded E (evaluation installation)")]
    PRODUCT_EMBEDDED_E_EVAL = 108U,
    /// <summary>Nano Server.</summary>
    [Description("Nano Server")]
    PRODUCT_NANO_SERVER = 109U,
    /// <summary>Cloud storage server.</summary>
    [Description("Cloud storage server")]
    PRODUCT_CLOUD_STORAGE_SERVER = 110U,
    /// <summary>Core Connected.</summary>
    [Description("Core Connected")]
    PRODUCT_CORE_CONNECTED = 111U,
    /// <summary>Professional Student.</summary>
    [Description("Professional Student")]
    PRODUCT_PROFESSIONAL_STUDENT = 112U,
    /// <summary>Core Connected N.</summary>
    [Description("Core Connected N")]
    PRODUCT_CORE_CONNECTED_N = 113U,
    /// <summary>Professional Student N.</summary>
    [Description("Professional Student N")]
    PRODUCT_PROFESSIONAL_STUDENT_N = 114U,
    /// <summary>Core Connected Single Language.</summary>
    [Description("Core Connected Single Language")]
    PRODUCT_CORE_CONNECTED_SINGLELANGUAGE = 115U,
    /// <summary>Core Connected China.</summary>
    [Description("Core Connected China")]
    PRODUCT_CORE_CONNECTED_COUNTRYSPECIFIC = 116U,
    /// <summary>Connected Car.</summary>
    [Description("Connected Car")]
    PRODUCT_CONNECTED_CAR = 117U,
    /// <summary>Industry Handeld.</summary>
    [Description("Industry Handeld")]
    PRODUCT_INDUSTRY_HANDHELD = 118U,
    /// <summary>PPI Professional.</summary>
    [Description("PPI Professional")]
    PRODUCT_PPI_PRO = 119U,
    /// <summary>ARM64 Sever.</summary>
    [Description("ARM64 Sever")]
    PRODUCT_ARM64_SERVER = 120U,
    /// <summary>The Education value.</summary>
    [Description("Education")]
    PRODUCT_EDUCATION = 121U,
    /// <summary>Education N.</summary>
    [Description("Education N")]
    PRODUCT_EDUCATION_N = 122U,
    /// <summary>IoT Core.</summary>
    [Description("IoT Core")]
    PRODUCT_IOTUAP = 123U,
    /// <summary>Cloud Host infrastructure Server.</summary>
    [Description("Cloud Host infrastructure Server")]
    PRODUCT_CLOUD_HOST_INFRASTRUCTURE_SERVER = 124U,
    /// <summary>Enterprise LTSB.</summary>
    [Description("Enterprise LTSB")]
    PRODUCT_ENTERPRISE_S = 125U,
    /// <summary>Windows 10 Enterprise 2015 LTSB N.</summary>
    [Description("Windows 10 Enterprise 2015 LTSB N")]
    PRODUCT_ENTERPRISE_S_N = 126U,
    /// <summary>Professional S.</summary>
    [Description("Professional S")]
    PRODUCT_PROFESSIONAL_S = 127U,
    /// <summary>Professional LTSB N.</summary>
    [Description("Professional LTSB N")]
    PRODUCT_PROFESSIONAL_S_N = 128U,
    /// <summary>Windows 10 Enterprise 2015 LTSB Evaluation.</summary>
    [Description("Windows 10 Enterprise 2015 LTSB Evaluation")]
    PRODUCT_ENTERPRISE_S_EVALUATION = 129U,
    /// <summary>Windows 10 Enterprise 2015 LTSB N Evaluation.</summary>
    [Description("Windows 10 Enterprise 2015 LTSB N Evaluation")]
    PRODUCT_ENTERPRISE_S_N_EVALUATION = 130U,
    /// <summary>IoT Core Commercial.</summary>
    [Description("IoT Core Commercial")]
    PRODUCT_IOTUAPCOMMERCIAL = 131U,
    /// <summary>The Unlicensed value.</summary>
    [Description("Unlicensed")]
    PRODUCT_UNLICENSED = 2_882_382_797U
}
