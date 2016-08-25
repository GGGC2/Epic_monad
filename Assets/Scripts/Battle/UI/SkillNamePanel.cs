using UnityEngine;
using UnityEngine.UI;

namespace BattleUI
{
public class SkillNamePanel : MonoBehaviour
{
    GameObject text;
    GameObject background;

    void Awake()
    {
        text = transform.Find("Text").gameObject;
        background = transform.Find("Background").gameObject;
    }

    public void Hide()
    {
        text.GetComponent<Text>().text = "";
        background.GetComponent<Image>().enabled = false;
    }

    public void Set(string skillName)
    {
        text.GetComponent<Text>().text = skillName;
        background.GetComponent<Image>().enabled = true;
    }
}
}