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
#endregion

namespace Cnas.Siui.Cnp2Cid
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            Application.Run( new Cnp2CidForm() );
        }
    }
}
