using System.Collections.Generic;

namespace MMBGame
{
    public static class CommandActionInputRequirements
    {
        private static readonly HashSet<string> VALUE_ACTIONS = new HashSet<string>
        {
            "AidAction",
            "RequisitionAction",
            "LoanAction",
            "벌금",
            "매수",
            "제후국",
            "대민지원",
            "여론조작"
        };

        public static bool RequiresValue(string actionName)
        {
            return !string.IsNullOrEmpty(actionName) && VALUE_ACTIONS.Contains(actionName);
        }
    }
}
