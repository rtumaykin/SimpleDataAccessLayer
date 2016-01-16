using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.Xml;
using EnvDTE;
using System.Linq;
using EnvDTE80;
using ICSharpCode.NRefactory.CSharp;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SimpleDataAccessLayer.Common.codegen;
using SimpleDataAccessLayer.Common.config.models;
using SimpleDataAccessLayer.Common.wizard;
using Formatting = Newtonsoft.Json.Formatting;
using SelectionContainer = Microsoft.VisualStudio.Shell.SelectionContainer;

namespace SimpleDataAccessLayer.vs2015
{
    /// <summary>
    /// This control host the editor (an extended RichTextBox) and is responsible for
    /// handling the commands targeted to the editor as well as saving and loading
    /// the document. This control also implement the search and replace functionalities.
    /// </summary>

    ///////////////////////////////////////////////////////////////////////////////
    // Having an entry in the new file dialog.
    //
    // For our file type should appear under "General" in the new files dialog, we need the following:-
    //     - A .vsdir file in the same directory as NewFileItems.vsdir (generally under Common7\IDE\NewFileItems).
    //       In our case the file name is Editor.vsdir but we only require a file with .vsdir extension.
    //     - An empty dal file in the same directory as NewFileItems.vsdir. In
    //       our case we chose DataAccessLayer.dal. Note this file name appears in Editor.vsdir
    //       (see vsdir file format below)
    //     - Three text strings in our language specific resource. File Resources.resx :-
    //          - "Rich Text file" - this is shown next to our icon.
    //          - "A blank rich text file" - shown in the description window
    //             in the new file dialog.
    //          - "DataAccessLayer" - This is the base file name. New files will initially
    //             be named as DataAccessLayer1.dal, DataAccessLayer2.dal... etc.
    ///////////////////////////////////////////////////////////////////////////////
    // Editor.vsdir contents:-
    //    DataAccessLayer.dal|{3085E1D6-A938-478e-BE49-3546C09A1AB1}|#106|80|#109|0|401|0|#107
    //
    // The fields in order are as follows:-
    //    - DataAccessLayer.dal - our empty dal file
    //    - {db16ff5e-400a-4cb7-9fde-cb3eab9d22d2} - our Editor package guid
    //    - #106 - the ID of "Rich Text file" in the resource
    //    - 80 - the display ordering priority
    //    - #109 - the ID of "A blank rich text file" in the resource
    //    - 0 - resource dll string (we don't use this)
    //    - 401 - the ID of our icon
    //    - 0 - various flags (we don't use this - se vsshell.idl)
    //    - #107 - the ID of "dal"
    ///////////////////////////////////////////////////////////////////////////////

