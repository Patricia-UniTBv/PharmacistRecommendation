#region Copyright ©2011-2013, SIVECO Romania SA - All Rights Reserved
// ======================================================================
// Copyright ©2011-2013 SIVECO Romania SA - All Rights Reserved
// ======================================================================
// This file and its contents are protected by Romanian and International
// copyright laws. Unauthorized reproduction and/or distribution of all
// or any portion of the code contained herein is strictly prohibited
// and will result in severe civil and criminal penalties.
// Any violations of this copyright will be prosecuted
// to the fullest extent possible under law.
// ======================================================================
// THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.
// ======================================================================
#endregion

#region References
using System;
using System.Windows.Forms;
using Cnas.Siui.CidGen;
using Cnas.Siui.Cnp2Cid.Properties;
#endregion

namespace Cnas.Siui.Cnp2Cid
{
    public partial class Cnp2CidForm : Form
    {
        #region Constructor
        /// <summary>
        /// Creates new instance of class <see cref="Cnp2CidForm"/>
        /// </summary>
        public Cnp2CidForm()
        {
            this.InitializeComponent();
            this.Text += string.Format( " [v{0}]", Application.ProductVersion );
        }
        #endregion

        #region Methods
        /// <summary>
        /// Generates a cryptographic hashing for a given valid CNP ( card code )
        /// </summary>
        private void GenerateCid()
        {
            errorProvider.Clear();
            string message = null;

            var cnp = this.textCnp.Text.Trim();

            if( cnp == string.Empty )
            {
                message = Resources.RequiredPid;
            }
            else
            {
                if( PidUtils.IsValid( cnp ) )
                {
                    textCid.Text = CryptoHash.GetCidHash( cnp );
                    if( this.textCid.CanFocus )
                    {
                        this.textCid.Focus();
                    }
                }
                else
                {
                    message = Resources.InvalidPid;
                }
            }

            if( message != null )
            {
                textCid.Text = string.Empty;
                this.ShowMessage( message, textCnp );
            }
        }

        /// <summary>
        /// Shows an error visual message box and marks any given control
        /// </summary>
        /// <param name="message">Error Message</param>
        /// <param name="control">The UI Control generating the error</param>
        private void ShowMessage( string message, Control control = null )
        {
            if( control != null )
            {
                errorProvider.SetError( control, message );
            }

            MessageBox.Show( message, Resources.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
        }
        #endregion

        #region Event-Handlers
        private void btnGenerate_Click( object sender, EventArgs e )
        {
            this.GenerateCid();
        }

        private void textCnp_KeyPress( object sender, KeyPressEventArgs e )
        {
            // force only numeric input
            if( e.KeyChar > 31 && ( e.KeyChar < '0' || e.KeyChar > '9' ) )
            {
                e.Handled = true;
            }
        }
        #endregion
    }
}