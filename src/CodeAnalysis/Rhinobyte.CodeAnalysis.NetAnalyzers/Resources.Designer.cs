//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Rhinobyte.CodeAnalysis.NetAnalyzers {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Rhinobyte.CodeAnalysis.NetAnalyzers.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Type members should be ordered correcty into their respective groups..
        /// </summary>
        internal static string RBCS0001_AnalyzerDescription {
            get {
                return ResourceManager.GetString("RBCS0001_AnalyzerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Member &apos;{0}&apos; is not ordered correctly by group.
        /// </summary>
        internal static string RBCS0001_AnalyzerMessageFormat {
            get {
                return ResourceManager.GetString("RBCS0001_AnalyzerMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type members should be ordered by group.
        /// </summary>
        internal static string RBCS0001_AnalyzerTitle {
            get {
                return ResourceManager.GetString("RBCS0001_AnalyzerTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type members within the same group should be ordered correctly..
        /// </summary>
        internal static string RBCS0002_AnalyzerDescription {
            get {
                return ResourceManager.GetString("RBCS0002_AnalyzerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Member &apos;{0}&apos; is not ordered alphabetically or by configured priority.
        /// </summary>
        internal static string RBCS0002_AnalyzerMessageFormat {
            get {
                return ResourceManager.GetString("RBCS0002_AnalyzerMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type members within the same group should be ordered correctly.
        /// </summary>
        internal static string RBCS0002_AnalyzerTitle {
            get {
                return ResourceManager.GetString("RBCS0002_AnalyzerTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Member assignments in the object initializer are not ordered correctly..
        /// </summary>
        internal static string RBCS0003_AnalyzerDescription {
            get {
                return ResourceManager.GetString("RBCS0003_AnalyzerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The member assignments &apos;{0}&apos; are out of order.
        /// </summary>
        internal static string RBCS0003_AnalyzerMessageFormat {
            get {
                return ResourceManager.GetString("RBCS0003_AnalyzerMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Member assignments in an object initializer are ordered correctly.
        /// </summary>
        internal static string RBCS0003_AnalyzerTitle {
            get {
                return ResourceManager.GetString("RBCS0003_AnalyzerTitle", resourceCulture);
            }
        }
    }
}
