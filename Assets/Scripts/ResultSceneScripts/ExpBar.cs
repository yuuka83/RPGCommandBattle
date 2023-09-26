using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExpBar : MonoBehaviour
{
    [SerializeField] private Image expBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI remainExpText;

    public IEnumerator SetExpSmooth(ExpPair expPair)
    {
        Debug.Log("old" + expPair.oldLevel + "new" + expPair.newLevel);
        float sumNextExp = 0;
        float sumGetExp = 0;
        float currentLevel = expPair.oldLevel;
        levelText.text = "Lv" + currentLevel.ToString();
        // ���̌o���l�S��
        for (int i = 0; i < expPair.nextExp.Count; i++)
        {
            sumNextExp += expPair.nextExp[i];
            sumGetExp += expPair.getExp[i];
        }

        for (int i = 0; i < expPair.getExp.Count; i++)
        {
            float changeAmount;
            float currentExp = expBar.fillAmount;
            float remainNextExp = expPair.nextExp[i];
            //��{��
            changeAmount = remainNextExp * Time.deltaTime;
            // Exp�o�[��1�t���[����changeAmount�����₷����
            // currentExp�����̃��x���̎擾�����o���l�ɂȂ�܂ŌJ��Ԃ�
            while (currentExp <= expPair.getExp[i])
            {
                currentExp += changeAmount;
                remainNextExp -= changeAmount;
                remainExpText.text = "����" + remainNextExp.ToString("f0");
                expBar.fillAmount = (expPair.nextExp[i] - remainNextExp) / expPair.nextExp[i];
                yield return new WaitForEndOfFrame();
            }
            expBar.fillAmount = expPair.getExp[i] / expPair.nextExp[i];

            if (expBar.fillAmount >= 1)
            {
                expBar.fillAmount = 0;
                currentLevel += 1;
                levelText.text = "Lv" + currentLevel.ToString();
            }
        }
        currentLevel = expPair.newLevel;
        levelText.text = "Lv" + currentLevel.ToString();
        WaitForEndOfFrame coroutine = new WaitForEndOfFrame();
        yield return coroutine;
    }
}