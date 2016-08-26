using UnityEngine;
using UnityEngine.UI;

namespace BattleUI
{
public class SkillNamePanel : MonoBehaviour
{
    GameObject text;
    GameObject background;
    GameObject background2;

    void Awake()
    {
        text = transform.Find("Text").gameObject;
        background = transform.Find("Background").gameObject;
        background2 = transform.Find("Background2").gameObject;
    }

    void Start()
    {
        Hide();
    }

    public void Hide()
    {
        text.GetComponent<Text>().text = "";
        background.GetComponent<Image>().enabled = false;
        background2.GetComponent<Image>().enabled = false;
    }

    public void Set(string skillName)
    {
        text.GetComponent<Text>().text = skillName;
        background.GetComponent<Image>().enabled = true;
        background2.GetComponent<Image>().enabled = true;
    }
}
}