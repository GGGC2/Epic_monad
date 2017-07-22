using System.Collections;
using Enums;

namespace Battle.Skills {
    public class Eugene_2_l_SkillLogic : BaseSkillLogic {   //statusEffect에 스킬에 붙은 패시브 효과를 저장할 방법이 (현재로썬) 없기 때문에,
                                                            //스킬에 붙은 패시브 효과에 대한 로직도 그 스킬의 SkillLogic에 만들어야 함.
        public override IEnumerator TriggerStatusEffectAtActionEnd(Unit target, StatusEffect statusEffect) {   //순수한 물 skillLogic
            if (statusEffect.fixedElem.actuals.Count >= 2) {    //순수한 물 스킬을 배웠을 경우
                /* 기본 힐량을 a라 하고 배수를 1.2라 할 때, 초기엔 statusEffect의 각 ActualElement의 amount가 a, 1.2로 설정되어있음.
                 * 물 타일로 이동할 때는 각각의 amount를 1.2a와 1/1.2 로 만듦.
                 * 타일의 속성이 물속성인지의 여부가 바뀔 때마다 힐량에 배수를 곱하고, 배수를 1/배수로 만들면 됨. 
                */
                Element tileElement = target.GetTileUnderUnit().GetTileElement();
                float healAmount = statusEffect.GetAmount(0);
                float coefficient = statusEffect.GetAmount(1);
                bool isCurrentTileWater = (tileElement == Element.Water);
                bool isLastTileWater = (coefficient < 1);
                if (isCurrentTileWater != isLastTileWater) {    //물 속성 타일에서 아닌 타일로 옮겨갔을 경우, 혹은 그 반대의 경우
                    statusEffect.SetAmount(0, healAmount * coefficient);
                    statusEffect.SetAmount(1, 1.0f / coefficient);
                }
            }
            yield return null;
        }
        public override void TriggerStatusEffectAtPhaseStart(Unit target, StatusEffect statusEffect) { //청명수의 은총 skillLogic
            if (statusEffect.fixedElem.actuals.Count >= 3) {    //청명수의 은총 스킬을 배웠을 경우
                if(target.GetTileUnderUnit().GetTileElement() == Element.Water)
                    target.RemoveStatusEffect(statusEffect.GetCaster(), StatusEffectCategory.Debuff, 1);
            }
        }
    }
}