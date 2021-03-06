﻿using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("EPS.Web")]
[assembly: AssemblyDescription("Helpers for web applications")]
[assembly: AssemblyProduct("EPS.Web")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("02f7fc81-c30c-429b-be60-b65af8ab62ce")]

[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Target = "EPS.Web.Abstractions", Scope = "namespace", Justification = "Helpers mirror .NET framework type layout")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Target = "EPS.Web.Routing", Scope = "namespace", Justification = "Helpers mirror .NET framework type layout")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Target = "EPS.Runtime.Caching", Scope = "namespace", Justification = "Helpers mirror .NET framework type layout")]
