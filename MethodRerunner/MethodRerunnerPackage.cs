using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace furaga.MethodRerunner
{
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidMethodRerunnerPkgString)]
    public sealed class MethodRerunnerPackage : Package
    {
        public MethodRerunnerPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                CommandID menuCommandID = new CommandID(GuidList.guidMethodRerunnerCmdSet, (int)PkgCmdIDList.cmdidSaveParamsCommand);
                MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID );
                mcs.AddCommand( menuItem );

                CommandID menuCommandID2 = new CommandID(GuidList.guidMethodRerunnerCmdSet, (int)PkgCmdIDList.cmdidInsertTestCode);
                MenuCommand menuItem2 = new MenuCommand(MenuItemCallback, menuCommandID2);
                mcs.AddCommand(menuItem2);
            }
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            if (sender is MenuCommand)
            {
                switch ((uint)(sender as MenuCommand).CommandID.ID)
                {
                    case PkgCmdIDList.cmdidSaveParamsCommand:
                        MethodRerun.SaveMethodParameter();
                        break;
                    case PkgCmdIDList.cmdidInsertTestCode:
                        MethodRerun.InsertMethodTestCode();
                        break;
                }
            }
        }
    }
}
