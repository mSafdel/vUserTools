using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Diagnostics;

namespace vUserTools
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                PrincipalContext userActiveDirecory = new PrincipalContext(ContextType.Domain, "...Your Domain Name...");
                UserPrincipal userName = new UserPrincipal(userActiveDirecory);
                PrincipalSearcher searchObj = new PrincipalSearcher(userName);

                foreach (UserPrincipal result in searchObj.FindAll())
                {
                    if (result.DisplayName != null)
                    {
                        DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;

                        if (IsActive(de))
                        {
                            string strUserName = result.DisplayName;
                            string strUserId = de.Properties["samAccountName"].Value.ToString();
                            dgvUsers.Rows.Add(strUserName, strUserId);
                        }
                    }
                }

                dgvUsers.Sort(dgvUsers.Columns["colUserId"], ListSortDirection.Ascending);
                searchObj.Dispose();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchValue = txtSearch.Text.Trim().ToLower();
            int iSearchLen = searchValue.Length;

            //Debug.Print(iSearchLen.ToString());
            try
            {
                foreach (DataGridViewRow row in dgvUsers.Rows)
                {
                    string user = row.Cells[1].Value.ToString().ToLower();
                    //Debug.Print(user + " - " + user.Length);

                    if (user.Length >= iSearchLen)
                    {
                        if (user.Substring(0, iSearchLen).Equals(searchValue))
                        {
                            row.Selected = true;
                            dgvUsers.FirstDisplayedScrollingRowIndex = dgvUsers.SelectedRows[0].Index;
                            break;
                        }
                        else
                            dgvUsers.ClearSelection();

                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void btnUnlock_Click(object sender, EventArgs e)
        {
            try
            {
                 if (dgvUsers.SelectedRows != null)
                {
                    Debug.Print(dgvUsers.SelectedRows[0].Cells[1].ToString());
                    string strSelectedUser = dgvUsers.SelectedRows[0].Cells[1].Value.ToString();

                    PrincipalContext userActiveDirecory = new PrincipalContext(ContextType.Domain, "...Your Domain Name...");
                    UserPrincipal user = UserPrincipal.FindByIdentity(userActiveDirecory, IdentityType.SamAccountName, strSelectedUser);

                    if (user != null)
                    {
                        Debug.Print(user.IsAccountLockedOut().ToString());
                        if (user.IsAccountLockedOut())
                        {
                            user.UnlockAccount();
                            MessageBox.Show("Account is Unlock!","vUserTools",MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        private bool IsActive(DirectoryEntry de)
        {
            if (de.NativeGuid == null) return false;

            int flags = (int)de.Properties["userAccountControl"].Value;

            return !Convert.ToBoolean(flags & 0x0002);
        }
    }
}
