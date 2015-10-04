using System;
using System.Linq;
using System.Windows.Forms;
using SimpleDataAccessLayer.Common.config.models;

namespace SimpleDataAccessLayer.Common.wizard
{
    public partial class ModelWizard : Form
    {
        private readonly DalConfig _config;

        internal DalConfig Config => _config;

        public ModelWizard(DalConfig config)
        {
            _config = config;
            InitializeComponent();
            VisibleTab_Changed(null, null);
            foreach (TabPage tabPage in tabContainer.TabPages)
            {
                tabPage.Controls.OfType<IUseDalConfig>().FirstOrDefault()?.InitializeFromDalConfig(_config);
            }
        }

        private void Back_Click(object sender, EventArgs e)
        {
            tabContainer.SelectedIndex--;
        }

        private void Next_Click(object sender, EventArgs e)
        {
            IValidateInput tabForm = null;
            foreach (var control in tabContainer.SelectedTab.Controls.OfType<IValidateInput>())
            {
                tabForm = control;
            }

            if (tabForm == null)
                return;

            var validationResponse = tabForm.ValidateInput();

            if (!string.IsNullOrWhiteSpace(validationResponse))
            {
                MessageBox.Show(validationResponse, "Error on the page", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                tabContainer.SelectedTab.Controls.OfType<IUseDalConfig>().FirstOrDefault()?.UpdateDalConfig(_config);
                tabContainer.SelectedIndex++;
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            //Close();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            foreach (TabPage tabPage in tabContainer.TabPages)
            {
                tabPage.Controls.OfType<IUseDalConfig>().FirstOrDefault()?.UpdateDalConfig(_config);
            }
        }

        private void VisibleTab_Changed(object sender, EventArgs e)
        {
            Back.Enabled = tabContainer.SelectedIndex > 0;
            Text = "Model Designer - " + tabContainer.SelectedTab.Text;
            OK.Visible = tabContainer.SelectedIndex == tabContainer.TabCount - 1;
            Next.Visible = tabContainer.SelectedIndex < tabContainer.TabCount - 1;
            tabContainer.SelectedTab.Controls.OfType<IRefreshTabOnActivate>().FirstOrDefault()?.RefreshTabOnActivate();

        }
    }
}
