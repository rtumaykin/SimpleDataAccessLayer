//------------------------------------------------------------------------------
// <copyright file="SimpleDataAccessLayer.cs" company="Roman Tumaykin">
//     Copyright (c) 2016 Roman Tumaykin.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;

namespace SimpleDataAccessLayer
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// <para>
    /// ProvideAutoLoad & Load/save options: http://jbrd.github.io/2007/02/11/visual-studio-2005-sdk-saving-custom-data-in-solution-user-options-suo-files-using-the-managed-package-framework.html
    /// </para>
    /// </remarks>
    [ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}")]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(GuidList.GuidSimpleDataAccessLayerPkgString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideEditorExtension(typeof(EditorFactory), ".dal", 50,
              ProjectGuid = "{A2FE74E1-B743-11d0-AE1A-00A0C90FFFC3}",
              TemplateDir = "Templates",
              NameResourceID = 105,
              DefaultName = "SimpleDataAccessLayer.Editor")]
    public sealed class SimpleDataAccessLayerPackage : Package, IVsPersistSolutionOpts
    {
        private Dictionary<string, string> _passwords;
        private const string PasswordsUserSection = "USER_PASSWORDS";

        internal Dictionary<string, string> Passwords
        {
            get
            {
                LazyInitializer.EnsureInitialized(ref _passwords, () => new Dictionary<string, string>());
                return _passwords;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDataAccessLayerPackage"/> class.
        /// </summary>
        public SimpleDataAccessLayerPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            AddOptionKey(PasswordsUserSection);
            RegisterEditorFactory(new EditorFactory(this));
        }

        /// <summary>
        /// this method makes the EnvDTE object public so I can use it in my editor
        /// </summary>
        /// <returns></returns>
        public EnvDTE.DTE GetEnvDTE()
        {
            return (EnvDTE.DTE) GetService(typeof (EnvDTE.DTE));
        }

        protected override void OnLoadOptions(string key, Stream stream)
        {
            if (key == PasswordsUserSection)
            {
                using (var bReader = new BinaryReader(stream))
                {
                    var passwordsSerialized = bReader.ReadString();
                    Passwords.Clear();
                    foreach (var password in JsonConvert.DeserializeObject<Dictionary<string, string>>(passwordsSerialized))
                    {
                        Passwords.Add(password.Key, password.Value);
                    }
                }
            }
            base.OnLoadOptions(key, stream);
        }

        protected override void OnSaveOptions(string key, Stream stream)
        {
            if (key == PasswordsUserSection)
            {
                var passwordsSerialized = JsonConvert.SerializeObject(Passwords);

                using (var bw = new BinaryWriter(stream))
                {
                    bw.Write(passwordsSerialized);
                }
            }
            base.OnSaveOptions(key, stream);
        }

        #endregion
    }
}
