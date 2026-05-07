namespace MMBGame
{
    public struct ActionResultContext
    {
        public string actionName;
        public string targetName;
        public int inputValue;
        public int supportDelta;
        public int goldDelta;
        public int goldPerTurnDelta;
        public int taxModifierBefore;
        public int taxModifierAfter;
        public int targetFile;
        public int targetRank;
    }

    public static class ActionResultText
    {
        public static string Resolve(string actionName, ActionResultContext context)
        {
            switch (actionName)
            {
                case "SpecialTax":
                    return context.targetName + "의 세금이 일시적으로 2배가 되었으며,\n지지도가 " + FormatDelta(context.supportDelta) + " 변했습니다.";
                case "TaxExemption":
                    return context.targetName + "의 세금이 이번 턴 면제되었으며,\n지지도가 " + FormatDelta(context.supportDelta) + " 변했습니다.";
                case "AidAction":
                    return context.targetName + "에게 " + context.inputValue + " 골드를 지원했습니다.\n지지도 " + FormatDelta(context.supportDelta) + ", 보유 골드 " + FormatDelta(context.goldDelta) + ".";
                case "RequisitionAction":
                    return context.targetName + "에게서 " + context.inputValue + " 골드를 징발했습니다.\n지지도 " + FormatDelta(context.supportDelta) + ", 보유 골드 " + FormatDelta(context.goldDelta) + ".";
                case "LoanAction":
                    return context.targetName + "에게 " + context.inputValue + " 골드를 사채로 빌려주었습니다.\n지지도 " + FormatDelta(context.supportDelta) + ", 보유 골드 " + FormatDelta(context.goldDelta) + ".";
                case "RearDeployAction":
                    return context.targetName + "을 후방 배치했습니다.\n지지도 " + FormatDelta(context.supportDelta) + ", 턴 수입 " + FormatDelta(context.goldPerTurnDelta) + ".";
                case "FrontDeployAction":
                    return context.targetName + "을 전방 복귀시켰습니다.\n지지도 " + FormatDelta(context.supportDelta) + ", 턴 수입 " + FormatDelta(context.goldPerTurnDelta) + ".";
                case "BarricadeAction":
                    return "바리케이드 설치 명령을 예약했습니다.";
                case "RoadPlanAction":
                    return "도로계획을 실행했습니다.";
                case "TrebuchetAction":
                    return "트레뷰셋 설치 명령을 예약했습니다.";
                case "BombardAction":
                    return "투석 명령을 예약했습니다.";
                case "OutpostAction":
                    return context.targetName + "에게 전초기지 효과를 예약했습니다.";
                case "DivinePowerAction":
                    return context.targetName + "에게 신의 힘 효과를 예약했습니다.";
                case "MiracleAction":
                    return context.targetName + "에게 기적 효과를 예약했습니다.";
                case "MilitaryExemptionAction":
                    return context.targetName + "에게 군법면제 효과를 예약했습니다.";
                case "정찰":
                    return "정찰을 실행했습니다.";
                case "제후국":
                    return context.targetName + "에게 제후국 행동을 실행했습니다.";
                case "심문":
                    return context.targetName + "에게 심문을 실행했습니다.";
                case "매수":
                    return context.targetName + "에게 매수를 시도했습니다.";
                case "정보":
                    return context.targetName + "에게 정보 행동을 실행했습니다.";
                case "배신:접촉":
                    return context.targetName + "에게 배신 접촉을 실행했습니다.";
                case "배신:정보":
                    return context.targetName + "에게 배신 정보를 실행했습니다.";
                case "배신:실책":
                    return context.targetName + "에게 배신 실책을 실행했습니다.";
                case "배신:파벌":
                    return context.targetName + "에게 배신 파벌을 실행했습니다.";
                case "배신:암살":
                    return context.targetName + "에게 배신 암살을 실행했습니다.";
                case "암살":
                    return context.targetName + "에게 암살을 실행했습니다.";
                case "선전":
                    return "선전을 실행했습니다.";
                case "대민지원":
                    return "대민지원을 실행했습니다.";
                case "방문":
                    return context.targetName + "을 방문했습니다.";
                case "벌금":
                    return context.targetName + "에게 벌금을 실행했습니다.";
                case "처형":
                    return context.targetName + "을 처형했습니다.";
                case "선동":
                    return "선동을 실행했습니다.";
                case "여론조작":
                    return "여론조작을 실행했습니다.";
                default:
                    return actionName + " 행동이 실행되었습니다.";
            }
        }

        private static string FormatDelta(int delta)
        {
            if (delta > 0)
            {
                return "+" + delta;
            }

            return delta.ToString();
        }
    }
}
