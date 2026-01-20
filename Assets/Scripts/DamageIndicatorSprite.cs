using UnityEngine;

public class DamageIndicatorSprite : MonoBehaviour
{
    public Sprite[] stages;

    public void UpdateSprite(float progress)
    {
        int stageIndex = Mathf.RoundToInt(progress * (stages.Length - 1));
        GetComponent<SpriteRenderer>().sprite = stages[stageIndex];
    }
}
