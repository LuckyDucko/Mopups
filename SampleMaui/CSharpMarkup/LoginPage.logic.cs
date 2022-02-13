using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui;
using Microsoft.Maui.Controls;

using Mopups.Pages;

namespace SampleMaui.CSharpMarkup
{
    public partial class LoginPage : PopupPage
    {
        public LoginPage()
        {
            BuildContent();
           
        }

        protected override bool OnBackgroundClicked()
        {
            return base.OnBackgroundClicked();
        }
    }
}
