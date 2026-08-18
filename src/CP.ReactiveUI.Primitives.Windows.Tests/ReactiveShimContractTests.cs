// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace CP.ReactiveUI.Primitives.Windows.Tests;

/// <summary>Verifies the lean and System.Reactive shared-source package contract.</summary>
public class ReactiveShimContractTests
{
    /// <summary>The namespace prefix emitted by the System.Reactive build.</summary>
    private const string ReactiveNamespacePrefix = "CP.ReactiveUI.Primitives.Windows.Reactive.";

    /// <summary>The namespace prefix emitted by the lean build.</summary>
    private const string LeanNamespacePrefix = "CP.ReactiveUI.Primitives.Windows.";

    /// <summary>The Unit type emitted by the System.Reactive build.</summary>
    private const string ReactiveUnitTypeName = "System.Reactive.Unit";

    /// <summary>The void signal type emitted by the lean build.</summary>
    private const string LeanVoidTypeName = "ReactiveUI.Primitives.RxVoid";

    /// <summary>Verifies representative shared types are emitted into distinct package assemblies and namespaces.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Test]
    public async Task SharedTypesHaveDistinctPackageIdentitiesAsync()
    {
        await Assert.That(typeof(LeanWaitableTimer).Assembly.GetName().Name)
            .IsEqualTo("CP.ReactiveUI.Primitives.Windows");
        await Assert.That(typeof(ReactiveWaitableTimer).Assembly.GetName().Name)
            .IsEqualTo("CP.ReactiveUI.Primitives.Windows.Reactive");
        await Assert.That(typeof(LeanWaitableTimer).Assembly).IsNotEqualTo(typeof(ReactiveWaitableTimer).Assembly);
        await Assert.That(typeof(LeanWaitableTimer).FullName)
            .IsEqualTo("CP.ReactiveUI.Primitives.Windows.Desktop.Power.WaitableTimer");
        await Assert.That(typeof(ReactiveWaitableTimer).FullName)
            .IsEqualTo("CP.ReactiveUI.Primitives.Windows.Reactive.Desktop.Power.WaitableTimer");
    }

    /// <summary>Verifies the shared public API is identical after normalizing the reactive namespace segment.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Test]
    public async Task RepresentativePublicApisRemainInParityAsync()
    {
        await AssertApiParityAsync(typeof(LeanWaitableTimer), typeof(ReactiveWaitableTimer));
        await AssertApiParityAsync(typeof(LeanWinEventHook), typeof(ReactiveWinEventHook));
        await AssertApiParityAsync(typeof(LeanDeviceNotification), typeof(ReactiveDeviceNotification));
    }

    /// <summary>Verifies every exported desktop type and member is present in both shared-source assemblies.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Test]
    public async Task AllExportedDesktopApisRemainInParityAsync()
    {
        var leanTypes = GetNormalizedTypeMap(typeof(LeanWaitableTimer).Assembly);
        var reactiveTypes = GetNormalizedTypeMap(typeof(ReactiveWaitableTimer).Assembly);
        var leanNames = GetSortedKeys(leanTypes);
        var reactiveNames = GetSortedKeys(reactiveTypes);

        await Assert.That(reactiveNames).IsEquivalentTo(leanNames);
        foreach (var typeName in leanNames)
        {
            await AssertApiParityAsync(leanTypes[typeName], reactiveTypes[typeName]);
        }
    }

    /// <summary>Verifies each package references its intended ReactiveUI.Primitives implementation.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Test]
    public async Task PackageDependenciesMatchTheirReactiveModelAsync()
    {
        var leanReferences = GetReferenceNames(typeof(LeanWaitableTimer).Assembly);
        var reactiveReferences = GetReferenceNames(typeof(ReactiveWaitableTimer).Assembly);

        await Assert.That(leanReferences).Contains("ReactiveUI.Primitives");
        await Assert.That(reactiveReferences).Contains("ReactiveUI.Primitives.Reactive");
        await Assert.That(reactiveReferences).Contains("System.Reactive");
        await Assert.That(Array.IndexOf(reactiveReferences, "CP.ReactiveUI.Primitives.Windows") < 0).IsTrue();
    }

    /// <summary>Compares normalized declared public members for a lean/reactive type pair.</summary>
    /// <param name="leanType">The type emitted by the lean package.</param>
    /// <param name="reactiveType">The type emitted by the System.Reactive package.</param>
    /// <returns>A task that completes when the assertion has run.</returns>
    private static async Task AssertApiParityAsync(Type leanType, Type reactiveType)
    {
        var leanMembers = GetPublicMemberSignatures(leanType);
        var reactiveMembers = GetPublicMemberSignatures(reactiveType);

        await Assert.That(reactiveMembers).IsEquivalentTo(leanMembers);
    }

    /// <summary>Returns normalized public member signatures declared by a type.</summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>The sorted normalized signatures.</returns>
    private static string[] GetPublicMemberSignatures(Type type)
    {
        var signatures = new List<string>();
        foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            if (member.MemberType is not MemberTypes.NestedType)
            {
                signatures.Add(Normalize(member.ToString() ?? member.Name));
            }
        }

        signatures.Sort(StringComparer.Ordinal);
        return signatures.ToArray();
    }

    /// <summary>Returns the simple names of an assembly's direct references.</summary>
    /// <param name="assembly">The assembly to inspect.</param>
    /// <returns>The referenced assembly names.</returns>
    private static string[] GetReferenceNames(Assembly assembly)
    {
        var names = new List<string>();
        foreach (var reference in assembly.GetReferencedAssemblies())
        {
            names.Add(reference.Name ?? string.Empty);
        }

        names.Sort(StringComparer.Ordinal);
        return names.ToArray();
    }

    /// <summary>Creates a map of normalized exported desktop type names to their runtime types.</summary>
    /// <param name="assembly">The shared-source package assembly.</param>
    /// <returns>The normalized type map.</returns>
    private static Dictionary<string, Type> GetNormalizedTypeMap(Assembly assembly)
    {
        var types = new Dictionary<string, Type>(StringComparer.Ordinal);
        foreach (var type in assembly.GetExportedTypes())
        {
            if (type.Name[0] == '<')
            {
                continue;
            }

            var typeName = Normalize(type.FullName ?? type.Name);
            types.Add(typeName, type);
        }

        return types;
    }

    /// <summary>Returns the sorted keys from a normalized type map.</summary>
    /// <param name="types">The type map.</param>
    /// <returns>The sorted type names.</returns>
    private static string[] GetSortedKeys(Dictionary<string, Type> types)
    {
        var names = new string[types.Count];
        types.Keys.CopyTo(names, 0);
        Array.Sort(names, StringComparer.Ordinal);
        return names;
    }

    /// <summary>Normalizes the namespace difference introduced by <c>REACTIVE_SHIM</c>.</summary>
    /// <param name="value">The signature to normalize.</param>
    /// <returns>The normalized signature.</returns>
    private static string Normalize(string value) =>
        value
            .Replace(ReactiveNamespacePrefix, LeanNamespacePrefix)
            .Replace(ReactiveUnitTypeName, LeanVoidTypeName);
}
