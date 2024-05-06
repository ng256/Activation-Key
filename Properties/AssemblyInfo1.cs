using System.Runtime.CompilerServices;

/***********************************************************
Activation Key v. 2.0

The MIT License (MIT)
Copyright: © NG256 2021-2024.

Permission is  hereby granted, free of charge, to any person
obtaining   a copy    of    this  software    and associated
documentation  files  (the "Software"),    to  deal   in the
Software without  restriction, including without  limitation
the rights to use, copy, modify, merge, publish, distribute,
sublicense,  and/or  sell  copies   of  the Software, and to
permit persons to whom the Software  is furnished to  do so,
subject       to         the      following      conditions:

The above copyright  notice and this permission notice shall
be  included  in all copies   or substantial portions of the
Software.

THE  SOFTWARE IS  PROVIDED  "AS IS", WITHOUT WARRANTY OF ANY
KIND, EXPRESS  OR IMPLIED, INCLUDING  BUT NOT LIMITED TO THE
WARRANTIES  OF MERCHANTABILITY, FITNESS    FOR A  PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN  NO EVENT SHALL  THE AUTHORS
OR  COPYRIGHT HOLDERS  BE  LIABLE FOR ANY CLAIM,  DAMAGES OR
OTHER LIABILITY,  WHETHER IN AN  ACTION OF CONTRACT, TORT OR
OTHERWISE, ARISING FROM, OUT OF   OR IN CONNECTION  WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
************************************************************/

#if COMVISIBLE
using System.EnterpriseServices;
#endif
using System.Reflection;
using System.Runtime.InteropServices;

// General information about this assembly is provided by the following set
// attributes. Change the values of these attributes to change the information,
// related to the assembly. 
[assembly: AssemblyTitle("Activation Key Library 2.0")] // Assembly name. 
[assembly: AssemblyDescription("Activation Key 2.0 Library")] // Assembly description. 
[assembly: AssemblyCompany("NG256")] // Developer.
[assembly: AssemblyProduct("NG256 Activation Key")] // Product name.
[assembly: AssemblyCopyright("© NG256 2021-2024")] // Copyright.
[assembly: AssemblyTrademark("NG256® Activation Key®")] // Trademark.
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("2.0.2405.0007")]
[assembly: AssemblyFileVersion("2.0.2405.0007")]
#if DEBUG
//[assembly: InternalsVisibleTo("Test")]
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

// Setting ComVisible to False makes the types in this assembly invisible
// for COM components. If you need to refer to the type in this assembly via COM,
// set the ComVisible attribute to TRUE for this type. 
#if COMVISIBLE
[assembly: ComVisible(true)]
[assembly: ApplicationName("Activation Key")] // COM application name.
[assembly: ApplicationID("bd25db63-218c-40af-92d1-8f02b4e9a355")]
#else
[assembly: ComVisible(false)]
#endif
// The following GUID serves to identify the type library if this project will be visible to COM 
[assembly: Guid("28cdc5b3-3a32-413f-80ad-1a68da1871c8")]