    //This is required for Find In files scenario to work properly. This provides a connection point 
    //to the event interface
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [ComSourceInterfaces(typeof(IVsTextViewEvents))]
    [ComVisible(true)]
    public sealed class EditorPane : WindowPane,
                                IVsPersistDocData,  //to Enable persistence functionality for document data
                                IPersistFileFormat, //to enable the programmatic loading or saving of an object 
                                                    //in a format specified by the user.
                                IVsFileChangeEvents,//to notify the client when file changes on disk
                                IVsDocDataFileChangeControl, //to Determine whether changes to files made outside 
                                                             //of the editor should be ignored
                                IVsFileBackup//,      //to support backup of files. Visual Studio File Recovery 
                                             //backs up all objects in the Running Document Table that 
                                             //support IVsFileBackup and have unsaved changes.
                                             //needed for Find and Replace to work appropriately
                                             //                            IExtensibleObject  //so we can get the automation object
                                             //                                IVsStatusbarUser,   //support updating the status bar
                                             //                                IEditor,  //the automation interface for Editor
                                             //                                IVsToolboxUser      //Sends notification about Toolbox items to the owner of these items
    {
        private const uint MyFormat = 0;
        private const string MyExtension = ".dal";
        private bool _fileFormatValid = true;

        public bool IsFileFormatValid
        {
            get
            {
                // I don't need to lock it, since this property will be accessed way after it is assigned.
                return _fileFormatValid;
            }
        }

        private class EditorProperties
        {
            private readonly EditorPane _editor;
            public EditorProperties(EditorPane editor)
            {
                _editor = editor;
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public string FileName
            {
                get { return _editor.FileName; }
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public bool DataChanged
            {
                get { return _editor.DataChanged; }
            }
        }

        #region Fields
        private SimpleDataAccessLayerPackage _myPackage;

/*        private IVsWritableSettingsStore _settingsStore = null;
        private IVsWritableSettingsStore SettingsStore
        {
            get
            {
                if (_settingsStore == null)
                {
                    // Get a reference to the DTE 
                    var dte = _myPackage.GetEnvDTE();

                    // Get the settings manager from the DTE. 
                    var serviceProvider = new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider) dte);

                    var settingsManager = serviceProvider.GetService(typeof(SVsSettingsManager)) as IVsSettingsManager;

                    // Write the user settings to _settingsStore.
                    if (settingsManager != null)
                        settingsManager.GetWritableSettingsStore(
                            (uint)__VsSettingsScope.SettingsScope_UserSettings,
                            out _settingsStore);
                }
                return _settingsStore;
            }
        }
*/

        private string _fileName = string.Empty;
        private bool _isDirty;
        // Flag true when we are loading the file. It is used to avoid to change the isDirty flag
        // when the changes are related to the load operation.
        private bool _loading;
        // This flag is true when we are asking the QueryEditQuerySave service if we can edit the
        // file. It is used to avoid to have more than one request queued.
        private bool _gettingCheckoutStatus;
        private MainEditorWindow _editorControl;

        private SelectionContainer _selContainer;
        private ITrackSelection _trackSel;
        private IVsFileChangeEx _vsFileChangeEx;

        private Timer _fileChangeTrigger = new Timer();

        // Don't use status bar
        //        private Timer FNFStatusbarTrigger = new Timer();

        private bool _fileChangedTimerSet;
        private int _ignoreFileChangeLevel;
        private bool _backupObsolete = true;
        private uint _vsFileChangeCookie;
        //          private string[] fontListArray;


        private IExtensibleObjectSite _extensibleObjectSite;

        #endregion

        #region "Window.Pane Overrides"
        /// <summary>
        /// Constructor that calls the Microsoft.VisualStudio.Shell.WindowPane constructor then
        /// our initialization functions.
        /// </summary>
        /// <param name="package">Our Package instance.</param>
        public EditorPane(SimpleDataAccessLayerPackage package)
            : base(null)
        {
            PrivateInit(package);
        }

        /// <summary>
        /// This is a required override from the Microsoft.VisualStudio.Shell.WindowPane class.
        /// It returns the extended rich text box that we host.
        /// </summary>
        public override IWin32Window Window => _editorControl;

        #endregion

        /// <summary>
        /// Initialization routine for the Editor. Loads the list of properties for the dal document 
        /// which will show up in the properties window 
        /// </summary>
        /// <param name="package"></param>
        private void PrivateInit(SimpleDataAccessLayerPackage package)
        {
            _myPackage = package;
            _loading = false;
            _gettingCheckoutStatus = false;
            _trackSel = null;

            Control.CheckForIllegalCrossThreadCalls = false;
            // Create an ArrayList to store the objects that can be selected
            var listObjects = new ArrayList();

            // Create the object that will show the document's properties
            // on the properties window.
            var prop = new EditorProperties(this);
            listObjects.Add(prop);

            // Create the SelectionContainer object.
            _selContainer = new SelectionContainer(true, false)
            {
                SelectableObjects = listObjects,
                SelectedObjects = listObjects
            };

            // Create and initialize the editor

            var resources = new ComponentResourceManager(typeof(EditorPane));
            _editorControl = new MainEditorWindow();

            //resources.ApplyResources(_editorControl, "editorControl", CultureInfo.CurrentUICulture);

            // Handle Focus event
            // I should override this one
            //this.editorControl.RichTextBoxControl.GotFocus += new EventHandler(this.OnGotFocus);

            // Call the helper function that will do all of the command setup work
            // -- no commands here // setupCommands();
        }

        /// <summary>
        /// returns the name of the file currently loaded
        /// </summary>
        public string FileName => _fileName;

        /// <summary>
        /// returns whether the contents of file have changed since the last save
        /// </summary>
        public bool DataChanged => _isDirty;

        /// <summary>
        /// returns an instance of the ITrackSelection service object
        /// </summary>
        private ITrackSelection TrackSelection
        {
            get { return _trackSel ?? (_trackSel = (ITrackSelection)GetService(typeof(ITrackSelection))); }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    /* this will need to be overwritten in my implementation
					if (this.editorControl != null && this.editorControl.RichTextBoxControl != null)
					{
						this.editorControl.RichTextBoxControl.TextChanged -= new System.EventHandler(this.OnTextChange);
						this.editorControl.RichTextBoxControl.MouseDown -= new MouseEventHandler(this.OnMouseClick);
						this.editorControl.RichTextBoxControl.SelectionChanged -= new EventHandler(this.OnSelectionChanged);
						this.editorControl.RichTextBoxControl.KeyDown -= new KeyEventHandler(this.OnKeyDown);
						this.editorControl.RichTextBoxControl.GotFocus -= new EventHandler(this.OnGotFocus);
					}
					*/
                    // Dispose the timers
                    if (null != _fileChangeTrigger)
                    {
                        _fileChangeTrigger.Dispose();
                        _fileChangeTrigger = null;
                    }

                    SetFileChangeNotification(null, false);

                    if (_editorControl != null)
                    {
                        // editorControl.RichTextBoxControl.Dispose();
                        _editorControl.Dispose();
                        _editorControl = null;
                    }
                    if (_fileChangeTrigger != null)
                    {
                        _fileChangeTrigger.Dispose();
                        _fileChangeTrigger = null;
                    }
                    if (_extensibleObjectSite != null)
                    {
                        _extensibleObjectSite.NotifyDelete(this);
                        _extensibleObjectSite = null;
                    }
                    GC.SuppressFinalize(this);
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Gets an instance of the RunningDocumentTable (RDT) service which manages the set of currently open 
        /// documents in the environment and then notifies the client that an open document has changed
        /// </summary>
        private void NotifyDocChanged()
        {
            // Make sure that we have a file name
            if (_fileName.Length == 0)
                return;

            // Get a reference to the Running Document Table
            var runningDocTable = (IVsRunningDocumentTable)GetService(typeof(SVsRunningDocumentTable));

            var docData = IntPtr.Zero;

            try
            {
                // Lock the document
                uint docCookie;
                uint itemId;
                IVsHierarchy hierarchy;
                var hr = runningDocTable.FindAndLockDocument(
                    (uint)_VSRDTFLAGS.RDT_ReadLock,
                    _fileName,
                    out hierarchy,
                    out itemId,
                    out docData,
                    out docCookie
                );

                ErrorHandler.ThrowOnFailure(hr);

                // Send the notification
                hr = runningDocTable.NotifyDocumentChanged(docCookie, (uint)__VSRDTATTRIB.RDTA_DocDataReloaded);

                // Unlock the document.
                // Note that we have to unlock the document even if the previous call failed.
                ErrorHandler.ThrowOnFailure(runningDocTable.UnlockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, docCookie));

                // Check ff the call to NotifyDocChanged failed.
                ErrorHandler.ThrowOnFailure(hr);
            }
            finally
            {
                if (docData != IntPtr.Zero)
                    Marshal.Release(docData);
            }
        }

        int IPersist.GetClassID(out Guid pClassId)
        {
            pClassId = GuidList.GuidSimpleDataAccessLayerVs2015EditorFactory;
            return VSConstants.S_OK;
        }

        #region IPersistFileFormat Members

        /// <summary>
        /// Notifies the object that it has concluded the Save transaction
        /// </summary>
        /// <param name="pszFilename">Pointer to the file name</param>
        /// <returns>S_OK if the function succeeds</returns>
        int IPersistFileFormat.SaveCompleted(string pszFilename)
        {
            // TODO:  Add Editor.SaveCompleted implementation
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Returns the path to the object's current working file 
        /// </summary>
        /// <param name="ppszFilename">Pointer to the file name</param>
        /// <param name="pnFormatIndex">Value that indicates the current format of the file as a zero based index
        /// into the list of formats. Since we support only a single format, we need to return zero. 
        /// Subsequently, we will return a single element in the format list through a call to GetFormatList.</param>
        /// <returns></returns>
        int IPersistFileFormat.GetCurFile(out string ppszFilename, out uint pnFormatIndex)
        {
            // We only support 1 format so return its index
            pnFormatIndex = MyFormat;
            ppszFilename = _fileName;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Initialization for the object 
        /// </summary>
        /// <param name="nFormatIndex">Zero based index into the list of formats that indicates the current format 
        /// of the file</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.InitNew(uint nFormatIndex)
        {
            if (nFormatIndex != MyFormat)
            {
                return VSConstants.E_INVALIDARG;
            }
            // until someone change the file, we can consider it not dirty as
            // the user would be annoyed if we prompt him to save an empty file
            _isDirty = false;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Returns the class identifier of the editor type
        /// </summary>
        /// <param name="pClassId">pointer to the class identifier</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.GetClassID(out Guid pClassId)
        {
            ErrorHandler.ThrowOnFailure(((IPersist)this).GetClassID(out pClassId));
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Provides the caller with the information necessary to open the standard common "Save As" dialog box. 
        /// This returns an enumeration of supported formats, from which the caller selects the appropriate format. 
        /// Each string for the format is terminated with a newline (\n) character. 
        /// The last string in the buffer must be terminated with the newline character as well. 
        /// The first string in each pair is a display string that describes the filter, such as "Text Only 
        /// (*.txt)". The second string specifies the filter pattern, such as "*.txt". To specify multiple filter 
        /// patterns for a single display string, use a semicolon to separate the patterns: "*.htm;*.html;*.asp". 
        /// A pattern string can be a combination of valid file name characters and the asterisk (*) wildcard character. 
        /// Do not include spaces in the pattern string. The following string is an example of a file pattern string: 
        /// "HTML File (*.htm; *.html; *.asp)\n*.htm;*.html;*.asp\nText File (*.txt)\n*.txt\n."
        /// </summary>
        /// <param name="ppszFormatList">Pointer to a string that contains pairs of format filter strings</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.GetFormatList(out string ppszFormatList)
        {
            const char endline = '\n';
            var formatList = string.Format(CultureInfo.InvariantCulture, "My Editor (*{0}){1}*{0}{1}{1}", MyExtension, endline);
            ppszFormatList = formatList;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Loads the file content into the textbox
        /// </summary>
        /// <param name="pszFilename">Pointer to the full path name of the file to load</param>
        /// <param name="grfMode">file format mode</param>
        /// <param name="fReadOnly">determines if the file should be opened as read only</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.Load(string pszFilename, uint grfMode, int fReadOnly)
        {
            if (pszFilename == null)
            {
                return VSConstants.E_INVALIDARG;
            }

            _loading = true;
            try
            {
                // Show the wait cursor while loading the file
                var vsUiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                int hr;
                if (vsUiShell != null)
                {
                    // Note: we don't want to throw or exit if this call fails, so
                    // don't check the return code.
                    hr = vsUiShell.SetWaitCursor();
                }

                var configSerialized = File.ReadAllText(pszFilename);
                _editorControl.Init(configSerialized);
                var authType = _editorControl.Config?.DesignerConnection?.Authentication?.AuthenticationType;

                var uniqueId = GetUniqueId(pszFilename);

                var savedPassword = _myPackage.Passwords.ContainsKey(uniqueId)
                    ? _myPackage.Passwords[uniqueId]
                    : null;


                if (authType != null && authType.Value == AuthenticationType.SqlAuthentication)
                    // if we have a password, use it
                    if (!string.IsNullOrWhiteSpace(savedPassword))
                    {
                        _editorControl.Config.DesignerConnection.Authentication.Password = savedPassword;
                    }
                    // if the loaded password is not blank and it is a default password, blank it.
                    else if (
                        !string.IsNullOrWhiteSpace(_editorControl.Config.DesignerConnection.Authentication.Password) &&
                        _editorControl.Config.DesignerConnection.Authentication.Password ==
                        "***saved in the user options (.suo) file***")
                    {
                        _editorControl.Config.DesignerConnection.Authentication.Password = string.Empty;
                    }
                    // if the loaded password is not blank and it is not a default password, use it.

                _isDirty = false;

                //Determine if the file is read only on the file system
                var fileAttrs = File.GetAttributes(pszFilename);

                var isReadOnly = (int)fileAttrs & (int)FileAttributes.ReadOnly;

                //Set readonly if either the file is readonly for the user or on the file system
                if (0 == isReadOnly && 0 == fReadOnly)
                    SetReadOnly(false);
                else
                    SetReadOnly(true);


                // Notify to the property window that some of the selected objects are changed
                var track = TrackSelection;
                if (null != track)
                {
                    hr = track.OnSelectChange(_selContainer);
                    if (ErrorHandler.Failed(hr))
                        return hr;
                }

                // Hook up to file change notifications
                if (String.IsNullOrEmpty(_fileName) || 0 != String.Compare(_fileName, pszFilename, true, CultureInfo.CurrentCulture))
                {
                    _fileName = pszFilename;
                    SetFileChangeNotification(pszFilename, true);

                    // Notify the load or reload
                    NotifyDocChanged();
                }
            }
            finally
            {
                _loading = false;
            }
            return VSConstants.S_OK;
        }



        /// <summary>
        /// Determines whether an object has changed since being saved to its current file
        /// </summary>
        /// <param name="pfIsDirty">true if the document has changed</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.IsDirty(out int pfIsDirty)
        {
            pfIsDirty = _isDirty ? 1 : 0;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Save the contents of the textbox into the specified file. If doing the save on the same file, we need to
        /// suspend notifications for file changes during the save operation.
        /// </summary>
        /// <param name="pszFilename">Pointer to the file name. If the pszFilename parameter is a null reference 
        /// we need to save using the current file
        /// </param>
        /// <param name="fRemember">Boolean value that indicates whether the pszFileName parameter is to be used 
        /// as the current working file.
        /// If remember != 0, pszFileName needs to be made the current file and the dirty flag needs to be cleared after the save.
        ///                   Also, file notifications need to be enabled for the new file and disabled for the old file 
        /// If remember == 0, this save operation is a Save a Copy As operation. In this case, 
        ///                   the current file is unchanged and dirty flag is not cleared
        /// </param>
        /// <param name="nFormatIndex">Zero based index into the list of formats that indicates the format in which 
        /// the file will be saved</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.Save(string pszFilename, int fRemember, uint nFormatIndex)
        {
            var hr = VSConstants.S_OK;
            var doingSaveOnSameFile = false;
            // If file is null or same --> SAVE
            if (pszFilename == null || pszFilename == _fileName)
            {
                fRemember = 1;
                doingSaveOnSameFile = true;
            }

            //Suspend file change notifications for only Save since we don't have notifications setup
            //for SaveAs and SaveCopyAs (as they are different files)
            if (doingSaveOnSameFile)
                SuspendFileChangeNotification(pszFilename, 1);

            try
            {
                var configForSaving = _editorControl.Config.Clone();
                var configProjectItem = _myPackage.GetEnvDTE().Solution.FindProjectItem(pszFilename);

                if (configForSaving.DesignerConnection.Authentication.AuthenticationType == AuthenticationType.SqlAuthentication)
                {
                    var sqlAuth = configForSaving.DesignerConnection.Authentication;
                    if (sqlAuth.SavePassword)
                    {
                        var uniqueId = GetUniqueId(pszFilename);
                        if (_myPackage.Passwords.ContainsKey(uniqueId))
                        {
                            _myPackage.Passwords[uniqueId] = sqlAuth.Password;
                        }
                        else
                        {
                            _myPackage.Passwords.Add(uniqueId, sqlAuth.Password);
                        }
                    }

                    sqlAuth.Password = "***saved in the user options (.suo) file***";
                }


                var configSerialized = JsonConvert.SerializeObject(configForSaving, Formatting.Indented, new StringEnumConverter());
                File.WriteAllText(_fileName, configSerialized);

                var configProjectItemChildren = configProjectItem.ProjectItems;

                var codeProjectItem =
                    configProjectItemChildren
                        .Cast<ProjectItem>()
                        .FirstOrDefault(item => item.Name.ToUpper().EndsWith(".cs".ToUpper()));

                if (codeProjectItem != null)
                {
                    var codeFileName = codeProjectItem.FileNames[0];
                    SuspendFileChangeNotification(codeFileName, 1);
                    try
                    {
                        var formattingOptions = FormattingOptionsFactory.CreateAllman();
                        formattingOptions.AlignElseInIfStatements = true;
                        formattingOptions.ArrayInitializerWrapping = Wrapping.WrapIfTooLong;
                        formattingOptions.ChainedMethodCallWrapping = Wrapping.WrapIfTooLong;
                        formattingOptions.AutoPropertyFormatting = PropertyFormatting.ForceOneLine;
                        formattingOptions.IndexerArgumentWrapping = Wrapping.WrapIfTooLong;
                        formattingOptions.IndexerClosingBracketOnNewLine = NewLinePlacement.SameLine;
                        formattingOptions.IndexerDeclarationClosingBracketOnNewLine = NewLinePlacement.SameLine;
                        formattingOptions.IndexerDeclarationParameterWrapping = Wrapping.WrapIfTooLong;
                        formattingOptions.MethodCallArgumentWrapping = Wrapping.WrapIfTooLong;
                        formattingOptions.MethodCallClosingParenthesesOnNewLine = NewLinePlacement.SameLine;
                        formattingOptions.MethodDeclarationClosingParenthesesOnNewLine = NewLinePlacement.SameLine;
                        formattingOptions.MethodDeclarationParameterWrapping = Wrapping.WrapIfTooLong;

                        formattingOptions.NewLineAferIndexerDeclarationOpenBracket = NewLinePlacement.SameLine;
                        formattingOptions.NewLineAferIndexerOpenBracket = NewLinePlacement.SameLine;
                        formattingOptions.NewLineAferMethodCallOpenParentheses = NewLinePlacement.SameLine;
                        formattingOptions.NewLineAferMethodDeclarationOpenParentheses = NewLinePlacement.SameLine;

                        // determine if the framework supports async
                        var project = (uint) codeProjectItem.ContainingProject.Properties.Item("TargetFramework").Value;
                        var supportsAsync = (project >> 16 > 4) || ((project >> 16 == 4) && (project - ((project >> 16) << 16) >= 5));

                        var formattedCode =
                            new CSharpFormatter(FormattingOptionsFactory.CreateAllman()).Format(
                                new Main(_editorControl.Config, supportsAsync).GetCode());

                        File.WriteAllText(codeFileName, formattedCode);
                    }
                    finally
                    {
                        SuspendFileChangeNotification(codeFileName, 0);
                    }
                }

            }
            catch (ArgumentException)
            {
                hr = VSConstants.E_FAIL;
            }
            catch (IOException)
            {
                hr = VSConstants.E_FAIL;
            }
            finally
            {
                //restore the file change notifications
                if (doingSaveOnSameFile)
                    SuspendFileChangeNotification(pszFilename, 0);
            }

            if (VSConstants.E_FAIL == hr)
                return hr;

            //Save and Save as
            if (fRemember != 0)
            {
                //Save as
                if (null != pszFilename && !_fileName.Equals(pszFilename))
                {
                    SetFileChangeNotification(_fileName, false); //remove notification from old file
                    SetFileChangeNotification(pszFilename, true); //add notification for new file
                    _fileName = pszFilename;     //cache the new file name
                }
                _isDirty = false;
                SetReadOnly(false);             //set read only to false since you were successfully able
                                                //to save to the new file                                                    
            }

            var track = TrackSelection;
            if (null != track)
            {
                hr = track.OnSelectChange(_selContainer);
            }

            // Since all changes are now saved properly to disk, there's no need for a backup.
            _backupObsolete = false;
            return hr;
        }

        #endregion


        #region IVsPersistDocData Members

        /// <summary>
        /// Used to determine if the document data has changed since the last time it was saved
        /// </summary>
        /// <param name="pfDirty">Will be set to 1 if the data has changed</param>
        /// <returns>S_OK if the function succeeds</returns>
        int IVsPersistDocData.IsDocDataDirty(out int pfDirty)
        {
            return ((IPersistFileFormat)this).IsDirty(out pfDirty);
        }

        /// <summary>
        /// Saves the document data. Before actually saving the file, we first need to indicate to the environment
        /// that a file is about to be saved. This is done through the "SVsQueryEditQuerySave" service. We call the
        /// "QuerySaveFile" function on the service instance and then proceed depending on the result returned as follows:
        /// If result is QSR_SaveOK - We go ahead and save the file and the file is not read only at this point.
        /// If result is QSR_ForceSaveAs - We invoke the "Save As" functionality which will bring up the Save file name 
        ///                                dialog 
        /// If result is QSR_NoSave_Cancel - We cancel the save operation and indicate that the document could not be saved
        ///                                by setting the "pfSaveCanceled" flag
        /// If result is QSR_NoSave_Continue - Nothing to do here as the file need not be saved
        /// </summary>
        /// <param name="dwSave">Flags which specify the file save options:
        /// VSSAVE_Save        - Saves the current file to itself.
        /// VSSAVE_SaveAs      - Prompts the User for a filename and saves the file to the file specified.
        /// VSSAVE_SaveCopyAs  - Prompts the user for a filename and saves a copy of the file with a name specified.
        /// VSSAVE_SilentSave  - Saves the file without prompting for a name or confirmation.  
        /// </param>
        /// <param name="pbstrMkDocumentNew">Pointer to the path to the new document</param>
        /// <param name="pfSaveCanceled">value 1 if the document could not be saved</param>
        /// <returns></returns>
        int IVsPersistDocData.SaveDocData(VSSAVEFLAGS dwSave, out string pbstrMkDocumentNew, out int pfSaveCanceled)
        {
            pbstrMkDocumentNew = null;
            pfSaveCanceled = 0;
            int hr;

            switch (dwSave)
            {
                case VSSAVEFLAGS.VSSAVE_Save:
                case VSSAVEFLAGS.VSSAVE_SilentSave:
                    {
                        var queryEditQuerySave = (IVsQueryEditQuerySave2)GetService(typeof(SVsQueryEditQuerySave));

                        // Call QueryEditQuerySave
                        uint result = 0;
                        hr = queryEditQuerySave.QuerySaveFile(
                                _fileName,        // filename
                                0,    // flags
                                null,            // file attributes
                                out result);    // result
                        if (ErrorHandler.Failed(hr))
                            return hr;

                        // Process according to result from QuerySave
                        switch ((tagVSQuerySaveResult)result)
                        {
                            case tagVSQuerySaveResult.QSR_NoSave_Cancel:
                                // Note that this is also case tagVSQuerySaveResult.QSR_NoSave_UserCanceled because these
                                // two tags have the same value.
                                pfSaveCanceled = ~0;
                                break;

                            case tagVSQuerySaveResult.QSR_SaveOK:
                                {
                                    // Call the shell to do the save for us
                                    var uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                                    hr = uiShell.SaveDocDataToFile(dwSave, this, _fileName, out pbstrMkDocumentNew, out pfSaveCanceled);
                                    if (ErrorHandler.Failed(hr))
                                        return hr;
                                }
                                break;

                            case tagVSQuerySaveResult.QSR_ForceSaveAs:
                                {
                                    // Call the shell to do the SaveAS for us
                                    var uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                                    hr = uiShell.SaveDocDataToFile(VSSAVEFLAGS.VSSAVE_SaveAs, this, _fileName, out pbstrMkDocumentNew, out pfSaveCanceled);
                                    if (ErrorHandler.Failed(hr))
                                        return hr;
                                }
                                break;

                            case tagVSQuerySaveResult.QSR_NoSave_Continue:
                                // In this case there is nothing to do.
                                break;

                            default:
                                throw new NotSupportedException("Unsupported result from QEQS");
                        }
                        break;
                    }
                case VSSAVEFLAGS.VSSAVE_SaveAs:
                case VSSAVEFLAGS.VSSAVE_SaveCopyAs:
                    {
                        // Make sure the file name as the right extension
                        if (String.Compare(MyExtension, Path.GetExtension(_fileName), true, CultureInfo.CurrentCulture) != 0)
                        {
                            _fileName += MyExtension;
                        }
                        // Call the shell to do the save for us
                        var uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                        hr = uiShell.SaveDocDataToFile(dwSave, this, _fileName, out pbstrMkDocumentNew, out pfSaveCanceled);
                        if (ErrorHandler.Failed(hr))
                            return hr;
                        break;
                    }
                default:
                    throw new ArgumentException("Unsupported Save flag");
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Loads the document data from the file specified
        /// </summary>
        /// <param name="pszMkDocument">Path to the document file which needs to be loaded</param>
        /// <returns>S_Ok if the method succeeds</returns>
        int IVsPersistDocData.LoadDocData(string pszMkDocument)
        {
            return ((IPersistFileFormat)this).Load(pszMkDocument, 0, 0);
        }

        /// <summary>
        /// Used to set the initial name for unsaved, newly created document data
        /// </summary>
        /// <param name="pszDocDataPath">String containing the path to the document. We need to ignore this parameter
        /// </param>
        /// <returns>S_OK if the method succeeds</returns>
        int IVsPersistDocData.SetUntitledDocPath(string pszDocDataPath)
        {
            return ((IPersistFileFormat)this).InitNew(MyFormat);
        }

        /// <summary>
        /// Returns the Guid of the editor factory that created the IVsPersistDocData object
        /// </summary>
        /// <param name="pClassId">Pointer to the class identifier of the editor type</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IVsPersistDocData.GetGuidEditorType(out Guid pClassId)
        {
            return ((IPersistFileFormat)this).GetClassID(out pClassId);
        }

        /// <summary>
        /// Close the IVsPersistDocData object
        /// </summary>
        /// <returns>S_OK if the function succeeds</returns>
        int IVsPersistDocData.Close()
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Determines if it is possible to reload the document data
        /// </summary>
        /// <param name="pfReloadable">set to 1 if the document can be reloaded</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IVsPersistDocData.IsDocDataReloadable(out int pfReloadable)
        {
            // Allow file to be reloaded
            pfReloadable = 1;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Renames the document data
        /// </summary>
        /// <param name="grfAttribs"></param>
        /// <param name="pHierNew"></param>
        /// <param name="itemidNew"></param>
        /// <param name="pszMkDocumentNew"></param>
        /// <returns></returns>
        int IVsPersistDocData.RenameDocData(uint grfAttribs, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            // TODO:  Add EditorPane.RenameDocData implementation
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Reloads the document data
        /// </summary>
        /// <param name="grfFlags">Flag indicating whether to ignore the next file change when reloading the document data.
        /// This flag should not be set for us since we implement the "IVsDocDataFileChangeControl" interface in order to 
        /// indicate ignoring of file changes
        /// </param>
        /// <returns>S_OK if the method succeeds</returns>
        int IVsPersistDocData.ReloadDocData(uint grfFlags)
        {
            return ((IPersistFileFormat)this).Load(_fileName, grfFlags, 0);
        }

        /// <summary>
        /// Called by the Running Document Table when it registers the document data. 
        /// </summary>
        /// <param name="docCookie">Handle for the document to be registered</param>
        /// <param name="pHierNew">Pointer to the IVsHierarchy interface</param>
        /// <param name="itemidNew">Item identifier of the document to be registered from VSITEM</param>
        /// <returns></returns>
        int IVsPersistDocData.OnRegisterDocData(uint docCookie, IVsHierarchy pHierNew, uint itemidNew)
        {
            //Nothing to do here
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsFileChangeEvents Members

        /// <summary>
        /// Notify the editor of the changes made to one or more files
        /// </summary>
        /// <param name="cChanges">Number of files that have changed</param>
        /// <param name="rgpszFile">array of the files names that have changed</param>
        /// <param name="rggrfChange">Array of the flags indicating the type of changes</param>
        /// <returns></returns>
        int IVsFileChangeEvents.FilesChanged(uint cChanges, string[] rgpszFile, uint[] rggrfChange)
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "\t**** Inside FilesChanged ****"));

            //check the different parameters
            if (0 == cChanges || null == rgpszFile || null == rggrfChange)
                return VSConstants.E_INVALIDARG;

            //ignore file changes if we are in that mode
            if (_ignoreFileChangeLevel != 0)
                return VSConstants.S_OK;

            for (uint i = 0; i < cChanges; i++)
            {
                if (!String.IsNullOrEmpty(rgpszFile[i]) && String.Compare(rgpszFile[i], _fileName, true, CultureInfo.CurrentCulture) == 0)
                {
                    // if the readonly state (file attributes) have changed we can immediately update
                    // the editor to match the new state (either readonly or not readonly) immediately
                    // without prompting the user.
                    if (0 != (rggrfChange[i] & (int)_VSFILECHANGEFLAGS.VSFILECHG_Attr))
                    {
                        var fileAttrs = File.GetAttributes(_fileName);
                        var isReadOnly = (int)fileAttrs & (int)FileAttributes.ReadOnly;
                        SetReadOnly(isReadOnly != 0);
                    }
                    // if it looks like the file contents have changed (either the size or the modified
                    // time has changed) then we need to prompt the user to see if we should reload the
                    // file. it is important to not synchronously reload the file inside of this FilesChanged
                    // notification. first it is possible that there will be more than one FilesChanged 
                    // notification being sent (sometimes you get separate notifications for file attribute
                    // changing and file size/time changing). also it is the preferred UI style to not
                    // prompt the user until the user re-activates the environment application window.
                    // this is why we use a timer to delay prompting the user.
                    if (0 != (rggrfChange[i] & (int)(_VSFILECHANGEFLAGS.VSFILECHG_Time | _VSFILECHANGEFLAGS.VSFILECHG_Size)))
                    {
                        if (!_fileChangedTimerSet)
                        {
                            _fileChangeTrigger = new Timer();
                            _fileChangedTimerSet = true;
                            _fileChangeTrigger.Interval = 1000;
                            _fileChangeTrigger.Tick += OnFileChangeEvent;
                            _fileChangeTrigger.Enabled = true;
                        }
                    }
                }
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Notify the editor of the changes made to a directory
        /// </summary>
        /// <param name="pszDirectory">Name of the directory that has changed</param>
        /// <returns></returns>
        int IVsFileChangeEvents.DirectoryChanged(string pszDirectory)
        {
            //Nothing to do here
            return VSConstants.S_OK;
        }
        #endregion

        #region IVsDocDataFileChangeControl Members

        /// <summary>
        /// Used to determine whether changes to DocData in files should be ignored or not
        /// </summary>
        /// <param name="fIgnore">a non zero value indicates that the file changes should be ignored
        /// </param>
        /// <returns></returns>
        int IVsDocDataFileChangeControl.IgnoreFileChanges(int fIgnore)
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "\t **** Inside IgnoreFileChanges ****"));

            if (fIgnore != 0)
            {
                _ignoreFileChangeLevel++;
            }
            else
            {
                if (_ignoreFileChangeLevel > 0)
                    _ignoreFileChangeLevel--;

                // We need to check here if our file has changed from "Read Only"
                // to "Read/Write" or vice versa while the ignore level was non-zero.
                // This may happen when a file is checked in or out under source
                // code control. We need to check here so we can update our caption.
                var fileAttrs = File.GetAttributes(_fileName);
                var isReadOnly = (int)fileAttrs & (int)FileAttributes.ReadOnly;
                SetReadOnly(isReadOnly != 0);
            }
            return VSConstants.S_OK;
        }
        #endregion

        #region File Change Notification Helpers

        /// <summary>
        /// In this function we inform the shell when we wish to receive 
        /// events when our file is changed or we inform the shell when 
        /// we wish not to receive events anymore.
        /// </summary>
        /// <param name="pszFileName">File name string</param>
        /// <param name="fStart">TRUE indicates advise, FALSE indicates unadvise.</param>
        /// <returns>Result of the operation</returns>
        private int SetFileChangeNotification(string pszFileName, bool fStart)
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "\t **** Inside SetFileChangeNotification ****"));

            var result = VSConstants.E_FAIL;

            //Get the File Change service
            if (null == _vsFileChangeEx)
                _vsFileChangeEx = (IVsFileChangeEx)GetService(typeof(SVsFileChangeEx));
            if (null == _vsFileChangeEx)
                return VSConstants.E_UNEXPECTED;

            // Setup Notification if fStart is TRUE, Remove if fStart is FALSE.
            if (fStart)
            {
                if (_vsFileChangeCookie == VSConstants.VSCOOKIE_NIL)
                {
                    //Receive notifications if either the attributes of the file change or 
                    //if the size of the file changes or if the last modified time of the file changes
                    result = _vsFileChangeEx.AdviseFileChange(pszFileName,
                        (uint)(_VSFILECHANGEFLAGS.VSFILECHG_Attr | _VSFILECHANGEFLAGS.VSFILECHG_Size | _VSFILECHANGEFLAGS.VSFILECHG_Time),
                        this,
                        out _vsFileChangeCookie);
                    if (_vsFileChangeCookie == VSConstants.VSCOOKIE_NIL)
                        return VSConstants.E_FAIL;
                }
            }
            else
            {
                if (_vsFileChangeCookie != VSConstants.VSCOOKIE_NIL)
                {
                    result = _vsFileChangeEx.UnadviseFileChange(_vsFileChangeCookie);
                    _vsFileChangeCookie = VSConstants.VSCOOKIE_NIL;
                }
            }
            return result;
        }

        /// <summary>
        /// In this function we suspend receiving file change events for
        /// a file or we reinstate a previously suspended file depending
        /// on the value of the given fSuspend flag.
        /// </summary>
        /// <param name="pszFileName">File name string</param>
        /// <param name="fSuspend">TRUE indicates that the events needs to be suspended</param>
        /// <returns></returns>

        private int SuspendFileChangeNotification(string pszFileName, int fSuspend)
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "\t **** Inside SuspendFileChangeNotification ****"));

            if (null == _vsFileChangeEx)
                _vsFileChangeEx = (IVsFileChangeEx)GetService(typeof(SVsFileChangeEx));
            if (null == _vsFileChangeEx)
                return VSConstants.E_UNEXPECTED;

            if (0 == fSuspend)
            {
                // we are transitioning from suspended to non-suspended state - so force a
                // sync first to avoid asynchronous notifications of our own change
                if (_vsFileChangeEx.SyncFile(pszFileName) == VSConstants.E_FAIL)
                    return VSConstants.E_FAIL;
            }

            //If we use the VSCOOKIE parameter to specify the file, then pszMkDocument parameter 
            //must be set to a null reference and vice versa 
            return _vsFileChangeEx.IgnoreFile(_vsFileChangeCookie, null, fSuspend);
        }
        #endregion

        #region IVsFileBackup Members

        /// <summary>
        /// This method is used to Persist the data to a single file. On a successful backup this 
        /// should clear up the backup dirty bit
        /// </summary>
        /// <param name="pszBackupFileName">Name of the file to persist</param>
        /// <returns>S_OK if the data can be successfully persisted.
        /// This should return STG_S_DATALOSS or STG_E_INVALIDCODEPAGE if there is no way to 
        /// persist to a file without data loss
        /// </returns>
        int IVsFileBackup.BackupFile(string pszBackupFileName)
        {
            try
            {
                // implement this editorControl.RichTextBoxControl.SaveFile(pszBackupFileName);
                _backupObsolete = false;
            }
            catch (ArgumentException)
            {
                return VSConstants.E_FAIL;
            }
            catch (IOException)
            {
                return VSConstants.E_FAIL;
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Used to set the backup dirty bit. This bit should be set when the object is modified 
        /// and cleared on calls to BackupFile and any Save method
        /// </summary>
        /// <param name="pbObsolete">the dirty bit to be set</param>
        /// <returns>returns 1 if the backup dirty bit is set, 0 otherwise</returns>
        int IVsFileBackup.IsBackupFileObsolete(out int pbObsolete)
        {
            pbObsolete = _backupObsolete ? 1 : 0;
            return VSConstants.S_OK;
        }

        #endregion

        /// <summary>
        /// Used to ReadOnly property for the Rich TextBox and correspondingly update the editor caption
        /// </summary>
        /// <param name="isFileReadOnly">Indicates whether the file loaded is Read Only or not</param>
        private void SetReadOnly(bool isFileReadOnly)
        {
            // this.editorControl.RichTextBoxControl.ReadOnly = _isFileReadOnly;

            //update editor caption with "[Read Only]" or "" as necessary
            var frame = (IVsWindowFrame)GetService(typeof(SVsWindowFrame));
            var editorCaption = "";
            if (isFileReadOnly)
                editorCaption = GetResourceString("@100");
            ErrorHandler.ThrowOnFailure(frame.SetProperty((int)__VSFPROPID.VSFPROPID_EditorCaption, editorCaption));
            _backupObsolete = true;
        }

        /// <summary>
        /// This event is triggered when one of the files loaded into the environment has changed outside of the
        /// editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFileChangeEvent(object sender, EventArgs e)
        {
            //Disable the timer
            _fileChangeTrigger.Enabled = false;

            var message = GetResourceString("@101");    //get the message string from the resource
            var vsUiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            var result = 0;
            var tempGuid = Guid.Empty;
            if (vsUiShell != null)
            {
                //Show up a message box indicating that the file has changed outside of VS environment
                ErrorHandler.ThrowOnFailure(vsUiShell.ShowMessageBox(0, ref tempGuid, _fileName, message, null, 0,
                    OLEMSGBUTTON.OLEMSGBUTTON_YESNOCANCEL, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                    OLEMSGICON.OLEMSGICON_QUERY, 0, out result));
            }
            //if the user selects "Yes", reload the current file
            if (result == (int)DialogResult.Yes)
            {
                ErrorHandler.ThrowOnFailure(((IVsPersistDocData)this).ReloadDocData(0));
            }

            _fileChangedTimerSet = false;
        }

        /// <summary>
        /// This method loads a localized string based on the specified resource.
        /// </summary>
        /// <param name="resourceName">Resource to load</param>
        /// <returns>String loaded for the specified resource</returns>
        internal string GetResourceString(string resourceName)
        {
            string resourceValue;
            var resourceManager = (IVsResourceManager)GetService(typeof(SVsResourceManager));
            if (resourceManager == null)
            {
                throw new InvalidOperationException("Could not get SVsResourceManager service. Make sure the package is Sited before calling this method");
            }
            var packageGuid = _myPackage.GetType().GUID;
            var hr = resourceManager.LoadResourceString(ref packageGuid, -1, resourceName, out resourceValue);
            ErrorHandler.ThrowOnFailure(hr);
            return resourceValue;
        }

        /// <summary>
        /// This function asks to the QueryEditQuerySave service if it is possible to
        /// edit the file.
        /// </summary>
        private bool CanEditFile()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "\t**** CanEditFile called ****"));

            // Check the status of the recursion guard
            if (_gettingCheckoutStatus)
                return false;

            try
            {
                // Set the recursion guard
                _gettingCheckoutStatus = true;

                // Get the QueryEditQuerySave service
                var queryEditQuerySave = (IVsQueryEditQuerySave2)GetService(typeof(SVsQueryEditQuerySave));

                // Now call the QueryEdit method to find the edit status of this file
                string[] documents = { _fileName };
                uint result;
                uint outFlags;

                // Note that this function can popup a dialog to ask the user to checkout the file.
                // When this dialog is visible, it is possible to receive other request to change
                // the file and this is the reason for the recursion guard.
                var hr = queryEditQuerySave.QueryEditFiles(
                    0,              // Flags
                    1,              // Number of elements in the array
                    documents,      // Files to edit
                    null,           // Input flags
                    null,           // Input array of VSQEQS_FILE_ATTRIBUTE_DATA
                    out result,     // result of the checkout
                    out outFlags    // Additional flags
                );
                if (ErrorHandler.Succeeded(hr) && (result == (uint)tagVSQueryEditResult.QER_EditOK))
                {
                    // In this case (and only in this case) we can return true from this function.
                    return true;
                }
            }

            finally
            {
                _gettingCheckoutStatus = false;
            }
            return false;
        }

        /// <summary>
        /// This event is triggered when there contents of the file are changed inside the editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.ITrackSelection.OnSelectChange(Microsoft.VisualStudio.Shell.Interop.ISelectionContainer)")]
        private void OnTextChange(object sender, EventArgs e)
        {
            // During the load operation the text of the control will change, but
            // this change must not be stored in the status of the document.
            if (!_loading)
            {
                // The only interesting case is when we are changing the document
                // for the first time
                if (!_isDirty)
                {
                    // Check if the QueryEditQuerySave service allow us to change the file
                    if (!CanEditFile())
                    {
                        // We can not change the file (e.g. a checkout operation failed),
                        // so undo the change and exit.
                        //editorControl.RichTextBoxControl.Undo();
                        //						throw new NotImplementedException("ToDo");
                        return;
                    }

                    // It is possible to change the file, so update the status.
                    _isDirty = true;
                    var track = TrackSelection;
                    if (null != track)
                    {
                        // Note: here we don't need to check the return code.
                        track.OnSelectChange(_selContainer);
                    }
                    _backupObsolete = true;
                }
            }
        }
        /*
                /// <summary>
                /// This event is triggered when the control's GotFocus event is fired.
                /// </summary>
                /// <param name="sender"></param>
                /// <param name="e"></param>
                private void OnGotFocus(object sender, EventArgs e)
                {

                }
        */
        private string GetUniqueId(string pszFilename)
        {
            var solutionPath = Path.GetDirectoryName(_myPackage.GetEnvDTE()?.Solution?.FileName);
            return solutionPath != null ? new Uri(solutionPath).MakeRelativeUri(new Uri(pszFilename)).ToString() : null;
        }
    }
}

