using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveUI : MonoBehaviour
{
    [SerializeField] private Image bossImage;
    [SerializeField] private TextMeshProUGUI bossNameTMP;
    [SerializeField] private TextMeshProUGUI bossDescriptionTMP;
    [SerializeField] private TextMeshProUGUI bossCrimesTMP;

    public void OnEnable()
    {
        EnemyDetailsSO bossDetails = GameManager.Instance.GetBossEnemyDetails();
        
        if (bossDetails == null)
        {
            Debug.LogWarning("No boss enemy details found for this level");
            return;
        }
        
        BountyDetailsSO bountyDetails = bossDetails.bountyDetails;
        
        if (bountyDetails == null)
        {
            Debug.LogWarning("No bounty details found for this boss");
            return;
        }

        bossImage.sprite = bountyDetails.portraitSprite;
        bossNameTMP.SetText(bountyDetails.bountyName.ToUpper());
        bossDescriptionTMP.SetText(bountyDetails.description);
        bossCrimesTMP.SetText(bountyDetails.crimes);
    }
}