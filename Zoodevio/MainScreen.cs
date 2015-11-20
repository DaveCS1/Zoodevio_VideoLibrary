﻿using System;
using System.Windows.Forms;
using Zoodevio.Managers;

namespace Zoodevio
{
    public partial class MainScreen : Form
    {
        private bool _searchViewToggle = true;
        private bool _metadataViewToggle = true;

        private MainScreenManager _mainManager;

        // application children control accessors
        public BasicSearchControl BasicSearchControl { get { return basicSearchControl; } }
        public GridViewControl GridViewControl { get { return gridViewControl; } }
        public LibraryPanelControl LibraryPanelControl { get { return libraryPanelControl; } }
        public MetadataViewControl MetadataViewControl { get { return metadataViewControl; } }

        public MainScreen()
        {
            InitializeComponent();
            SetupManagers();
            _mainManager.LibraryManager.RefreshLibraryFromDatabase();

            hideMetadataToolStripMenuItem_Click(null, null);
        }

        // Setups the manager for the MainScreenManager
        private void SetupManagers()
        {
            _mainManager = new MainScreenManager(this);
        }

        #region Screen Lifecycle

        private void MainScreen_Load(object sender, EventArgs e)
        {

        }

        private void metadataViewControl_Load(object sender, EventArgs e)
        {

        }

        #endregion

        #region Context Menu

        #region View

        private void hideMetadataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // swap the toggle, and then set the value to the control
            Console.WriteLine("Toggled Metadata : " + !_metadataViewToggle);
            _metadataViewToggle = !_metadataViewToggle;
            metadataViewControl.Visible = _metadataViewToggle;
            tableLayoutPanel2.ColumnStyles[1].Width = _metadataViewToggle ? 135 : 0;
        }

        private void hideSearchAreaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Toggled Search Area : " + !_searchViewToggle);
            _searchViewToggle = !_searchViewToggle;
            basicSearchControl.Visible = _searchViewToggle;
            tableLayoutPanel1.RowStyles[1].Height = _searchViewToggle ? 25 : 0;
        }

        #endregion

        #region Settings

        private void setLibraryRootToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /** SET NEW LIBRARY ROOT **/

            // Start a folder browser dialog window to select root
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();

            // If the folder browser dialog was a success:
            if (result == DialogResult.OK)
            {
                // Get the selected path for the root
                string rootURL = fbd.SelectedPath;

                // Pass to main screen manager to interact with DB
                /* try { 
                     _mainManager.SetLibraryRoot(rootURL);
                     _mainManager.LibraryManager.RefreshLibraryFromDatabase();
                     MessageBox.Show("Successfully set new library root!",
                         "Zoodevio Video Library",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.None);
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine(ex.StackTrace);
                     MessageBox.Show("Failed to set new library root.",
                         "Zoodevio Video Library",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Exclamation);
                     throw ex;
                 } */

                _mainManager.SetLibraryRoot(rootURL);
                _mainManager.LibraryManager.RefreshLibraryFromDatabase();
            }
        }

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /** OPEN PREFERENCES DIALOG **/
            PreferenceScreen prefs = new PreferenceScreen();
            prefs.StartPosition = FormStartPosition.CenterParent;
            prefs.ShowDialog(); 

        }

        private void customTagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CustomTagsScreen tags = new CustomTagsScreen();
            tags.StartPosition = FormStartPosition.CenterParent;
            tags.ShowDialog();
        }

        #endregion

        #region Debug

        private void forceFolderHiarchyRefreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _mainManager.LibraryManager.RefreshLibraryFromDatabase();
        }

        #endregion

        #endregion

    }
}
