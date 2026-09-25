using UnityEngine;

namespace Hwanjo.ElementLab
{
    public enum TraceResponse { Keep, End, Transport, Freeze, Thaw, Unsupported }
    public static class TraceSupport
    {
        public static TraceResponse Get(Element current, bool solidIce, Element incoming)
        {
            if (current == incoming || solidIce && incoming == Element.Water) return TraceResponse.Keep;
            if (current == Element.Fire) return incoming == Element.Wind ? TraceResponse.Transport : TraceResponse.End;
            if (current == Element.Water) return incoming == Element.Wind ? TraceResponse.Transport : incoming == Element.Ice ? TraceResponse.Freeze : TraceResponse.Unsupported;
            if (current == Element.Ice && incoming == Element.Fire) return solidIce ? TraceResponse.Thaw : TraceResponse.End;
            return TraceResponse.Unsupported;
        }
        public static string Description(Element current, bool solidIce, Element incoming)
        {
            switch (Get(current, solidIce, incoming))
            {
                case TraceResponse.Keep: return "유지 · 갱신 없음";
                case TraceResponse.End: return "소멸";
                case TraceResponse.Transport: return "전달체 1개";
                case TraceResponse.Freeze: return "얼음 구조로 변환";
                case TraceResponse.Thaw: return "해동 → 물 검흔";
                default: return solidIce && incoming == Element.Wind ? "고정형 · 이동 불가" : "이번 버전 미지원";
            }
        }
        public static Element FromEffect(Effect effect) => effect == Effect.Heat ? Element.Fire : effect == Effect.Moisture ? Element.Water : effect == Effect.Cold ? Element.Ice : Element.Wind;
    }
    public static class LabKorean
    {
        public static readonly string[] Elements = { "불", "물", "바람", "얼음" };
        public static string EffectName(Effect effect) => effect == Effect.Heat ? "열" : effect == Effect.Moisture ? "수분" : effect == Effect.Cold ? "냉기" : "바람 충격";
        public static string State(TargetState state) => (state.Liquid ? "물" : state.Moisture == Moisture.Dry ? "건조" : state.Moisture == Moisture.Wet ? "젖음" : "보통") + (state.Burning ? " · 연소" : "") + (state.Frozen ? " · 동결" : "");
        public static string Reason(string reason)
        {
            if (string.IsNullOrEmpty(reason)) return "";
            if (reason.Contains("SameElement") || reason.Contains("Already") || reason.Contains("Keep")) return "이미 적용 중: 중첩·수명 갱신 없음";
            if (reason.Contains("CreatingAction")) return "생성 공격으로 재타격 불가";
            if (reason.Contains("NeedsMoisture")) return "조건 불충족: 동결에 필요한 수분이 없음";
            if (reason.Contains("MaterialMissing")) return "조건 불충족: 전달할 불씨가 없음";
            if (reason.Contains("Cooldown")) return "재적용 대기 중";
            if (reason.Contains("RehitBlocked")) return "같은 공격의 중복 적용 차단";
            if (reason.Contains("NoTrait")) return "해당 반응 특성 없음";
            if (reason.Contains("Immovable")) return "고정형: 이동 불가";
            if (reason.Contains("Inactive")) return "소실된 대상";
            if (reason.Contains("Frozen")) return "동결 상태에서는 적용 불가";
            if (reason.Contains("FuelExpired")) return "연료 소진";
            if (reason == "Reset") return "초기화";
            return "이번 프로토타입에서 미지원";
        }
        public static string ReactionText(Reaction reaction) => State(reaction.Before) + " → " + State(reaction.After) + (reaction.Reason.Length > 0 ? " · " + Reason(reaction.Reason) : " · " + ConsequenceName(reaction.Consequence));
        public static string ConsequenceName(Consequence c)
        {
            string[] names = { "변화 없음", "해동", "건조", "점화", "소화", "젖음", "동결", "밀림", "회전", "전달" };
            return names[(int)c];
        }
    }
}
