//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace bytepress.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("bytepress.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to using System;
        ///using System.IO;
        ///using System.IO.Compression;
        ///using System.Reflection;
        ///using System.Threading;
        ///using System.Runtime.InteropServices;
        ///
        ///
        ///[assembly: AssemblyTitle(&quot;&quot;)]
        ///[assembly: AssemblyDescription(&quot;&quot;)]
        ///[assembly: AssemblyCompany(&quot;&quot;)]
        ///[assembly: AssemblyProduct(&quot;&quot;)]
        ///[assembly: AssemblyCopyright(&quot;&quot;)]
        ///[assembly: ComVisible(false)]
        ///[assembly: Guid(&quot;&quot;)]
        ///[assembly: AssemblyVersion(&quot;&quot;)]
        ///[assembly: AssemblyFileVersion(&quot;&quot;)]
        ///
        ///namespace bytepress
        ///{
        ///    class Program
        ///    {
        ///        pri [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string source {
            get {
                return ResourceManager.GetString("source", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to var app = typeof(System.Windows.Application);
        ///var field = app.GetField(&quot;_resourceAssembly&quot;, BindingFlags.NonPublic | BindingFlags.Static);
        ///field.SetValue(null, assembly);
        ///
        ///var helper = typeof(System.Windows.Navigation.BaseUriHelper);
        ///var property = helper.GetProperty(&quot;ResourceAssembly&quot;, BindingFlags.NonPublic | BindingFlags.Static);
        ///property.SetValue(null, assembly, null);.
        /// </summary>
        internal static string wpfhack {
            get {
                return ResourceManager.GetString("wpfhack", resourceCulture);
            }
        }
    }
}
