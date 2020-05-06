using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace SWIChallenge.Extensions
{
    public static class ControlExtensions
    {
        public static void Clear(this TextBlock ctr)
            => ctr.Text = string.Empty;
    }
}
