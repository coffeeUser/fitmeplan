﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Fitmeplan.IdentityServer {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Fitmeplan.IdentityServer.Resource", typeof(Resource).Assembly);
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
        ///   Looks up a localized string similar to Blocked..
        /// </summary>
        internal static string ErrorMessage_Blocked {
            get {
                return ResourceManager.GetString("ErrorMessage_Blocked", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You have input invalid email too many times. For the security reasons we have locked you. Please try to reset your password later..
        /// </summary>
        internal static string ErrorMessage_BlockedIp {
            get {
                return ResourceManager.GetString("ErrorMessage_BlockedIp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Email..
        /// </summary>
        internal static string ErrorMessage_Email {
            get {
                return ResourceManager.GetString("ErrorMessage_Email", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to send email, please try later..
        /// </summary>
        internal static string ErrorMessage_EmailSendingFailed {
            get {
                return ResourceManager.GetString("ErrorMessage_EmailSendingFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid token..
        /// </summary>
        internal static string ErrorMessage_InvalidToken {
            get {
                return ResourceManager.GetString("ErrorMessage_InvalidToken", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You have {0} attempts left..
        /// </summary>
        internal static string ErrorMessage_LoginAttemptsLeft {
            get {
                return ResourceManager.GetString("ErrorMessage_LoginAttemptsLeft", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is no user account with the provided email address or this account is not allowed to access the app..
        /// </summary>
        internal static string ErrorMessage_MobileWrongEmail {
            get {
                return ResourceManager.GetString("ErrorMessage_MobileWrongEmail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Reset failed..
        /// </summary>
        internal static string ErrorMessage_ResetFailed {
            get {
                return ResourceManager.GetString("ErrorMessage_ResetFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to change password, please try later..
        /// </summary>
        internal static string ErrorMessage_ResetPasswordFailed {
            get {
                return ResourceManager.GetString("ErrorMessage_ResetPasswordFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Token..
        /// </summary>
        internal static string ErrorMessage_Token {
            get {
                return ResourceManager.GetString("ErrorMessage_Token", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Token expired..
        /// </summary>
        internal static string ErrorMessage_TokenExpired {
            get {
                return ResourceManager.GetString("ErrorMessage_TokenExpired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is no user account with the provided email address..
        /// </summary>
        internal static string ErrorMessage_WrongEmail {
            get {
                return ResourceManager.GetString("ErrorMessage_WrongEmail", resourceCulture);
            }
        }
    }
}