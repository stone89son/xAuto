using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xAuto.Core.Enums
{
    /// <summary>
    /// Enum for all supported automation actions
    /// </summary>
    public enum ActionType
    {
        Click,
        SetText,
        Check,
        SelectRadio,
        SelectTab,
        SelectComboBoxItem,
        WaitUntilTextAppears,
        WaitUntilVisible,
        WaitUntilGone,
        Sleep
    }
}
