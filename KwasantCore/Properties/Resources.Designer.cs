﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KwasantCore.Properties {
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("KwasantCore.Properties.Resources", typeof(Resources).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @{
        ///
        ///var serverURL = Utilities.Server.ServerUrl;
        ///
        ///var linkTo = serverURL;
        ///var imageURL = serverURL + &quot;Content/img/EmailLogo.png&quot;;
        ///if(Utilities.Server.IsDevMode)
        ///{
        ///imageURL = &quot;kwasant.com/Content/img/EmailLogo.png&quot;;
        ///}
        ///
        ///var customerID = Model.UserID;
        ///
        ///var time = Model.IsAllDay
        ///? &quot;All day - &quot; + Model.StartDate.ToString(&quot;ddd d MMM&quot;)
        ///: Model.StartDate.ToString(&quot;ddd MMM d, yyyy hh:mm tt&quot;) + &quot; - &quot; + Model.EndDate.ToString(&quot;hh:mm tt&quot;);
        ///
        ///var timeZone = System.TimeZone.CurrentTimeZone.GetUtcOffset(Dat [rest of string was truncated]&quot;;.
        /// </summary>
        public static string HTMLEventInvitation {
            get {
                return ResourceManager.GetString("HTMLEventInvitation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @{
        ///
        ///var serverURL = Utilities.Server.ServerUrl;
        ///
        ///var linkTo = serverURL;
        ///var imageURL = serverURL + &quot;Content/img/EmailLogo.png&quot;;
        ///if(Utilities.Server.IsDevMode)
        ///{
        ///imageURL = &quot;kwasant.com/Content/img/EmailLogo.png&quot;;
        ///}
        ///
        ///var customerID = Model.UserID;
        ///
        ///var time = Model.IsAllDay
        ///? &quot;All day - &quot; + Model.StartDate.ToString(&quot;ddd d MMM&quot;)
        ///: Model.StartDate.ToString(&quot;ddd MMM d, yyyy hh:mm tt&quot;) + &quot; - &quot; + Model.EndDate.ToString(&quot;hh:mm tt&quot;);
        ///
        ///var timeZone = System.TimeZone.CurrentTimeZone.GetUtcOffset(Dat [rest of string was truncated]&quot;;.
        /// </summary>
        public static string HTMLEventInvitation_Update {
            get {
                return ResourceManager.GetString("HTMLEventInvitation_Update", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap perfect_krawsant {
            get {
                object obj = ResourceManager.GetObject("perfect_krawsant", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @{
        ///var basicText = Utilities.ConfigRepository.Get(&quot;emailBasicText&quot;);
        ///var time = Model.IsAllDay
        ///                        ? &quot;All day - &quot; + Model.StartDate.ToString(&quot;ddd d MMM&quot;)
        ///                        : Model.StartDate.ToString(&quot;ddd MMM d, yyyy hh:mm tt&quot;) + &quot; - &quot; + Model.EndDate.ToString(&quot;hh:mm tt&quot;);
        ///
        ///var timeZone = System.TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
        ///}
        ///
        ///@basicText
        ///@Model.Summary
        ///@Model.Description
        ///*When*
        ///@time UTC+@timeZone
        ///*Where*
        ///@Model.Location
        ///*Who*
        ///@{
        ///foreach(var [rest of string was truncated]&quot;;.
        /// </summary>
        public static string PlainEventInvitation {
            get {
                return ResourceManager.GetString("PlainEventInvitation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @{
        ///var basicText = Utilities.ConfigRepository.Get(&quot;emailBasicText&quot;);
        ///var time = Model.IsAllDay
        ///                        ? &quot;All day - &quot; + Model.StartDate.ToString(&quot;ddd d MMM&quot;)
        ///                        : Model.StartDate.ToString(&quot;ddd MMM d, yyyy hh:mm tt&quot;) + &quot; - &quot; + Model.EndDate.ToString(&quot;hh:mm tt&quot;);
        ///
        ///var timeZone = System.TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
        ///}
        ///
        ///@basicText
        ///@Model.Summary
        ///@Model.Description
        ///*When*
        ///@time UTC+@timeZone
        ///*Where*
        ///@Model.Location
        ///*Who*
        ///@{
        ///foreach(var [rest of string was truncated]&quot;;.
        /// </summary>
        public static string PlainEventInvitation_Update {
            get {
                return ResourceManager.GetString("PlainEventInvitation_Update", resourceCulture);
            }
        }
    }
}
