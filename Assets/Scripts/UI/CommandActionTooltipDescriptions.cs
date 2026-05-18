using System.Collections.Generic;

namespace MMBGame
{
    public static class CommandActionTooltipDescriptions
    {
        private static readonly Dictionary<string, string> DESCRIPTIONS = new Dictionary<string, string>
        {
            { "SpecialTax", "조건 : 없음\n기능 : 선택된 기물의 턴 당 세금 +100%, 지지도 +5" },
            { "TaxExemption", "조건 : 없음\n기능 : 선택된 기물의 턴 당 세금 면제, 지지도 -5" },
            { "AidAction", "조건 : 없음\n비용 : 금화 지불\n기능 : 선택된 아군 기물 지지도 증가" },
            { "RequisitionAction", "조건 : 없음\n획득 : 금화 획득\n기능 : 선택된 아군 기물 지지도 감소" },
            { "LoanAction", "조건 : 사채 후보 기물 필요\n비용 : 금화 지불\n기능 : 후보 기물 지지도 증가, 이후 원금+이자 회수" },
            { "RearDeployAction", "조건 : 공격받지 않는 보드 위 아군 기물\n기능 : 후방 배치, 지지도 +30, 턴 수입 증가, 세금 +100%" },
            { "FrontDeployAction", "조건 : 후방 배치된 아군 기물\n기능 : 전방 복귀, 지지도 -50, 턴 수입 감소" },
            { "BarricadeAction", "조건 : 설치 가능 칸 필요\n기능 : 바리케이드 설치" },
            { "TrebuchetAction", "조건 : 설치 가능 칸 필요\n기능 : 트레뷰셋 설치" },
            { "BombardAction", "조건 : 트레뷰셋 및 전방 대상 필요\n기능 : 지정 대상 포격" },
            { "OutpostAction", "조건 : 체크 상태 아님\n기능 : 다음 군사 페이즈에 전초기지 효과 적용" },
            { "DivinePowerAction", "조건 : 체크 상태 아님\n기능 : 다음 군사 페이즈에 신의 힘 효과 적용" },
            { "MiracleAction", "조건 : 체크 상태 아님\n기능 : 다음 군사 페이즈에 기적 효과 적용" },
            { "MilitaryExemptionAction", "조건 : 체크 상태 아님\n기능 : 다음 군사 페이즈에 군법면제 효과 적용" },
            { "RoadPlanAction", "조건 : 인접한 아군 후방 3열 타일 2개\n기능 : 두 타일 사이 도로 생성, 사용 후 제거" },
            { "정찰", "조건 : 없음\n기능 : 상대의 최근 행동 3개 공개" },
            { "제후국", "조건 : 적 기물 선택\n비용 : 입력값 x3 금화\n기능 : 반란 가중치 증가, 반란 판정 3회" },
            { "심문", "조건 : 아군 기물 선택\n기능 : 선택 기물 정보 획득" },
            { "매수", "조건 : 배신하지 않은 적 기물 선택\n비용 : 입력값 x3 금화\n기능 : 배신 가중치 증가(입력값/기본세금), 배신 판정 3회" },
            { "정보", "조건 : 배신한 적 기물 선택\n기능 : 선택 기물을 정보 대상 지정" },
            { "배신 - 접촉", "조건 : 배신한 적 기물 선택\n기능 : 선택 기물 정보 획득" },
            { "배신 - 정보", "조건 : 배신한 적 기물 선택\n기능 : 선택 기물을 정보 대상 지정" },
            { "배신 - 실책", "조건 : 배신한 적 비숍 선택\n기능 : 지지도 -5, 수락 가중치 +5" },
            { "배신 - 파벌", "조건 : 배신한 적 퀸 선택\n기능 : 지지도 -8, 반란 가중치 +1" },
            { "배신 - 암살", "조건 : 배신한 적 기물 선택\n기능 : 닿을 수 있는 같은 색 비배신 기물 암살 시도" },
            { "암살", "조건 : 적 기물 선택, 교차 공격 경로 필요\n기능 : 선택된 적 기물 제거" },
            { "선전", "조건 : 없음\n기능 : 아군 전체 지지도 증가" },
            { "대민지원", "조건 : 없음\n비용 : 금화 지불\n기능 : 아군 전체 지지도 증가" },
            { "방문", "조건 : 아군 기물 선택\n기능 : 선택된 기물 지지도 +3" },
            { "벌금", "조건 : 아군 기물 선택\n획득 : 금화 획득\n기능 : 선택된 기물 지지도 감소, 형벌 카운터 초기화" },
            { "처형", "조건 : 아군 기물 선택\n기능 : 선택된 기물 제거, 아군 전체 지지도 및 명예 감소" },
            { "선동", "조건 : 없음\n기능 : 적군 전체 지지도 감소" },
            { "여론조작", "조건 : 없음\n비용 : 금화 지불\n기능 : 아군 전체 지지도 감소" }
        };

        public static string Resolve(string actionName)
        {
            if (!string.IsNullOrEmpty(actionName) && DESCRIPTIONS.TryGetValue(actionName, out string description))
            {
                return description;
            }

            return "조건 : 확인 필요\n기능 : " + actionName;
        }
    }
}
